using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CurrencyExchange.Infrastructure.SwaggerConfig
{
    public class AddClientIdHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "clientid",
                In = ParameterLocation.Header,
                Description = "Client ID used for generating JWT token",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
