using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// A noteworthy processing or data condition that occurred during analysis.
//
// It states what happened to the analysis, never a judgement about the
// developer, and its presence never by itself determines profile completeness.
public sealed class ProfileWarning : ProfileCondition<ProfileWarningCode>
{
    public ProfileWarning(
        ProfileWarningCode code,
        ProfileConditionScope scope,
        IEnumerable<ProfileRepositoryReference>? affectedRepositories = null)
        : base(code, scope, affectedRepositories)
    {
    }
}
