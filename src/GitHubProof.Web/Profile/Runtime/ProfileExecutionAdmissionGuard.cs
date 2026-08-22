using System.Net;
using Microsoft.Extensions.Options;

namespace GitHubProof.Web.Profile.Runtime;

public interface IProfileExecutionAdmissionGuard
{
    ProfileAdmissionLease TryAcquire(
        string normalizedProfileLogin,
        IPAddress? visitorAddress);
}

public enum ProfileAdmissionRejection
{
    VisitorRateLimited,
    SameProfileBusy,
    GlobalCapacityBusy
}

// A successful lease owns both concurrency permits. Rejections own no permit.
// Disposal is idempotent so every Web terminal path can release in one using
// boundary without special-case cleanup.
public sealed class ProfileAdmissionLease : IDisposable
{
    private Action? release;

    private ProfileAdmissionLease(
        Action? release,
        ProfileAdmissionRejection? rejection,
        TimeSpan? retryAfter)
    {
        this.release = release;
        Rejection = rejection;
        RetryAfter = retryAfter;
    }

    public bool IsAdmitted => Rejection is null;

    public ProfileAdmissionRejection? Rejection { get; }

    public TimeSpan? RetryAfter { get; }

    internal static ProfileAdmissionLease Admitted(Action release) =>
        new(release, rejection: null, retryAfter: null);

    internal static ProfileAdmissionLease Rejected(
        ProfileAdmissionRejection rejection,
        TimeSpan? retryAfter = null) =>
        new(release: null, rejection, retryAfter);

    public void Dispose() => Interlocked.Exchange(ref release, null)?.Invoke();
}

// One process-local, fail-fast admission boundary. The lock linearizes the
// same-profile and global decisions so a same-profile collision is always a
// 409 candidate even when the global capacity is simultaneously full.
public sealed class ProfileExecutionAdmissionGuard : IProfileExecutionAdmissionGuard
{
    private const int CleanupInterval = 64;

    private readonly object sync = new();
    private readonly ProfileAdmissionOptions options;
    private readonly TimeProvider timeProvider;
    private readonly Dictionary<string, int> activeProfiles =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Queue<DateTimeOffset>> visitorAttempts =
        new(StringComparer.Ordinal);

    private int activeExecutions;
    private int attemptsUntilCleanup = CleanupInterval;

    public ProfileExecutionAdmissionGuard(
        IOptions<ProfileAdmissionOptions> options,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options.Value;
        this.timeProvider = timeProvider
            ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public ProfileAdmissionLease TryAcquire(
        string normalizedProfileLogin,
        IPAddress? visitorAddress)
    {
        if (string.IsNullOrWhiteSpace(normalizedProfileLogin))
        {
            throw new ArgumentException(
                "A normalized profile login is required.",
                nameof(normalizedProfileLogin));
        }

        lock (sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();

            ProfileAdmissionLease? visitorRejection =
                TryConsumeVisitorPermit(visitorAddress, now);

            if (visitorRejection is not null)
            {
                return visitorRejection;
            }

            activeProfiles.TryGetValue(normalizedProfileLogin, out int profileCount);

            if (profileCount >= options.SameProfileConcurrency)
            {
                return ProfileAdmissionLease.Rejected(
                    ProfileAdmissionRejection.SameProfileBusy);
            }

            if (activeExecutions >= options.GlobalConcurrency)
            {
                return ProfileAdmissionLease.Rejected(
                    ProfileAdmissionRejection.GlobalCapacityBusy);
            }

            activeExecutions++;
            activeProfiles[normalizedProfileLogin] = profileCount + 1;

            return ProfileAdmissionLease.Admitted(
                () => Release(normalizedProfileLogin));
        }
    }

    private ProfileAdmissionLease? TryConsumeVisitorPermit(
        IPAddress? visitorAddress,
        DateTimeOffset now)
    {
        if (!options.IsVisitorLimitEnabled)
        {
            return null;
        }

        if (visitorAddress is null)
        {
            throw new InvalidOperationException(
                "Visitor rate limiting is enabled, but RemoteIpAddress is unavailable.");
        }

        int permitLimit = options.VisitorPermitLimit!.Value;
        TimeSpan window = options.VisitorWindow!.Value;
        string visitorKey = visitorAddress.MapToIPv6().ToString();

        CleanupExpiredVisitorPartitions(now, window);

        if (!visitorAttempts.TryGetValue(visitorKey, out Queue<DateTimeOffset>? attempts))
        {
            attempts = new Queue<DateTimeOffset>();
            visitorAttempts.Add(visitorKey, attempts);
        }

        RemoveExpiredAttempts(attempts, now, window);

        if (attempts.Count >= permitLimit)
        {
            TimeSpan retryAfter = attempts.Peek() + window - now;

            return ProfileAdmissionLease.Rejected(
                ProfileAdmissionRejection.VisitorRateLimited,
                retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.FromSeconds(1));
        }

        // Visitor admission is intentionally consumed before the later
        // same-profile/global checks, so retry hammering cannot bypass it.
        attempts.Enqueue(now);
        return null;
    }

    private void CleanupExpiredVisitorPartitions(
        DateTimeOffset now,
        TimeSpan window)
    {
        attemptsUntilCleanup--;
        if (attemptsUntilCleanup > 0)
        {
            return;
        }

        attemptsUntilCleanup = CleanupInterval;

        foreach (string key in visitorAttempts.Keys.ToArray())
        {
            Queue<DateTimeOffset> attempts = visitorAttempts[key];
            RemoveExpiredAttempts(attempts, now, window);

            if (attempts.Count == 0)
            {
                visitorAttempts.Remove(key);
            }
        }
    }

    private static void RemoveExpiredAttempts(
        Queue<DateTimeOffset> attempts,
        DateTimeOffset now,
        TimeSpan window)
    {
        DateTimeOffset cutoff = now - window;

        while (attempts.TryPeek(out DateTimeOffset attemptedAt)
            && attemptedAt <= cutoff)
        {
            attempts.Dequeue();
        }
    }

    private void Release(string normalizedProfileLogin)
    {
        lock (sync)
        {
            int profileCount = activeProfiles[normalizedProfileLogin];

            if (profileCount == 1)
            {
                activeProfiles.Remove(normalizedProfileLogin);
            }
            else
            {
                activeProfiles[normalizedProfileLogin] = profileCount - 1;
            }

            activeExecutions--;
        }
    }
}

// Keeps direct unit construction of ProfileModel explicit and side-effect free.
// Production composition always replaces it with the singleton guard above.
public sealed class DisabledProfileExecutionAdmissionGuard
    : IProfileExecutionAdmissionGuard
{
    public static DisabledProfileExecutionAdmissionGuard Instance { get; } = new();

    private DisabledProfileExecutionAdmissionGuard()
    {
    }

    public ProfileAdmissionLease TryAcquire(
        string normalizedProfileLogin,
        IPAddress? visitorAddress) =>
        ProfileAdmissionLease.Admitted(static () => { });
}
