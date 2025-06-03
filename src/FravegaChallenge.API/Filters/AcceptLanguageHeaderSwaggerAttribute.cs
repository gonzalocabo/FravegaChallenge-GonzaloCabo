using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FravegaChallenge.API.Filters;

public class AcceptLanguageHeaderSwaggerAttribute : IOperationFilter
{

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new OpenApiString("es"),
                Enum = new List<IOpenApiAny>
                {
                    new OpenApiString("en"),
                    new OpenApiString("es")
                }
            }
        });
    }

}
