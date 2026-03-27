using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BackSharedGroceries.Filters
{
    /// <summary>
    /// Operation filter that adds JWT bearer authentication requirement to endpoints decorated with [Authorize].
    /// This ensures Swagger UI only applies the bearer token to endpoints that actually require authentication.
    /// </summary>
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the endpoint has the [Authorize] attribute
            var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>()
                .Any() ?? false;

            // Check if the endpoint has [AllowAnonymous] which overrides [Authorize]
            var hasAllowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AllowAnonymousAttribute>()
                .Any() ?? false;

            // Only add security requirement if endpoint has [Authorize] and NOT [AllowAnonymous]
            if (hasAuthorize && !hasAllowAnonymous)
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new() {
                        [new OpenApiSecuritySchemeReference("bearer", context.Document)] = []
                    }
                };

                // Add 401 response if not already present
                if (operation.Responses != null && !operation.Responses.ContainsKey("401"))
                {
                    operation.Responses.Add("401", new OpenApiResponse
                    {
                        Description = "Unauthorized - Valid JWT token required"
                    });
                }
            }
        }
    }
}