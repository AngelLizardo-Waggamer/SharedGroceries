using BackSharedGroceries.Middlewares;
using Data;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Base services configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(SwaggerOptions.APIVersion, new OpenApiInfo
    {
        Title = SwaggerOptions.Title,
        Version = SwaggerOptions.APIVersion,
        Description = SwaggerOptions.Description
    });

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// DbContext configuration
builder.Services.ConfigureDbContext(builder.Configuration);

// App building
var app = builder.Build();

// Apply Db migrations at startup
using (var scope = app.Services.CreateScope())
{
    scope.ApplyMigrationsToDb();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1); // Hide schemas section
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<DeviceSessionMiddleware>();

app.MapControllers();

app.Run();
