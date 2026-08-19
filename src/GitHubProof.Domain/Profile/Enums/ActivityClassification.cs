namespace GitHubProof.Domain.Profile.Enums;

// Repository source-control activity classification, evaluated against the
// CurrentActivityWindow of one eligibility policy. It is a recency boundary
// only: repository age never participates. Archived is a separate repository
// condition, so Active + Archived is a valid combination.
public enum ActivityClassification
{
    Undetermined = 0,
    Active = 1,
    Historical = 2
}
