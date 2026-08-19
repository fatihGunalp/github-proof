using GitHubProof.Domain.Profile.Enums;
using GitHubProof.Domain.Profile.ValueObjects;

namespace GitHubProof.Domain.Profile.Models;

// A known semantic boundary on how far the profile result may safely be
// interpreted, given the evidence surface actually available.
//
// It describes the evidence surface. It is never a negative developer
// conclusion, and it never encodes capability, seniority or employability.
public sealed class ProfileLimitation : ProfileCondition<ProfileLimitationCode>
{
    public ProfileLimitation(
        ProfileLimitationCode code,
        ProfileConditionScope scope,
        IEnumerable<ProfileRepositoryReference>? affectedRepositories = null)
        : base(code, scope, affectedRepositories)
    {
    }
}
