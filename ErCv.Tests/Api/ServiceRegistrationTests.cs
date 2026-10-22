using ErCv.Web.Features.Analysis;
using ErCv.Web.Features.CoverLetters;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Features.Selection;
using ErCv.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ErCv.Tests.Api;

// ENG-01. Same collection as the other API tests: a host that stops closes the shared Chrome.
[Collection("Documents")]
[Trait("Category", "Api")]
public class ServiceRegistrationTests
{
    public static TheoryData<Type> DomainTypes => new()
    {
        typeof(JobDescriptionParser),
        typeof(RelevanceScorer),
        typeof(SummaryGenerator),
        typeof(PageConstraintHelper),
        typeof(CVSelector),
        typeof(MatchScoreCalculator),
        typeof(MissingSkillsAnalyzer),
        typeof(KeywordAnalyzer),
        typeof(ATSChecker),
        typeof(JdHighlighter),
        typeof(ProfileValidator),
        typeof(CoverLetterGenerator)
    };

    [Theory]
    [MemberData(nameof(DomainTypes))]
    public async Task Every_request_scope_shares_one_instance_of_each_domain_class(Type type)
    {
        using var app = new ApiApp();

        var first = await app.InScopeAsync(sp => Task.FromResult(sp.GetRequiredService(type)));
        var second = await app.InScopeAsync(sp => Task.FromResult(sp.GetRequiredService(type)));

        Assert.Same(first, second);
    }

    [Fact]
    public async Task The_generator_service_is_built_per_scope_by_the_container()
    {
        using var app = new ApiApp();

        var first = await app.InScopeAsync(sp => Task.FromResult(sp.GetRequiredService<CVGeneratorService>()));
        var second = await app.InScopeAsync(sp => Task.FromResult(sp.GetRequiredService<CVGeneratorService>()));

        Assert.NotSame(first, second);
    }
}
