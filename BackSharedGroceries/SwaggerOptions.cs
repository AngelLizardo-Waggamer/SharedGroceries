using Microsoft.OpenApi;
using BackSharedGroceries.Filters;

public static class SwaggerOptions
{
    private static readonly string APIVersion = "v1";
    public static readonly string Title = "Shared Groceries API";
    public static readonly string Description = """
    API for managing shared grocery lists and family shopping
    """;

    public static void ConfigureSwaggerDocGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(APIVersion, new OpenApiInfo
            {
                Title = Title,
                Version = APIVersion,
                Description = Description
            });

            // Enable XML comments for Swagger documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // Use operation filter to apply security only to endpoints with [Authorize]
            options.OperationFilter<AuthorizeOperationFilter>();
        });
    }
}