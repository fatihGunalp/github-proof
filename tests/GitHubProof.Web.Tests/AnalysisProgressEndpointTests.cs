using System.Text.Json;
using GitHubProof.Web.Pages;
using GitHubProof.Web.Progress;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace GitHubProof.Web.Tests;

// GET /analysis-progress?run=<token>
// live-analysis-progress-runtime-contract.md §10.4, §10.5, §16.
public sealed class AnalysisProgressEndpointTests
{
    private static AnalysisProgressModel CreateModel(
        AnalysisProgressRegistry registry)
    {
        return new AnalysisProgressModel(registry)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public void Known_run_returns_target_active_and_completed_with_no_store()
    {
        AnalysisProgressRegistry registry = new(TimeProvider.System);
        Guid token = Guid.NewGuid();
        AnalysisProgressRun run = registry.TryBegin(
            token,
            AnalysisProgressTargetKind.Repository)!;
        run.CompleteCurrentStage();

        AnalysisProgressModel model = CreateModel(registry);
        IActionResult result = model.OnGet(token.ToString("D"));

        ContentResult content = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", content.ContentType);
        Assert.Equal("no-store", model.Response.Headers.CacheControl);

        using JsonDocument document = JsonDocument.Parse(content.Content!);
        JsonElement root = document.RootElement;
        Assert.True(root.GetProperty("known").GetBoolean());
        Assert.Equal("repository", root.GetProperty("target").GetString());
        Assert.Equal(
            "repository-public-data-read",
            root.GetProperty("active").GetString());
        Assert.Equal(
            new[] { "repository-target-validated" },
            root.GetProperty("completed").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void Terminal_run_serializes_a_null_active_stage()
    {
        AnalysisProgressRegistry registry = new(TimeProvider.System);
        Guid token = Guid.NewGuid();
        AnalysisProgressRun run = registry.TryBegin(
            token,
            AnalysisProgressTargetKind.Profile)!;
        run.CompleteCurrentStage();
        run.CompleteCurrentStage();
        run.CompleteCurrentStage();

        ContentResult content = Assert.IsType<ContentResult>(
            CreateModel(registry).OnGet(token.ToString("D")));

        using JsonDocument document = JsonDocument.Parse(content.Content!);
        JsonElement active = document.RootElement.GetProperty("active");
        Assert.Equal(JsonValueKind.Null, active.ValueKind);
        Assert.Equal(3, document.RootElement.GetProperty("completed").GetArrayLength());
    }

    [Fact]
    public void Unknown_valid_uuid_returns_known_false_only_with_no_store()
    {
        AnalysisProgressRegistry registry = new(TimeProvider.System);
        AnalysisProgressModel model = CreateModel(registry);

        IActionResult result = model.OnGet(Guid.NewGuid().ToString("D"));

        ContentResult content = Assert.IsType<ContentResult>(result);
        Assert.Equal("{\"known\":false}", content.Content);
        Assert.Equal("application/json", content.ContentType);
        Assert.Equal("no-store", model.Response.Headers.CacheControl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-uuid")]
    [InlineData("12345")]
    public void Missing_or_malformed_run_value_returns_http_400_with_an_empty_body(string? run)
    {
        AnalysisProgressRegistry registry = new(TimeProvider.System);
        AnalysisProgressModel model = CreateModel(registry);

        IActionResult result = model.OnGet(run);

        StatusCodeResult status = Assert.IsAssignableFrom<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, status.StatusCode);
        Assert.Equal("no-store", model.Response.Headers.CacheControl);
    }

    [Fact]
    public void A_braced_uuid_is_not_canonical_and_returns_400()
    {
        AnalysisProgressRegistry registry = new(TimeProvider.System);
        AnalysisProgressModel model = CreateModel(registry);

        IActionResult result = model.OnGet("{" + Guid.NewGuid().ToString() + "}");

        StatusCodeResult status = Assert.IsAssignableFrom<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, status.StatusCode);
    }

    [Fact]
    public void No_response_carries_etag_or_conditional_semantics()
    {
        AnalysisProgressRegistry registry = new(TimeProvider.System);
        Guid token = Guid.NewGuid();
        registry.TryBegin(token, AnalysisProgressTargetKind.Repository);
        AnalysisProgressModel model = CreateModel(registry);

        model.OnGet(token.ToString("D"));

        Assert.False(model.Response.Headers.ContainsKey("ETag"));
        Assert.False(model.Response.Headers.ContainsKey("Last-Modified"));
    }

    [Fact]
    public void The_endpoint_never_registers_a_run()
    {
        AnalysisProgressRegistry registry = new(TimeProvider.System);
        AnalysisProgressModel model = CreateModel(registry);
        string token = Guid.NewGuid().ToString("D");

        model.OnGet(token);
        model.OnGet(token);

        Assert.Equal(0, registry.Count);
    }

    [Fact]
    public void The_endpoint_exposes_a_get_handler_only()
    {
        System.Reflection.MethodInfo[] handlers = typeof(AnalysisProgressModel)
            .GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance)
            .Where(method =>
                method.Name.StartsWith("On", StringComparison.Ordinal)
                && (method.Name.Contains("Get", StringComparison.Ordinal)
                    || method.Name.Contains("Post", StringComparison.Ordinal)
                    || method.Name.Contains("Put", StringComparison.Ordinal)
                    || method.Name.Contains("Delete", StringComparison.Ordinal)))
            .ToArray();

        System.Reflection.MethodInfo handler = Assert.Single(handlers);
        Assert.StartsWith("OnGet", handler.Name);
    }
}
