namespace GitHubProof.Domain.Profile.Models;

// Structural placeholder for the deferred RoleEvidenceAlignment capability.
//
// The capability is explicitly DEFERRED for Profile V1, and the contract that
// would define generic role taxonomy, role evidence definitions and alignment
// rules does not exist yet. This type therefore declares no members: it asserts
// nothing about what a role alignment contains, and deciding that shape is
// deferred P7 work rather than an implementation choice.
//
// It has no accessible constructor, so the deferral is enforced at compile
// time: a profile's RoleEvidenceAlignments collection can only ever be empty
// until a future contract defines the capability.
//
// An empty collection is a valid profile result. It is not negative evidence
// and it does not mean "no role match".
public sealed class RoleEvidenceAlignment
{
    private RoleEvidenceAlignment()
    {
    }
}
