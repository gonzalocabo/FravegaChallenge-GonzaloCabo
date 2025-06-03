namespace FravegaChallenge.API;

public static class VersionGroupProvider
{
    public static RouteGroupBuilder GetRouteGroupBuilder(this IEndpointRouteBuilder app, ICarterModule carterModule)
    {
        var endpointsType = carterModule.GetType();
        var version = endpointsType.Namespace?.Split('.').Last().ToLower() ?? "v1";
        var name = endpointsType.Name.Split("Endpoints").First();

        return app
            .MapGroup($"api/{version}")
            .WithDisplayName(version.ToString())
            .MapGroup(name.ToLower())
            .WithName(endpointsType.Name);
    }
}
