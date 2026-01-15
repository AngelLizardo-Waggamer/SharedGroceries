using BackSharedGroceries.Middlewares;
using Data;

var builder = WebApplication.CreateBuilder(args);

// Base services configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<DeviceSessionMiddleware>();

app.MapControllers();

app.Run();
