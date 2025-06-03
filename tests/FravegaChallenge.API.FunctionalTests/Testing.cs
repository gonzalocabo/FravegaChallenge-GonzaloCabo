namespace FravegaChallenge.API.FunctionalTests;

[SetUpFixture]
public partial class Testing
{
    private static CustomWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public Task RunBeforeAnyTests()
    {
        _factory = new CustomWebApplicationFactory();
        return Task.CompletedTask;
    }
    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await _factory.DisposeAsync();
    }

    public static HttpClient GetClient()
    {
        return _factory.CreateClient();
    }
}
