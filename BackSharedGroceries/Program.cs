using BackSharedGroceries.Middlewares;
using Data;
using BackSharedGroceries.Interfaces;
using BackSharedGroceries.Controllers.Auth;

var builder = WebApplication.CreateBuilder(args);

// Base services configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerDocGen();

// DbContext configuration
builder.Services.ConfigureDbContext(builder.Configuration);

// JWT Authentication configuration
builder.Services.ConfigureJWTAuth();

// Services and Repositories configuration
builder.Services.ConfigureInterfaces();

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
