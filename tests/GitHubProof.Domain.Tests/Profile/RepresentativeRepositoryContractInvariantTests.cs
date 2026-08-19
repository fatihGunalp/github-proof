using System.Reflection;
using GitHubProof.Domain.Profile.Models;
using Xunit;

namespace GitHubProof.Domain.Tests.Profile;

// Guards the locked §11.1 Prohibited Fields boundary directly against the
// production types: representative-repository results must never gain a
// rank/score/quality/strength/confidence/capability/seniority/employability
// field, even by accident, in any future change to this area.
public sealed class RepresentativeRepositoryContractInvariantTests
{
    private static readonly string[] ProhibitedTerms =
    [
        "Rank",
        "Score",
        "Quality",
        "Strength",
        "Confidence",
        "Capability",
        "Seniority",
        "Employability"
    ];

    [Theory]
    [InlineData(typeof(RepresentativeRepositorySelection))]
    [InlineData(typeof(RepresentativeRepository))]
    [InlineData(typeof(RepresentativePatternSupport))]
    public void ContractShapes_NeverExposeAProhibitedField(Type contractShape)
    {
        PropertyInfo[] properties = contractShape.GetProperties(
            BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo property in properties)
        {
            Assert.DoesNotContain(
                ProhibitedTerms,
                term => property.Name.Contains(
                    term,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
