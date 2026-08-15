using GitHubProof.Domain.Analysis.Models;

namespace GitHubProof.Domain.Analysis.Signals;

public interface ISignalEvaluator
{
    SignalDefinition Definition { get; }

    SignalResult Evaluate(RepositorySnapshot snapshot);
}
