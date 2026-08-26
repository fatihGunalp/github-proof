using GitHubProof.Web.Profile.Runtime;
using Microsoft.Extensions.Options;
using Xunit;

namespace GitHubProof.Web.Tests;

// Milestone B launch-burst gate. These are simultaneous ARRIVALS, not real
// GitHub analyses. The admitted request is held behind a bounded fake
// downstream operation while every other arrival reaches the fail-fast
// admission decision, proving that excess demand is rejected rather than
// queued or sent to the provider.
public sealed class ProfileLaunchBurstTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task SimultaneousArrivals_KeepExecutionBoundedAndRejectExcessWithoutWaiting(
        int arrivalCount)
    {
        const int activeAnalysisCapacity = 1;

        ProfileExecutionAdmissionGuard guard = new(
            Options.Create(new ProfileAdmissionOptions
            {
                GlobalConcurrency = activeAnalysisCapacity,
                SameProfileConcurrency = 1
            }),
            TimeProvider.System);

        TaskCompletionSource start = NewSignal();
        TaskCompletionSource allArrivalsReady = NewSignal();
        TaskCompletionSource releaseDownstream = NewSignal();
        TaskCompletionSource allAdmissionDecisionsMade = NewSignal();
        BurstObservation observation = new(arrivalCount);

        Task<ArrivalOutcome>[] arrivals = Enumerable
            .Range(0, arrivalCount)
            .Select(index => Task.Run(async () =>
            {
                observation.RecordArrivalReady(allArrivalsReady);
                await start.Task;

                using ProfileAdmissionLease lease = guard.TryAcquire(
                    $"profile-{index}",
                    visitorAddress: null);

                observation.RecordAdmissionDecision(allAdmissionDecisionsMade);

                if (!lease.IsAdmitted)
                {
                    return new ArrivalOutcome(
                        Admitted: false,
                        lease.Rejection,
                        DownstreamStarted: false);
                }

                observation.EnterDownstream();

                try
                {
                    await releaseDownstream.Task;

                    return new ArrivalOutcome(
                        Admitted: true,
                        Rejection: null,
                        DownstreamStarted: true);
                }
                finally
                {
                    observation.ExitDownstream();
                }
            }))
            .ToArray();

        // Do not release the burst until every scheduled arrival is parked on
        // the same start signal. Without this readiness barrier, late
        // ThreadPool scheduling could turn the test into staggered arrivals.
        await allArrivalsReady.Task.WaitAsync(TimeSpan.FromSeconds(5));
        start.SetResult();

        // If the guard queued excess arrivals, this would time out because the
        // single admitted fake analysis deliberately remains in progress.
        await allAdmissionDecisionsMade.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(activeAnalysisCapacity, observation.MaxActiveDownstream);
        Assert.False(Task.WhenAll(arrivals).IsCompleted);

        releaseDownstream.SetResult();

        ArrivalOutcome[] outcomes = await Task
            .WhenAll(arrivals)
            .WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(activeAnalysisCapacity, outcomes.Count(outcome => outcome.Admitted));
        Assert.Equal(
            arrivalCount - activeAnalysisCapacity,
            outcomes.Count(outcome =>
                outcome.Rejection == ProfileAdmissionRejection.GlobalCapacityBusy));
        Assert.Equal(
            activeAnalysisCapacity,
            outcomes.Count(outcome => outcome.DownstreamStarted));
        Assert.DoesNotContain(
            outcomes,
            outcome => !outcome.Admitted
                && outcome.Rejection != ProfileAdmissionRejection.GlobalCapacityBusy);
        Assert.Equal(0, observation.ActiveDownstream);
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private sealed record ArrivalOutcome(
        bool Admitted,
        ProfileAdmissionRejection? Rejection,
        bool DownstreamStarted);

    private sealed class BurstObservation(int arrivalCount)
    {
        private int remainingArrivalsReady = arrivalCount;
        private int remainingAdmissionDecisions = arrivalCount;
        private int activeDownstream;
        private int maxActiveDownstream;

        public int ActiveDownstream => Volatile.Read(ref activeDownstream);

        public int MaxActiveDownstream => Volatile.Read(ref maxActiveDownstream);

        public void RecordArrivalReady(TaskCompletionSource completed)
        {
            if (Interlocked.Decrement(ref remainingArrivalsReady) == 0)
            {
                completed.TrySetResult();
            }
        }

        public void RecordAdmissionDecision(TaskCompletionSource completed)
        {
            if (Interlocked.Decrement(ref remainingAdmissionDecisions) == 0)
            {
                completed.TrySetResult();
            }
        }

        public void EnterDownstream()
        {
            int current = Interlocked.Increment(ref activeDownstream);
            int observed = Volatile.Read(ref maxActiveDownstream);

            while (current > observed)
            {
                int previous = Interlocked.CompareExchange(
                    ref maxActiveDownstream,
                    current,
                    observed);

                if (previous == observed)
                {
                    return;
                }

                observed = previous;
            }
        }

        public void ExitDownstream() => Interlocked.Decrement(ref activeDownstream);
    }
}
