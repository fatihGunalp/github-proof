using System.Net;
using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Options;
using Xunit;

namespace GitHubProof.Web.Tests;

public sealed class ProfileExecutionAdmissionGuardTests
{
    [Fact]
    public void TryAcquire_SameProfileCollisionWinsBeforeGlobalCapacity()
    {
        ProfileExecutionAdmissionGuard guard = CreateGuard(
            new ProfileAdmissionOptions
            {
                GlobalConcurrency = 1,
                SameProfileConcurrency = 1
            });
        using ProfileAdmissionLease first = guard.TryAcquire("octocat", null);

        using ProfileAdmissionLease collision = guard.TryAcquire("OCTOCAT", null);

        Assert.True(first.IsAdmitted);
        Assert.False(collision.IsAdmitted);
        Assert.Equal(ProfileAdmissionRejection.SameProfileBusy, collision.Rejection);
    }

    [Fact]
    public void TryAcquire_DifferentProfileIsRejectedWhenGlobalCapacityIsFull()
    {
        ProfileExecutionAdmissionGuard guard = CreateGuard(
            new ProfileAdmissionOptions
            {
                GlobalConcurrency = 1,
                SameProfileConcurrency = 1
            });
        using ProfileAdmissionLease first = guard.TryAcquire("octocat", null);

        using ProfileAdmissionLease second = guard.TryAcquire("hubot", null);

        Assert.False(second.IsAdmitted);
        Assert.Equal(ProfileAdmissionRejection.GlobalCapacityBusy, second.Rejection);
    }

    [Fact]
    public void Dispose_ReleasesBothConcurrencyPermitsExactlyOnce()
    {
        ProfileExecutionAdmissionGuard guard = CreateGuard(
            new ProfileAdmissionOptions());
        ProfileAdmissionLease first = guard.TryAcquire("octocat", null);

        first.Dispose();
        first.Dispose();

        using ProfileAdmissionLease second = guard.TryAcquire("octocat", null);
        Assert.True(second.IsAdmitted);
    }

    [Fact]
    public void TryAcquire_VisitorPermitIsConsumedBeforeLaterGlobalRejection()
    {
        ManualTimeProvider timeProvider = new();
        ProfileExecutionAdmissionGuard guard = CreateGuard(
            new ProfileAdmissionOptions
            {
                GlobalConcurrency = 1,
                SameProfileConcurrency = 1,
                VisitorPermitLimit = 1,
                VisitorWindow = TimeSpan.FromMinutes(10)
            },
            timeProvider);
        using ProfileAdmissionLease occupying = guard.TryAcquire(
            "octocat",
            IPAddress.Parse("192.0.2.1"));

        using ProfileAdmissionLease globalRejection = guard.TryAcquire(
            "hubot",
            IPAddress.Parse("192.0.2.2"));
        occupying.Dispose();
        using ProfileAdmissionLease visitorRejection = guard.TryAcquire(
            "hubot",
            IPAddress.Parse("192.0.2.2"));

        Assert.Equal(
            ProfileAdmissionRejection.GlobalCapacityBusy,
            globalRejection.Rejection);
        Assert.Equal(
            ProfileAdmissionRejection.VisitorRateLimited,
            visitorRejection.Rejection);
        Assert.Equal(TimeSpan.FromMinutes(10), visitorRejection.RetryAfter);
    }

    [Fact]
    public void TryAcquire_VisitorWindowExpiresWithoutQueueingOrResultReuse()
    {
        ManualTimeProvider timeProvider = new();
        ProfileExecutionAdmissionGuard guard = CreateGuard(
            new ProfileAdmissionOptions
            {
                VisitorPermitLimit = 1,
                VisitorWindow = TimeSpan.FromMinutes(3)
            },
            timeProvider);
        IPAddress visitor = IPAddress.Parse("2001:db8::1");

        using (ProfileAdmissionLease first = guard.TryAcquire("octocat", visitor))
        {
            Assert.True(first.IsAdmitted);
        }

        using ProfileAdmissionLease rejected = guard.TryAcquire("hubot", visitor);
        Assert.Equal(ProfileAdmissionRejection.VisitorRateLimited, rejected.Rejection);

        timeProvider.Advance(TimeSpan.FromMinutes(3));

        using ProfileAdmissionLease admitted = guard.TryAcquire("hubot", visitor);
        Assert.True(admitted.IsAdmitted);
    }

    [Fact]
    public void TryAcquire_EnabledVisitorLimitRequiresRemoteIpAddress()
    {
        ProfileExecutionAdmissionGuard guard = CreateGuard(
            new ProfileAdmissionOptions
            {
                VisitorPermitLimit = 1,
                VisitorWindow = TimeSpan.FromMinutes(1)
            });

        Assert.Throws<InvalidOperationException>(
            () => guard.TryAcquire("octocat", visitorAddress: null));
    }

    private static ProfileExecutionAdmissionGuard CreateGuard(
        ProfileAdmissionOptions options,
        TimeProvider? timeProvider = null) =>
        new(Options.Create(options), timeProvider ?? TimeProvider.System);

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset now =
            DateTimeOffset.Parse("2026-08-22T00:00:00Z");

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan duration) => now += duration;
    }
}
