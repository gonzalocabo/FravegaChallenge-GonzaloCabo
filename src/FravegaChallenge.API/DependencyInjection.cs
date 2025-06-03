using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using FravegaChallenge.API.Filters;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<AcceptLanguageHeaderSwaggerAttribute>();
        });
        services.AddCarter();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        //Fix https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2293
        services.Configure<AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddLocalization();

        var supportedCultures = new string[] { "es", "en"}.Select(c => new CultureInfo(c)).ToArray();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("es");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        return services;
    }
}
