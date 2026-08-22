using Microsoft.Extensions.Options;

namespace GitHubProof.Web.Profile.Runtime;

public interface IProfileExecutionDeadlineFactory
{
    IProfileExecutionDeadline Create(CancellationToken callerCancellationToken);
}

public interface IProfileExecutionDeadline : IDisposable
{
    CancellationToken ExecutionToken { get; }

    bool IsExpired { get; }

    bool TryComplete();
}

internal sealed class ProfileExecutionDeadlineFactory : IProfileExecutionDeadlineFactory
{
    private readonly ProfileExecutionDeadlineOptions options;
    private readonly TimeProvider timeProvider;

    public ProfileExecutionDeadlineFactory(
        IOptions<ProfileExecutionDeadlineOptions> options,
        TimeProvider timeProvider)
    {
        this.options = options?.Value
            ?? throw new ArgumentNullException(nameof(options));
        this.timeProvider = timeProvider
            ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public IProfileExecutionDeadline Create(
        CancellationToken callerCancellationToken) =>
        new ProfileExecutionDeadline(
            callerCancellationToken,
            options.Timeout,
            timeProvider);
}

internal sealed class DisabledProfileExecutionDeadlineFactory
    : IProfileExecutionDeadlineFactory
{
    public static DisabledProfileExecutionDeadlineFactory Instance { get; } = new();

    private DisabledProfileExecutionDeadlineFactory()
    {
    }

    public IProfileExecutionDeadline Create(
        CancellationToken callerCancellationToken) =>
        new ProfileExecutionDeadline(
            callerCancellationToken,
            timeout: null,
            TimeProvider.System);
}

internal sealed class ProfileExecutionDeadline : IProfileExecutionDeadline
{
    private readonly object gate = new();
    private readonly CancellationTokenSource deadlineSource;
    private readonly CancellationTokenSource executionSource;
    private readonly ITimer? timer;
    private bool isExpired;
    private bool isComplete;
    private bool isDisposed;

    public ProfileExecutionDeadline(
        CancellationToken callerCancellationToken,
        TimeSpan? timeout,
        TimeProvider timeProvider)
    {
        deadlineSource = new CancellationTokenSource();
        executionSource = CancellationTokenSource.CreateLinkedTokenSource(
            callerCancellationToken,
            deadlineSource.Token);

        if (timeout is not null)
        {
            timer = timeProvider.CreateTimer(
                static state => ((ProfileExecutionDeadline)state!).Expire(),
                this,
                timeout.Value,
                Timeout.InfiniteTimeSpan);
        }
    }

    public CancellationToken ExecutionToken => executionSource.Token;

    public bool IsExpired
    {
        get
        {
            lock (gate)
            {
                return isExpired;
            }
        }
    }

    // Linearizes terminal result creation against the owned deadline timer.
    // The caller builds the candidate result first, then calls this method; a
    // deadline that won before completion causes that candidate to be discarded.
    public bool TryComplete()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);

            if (isExpired)
            {
                return false;
            }

            isComplete = true;
            timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return true;
        }
    }

    private void Expire()
    {
        lock (gate)
        {
            if (isComplete || isDisposed || isExpired)
            {
                return;
            }

            isExpired = true;
            deadlineSource.Cancel();
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            isComplete = true;
            timer?.Dispose();
            executionSource.Dispose();
            deadlineSource.Dispose();
        }
    }
}
