using Microsoft.OpenApi.Models;
using Mo5.RagServer.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "MO5 RAG Server API", 
        Version = "v1",
        Description = "API for semantic search in Thomson MO5 development knowledge base"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    // Add API key security definition for Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Description = "API key needed to access protected endpoints."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }, new string[] { } }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add API key authentication
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Mo5.RagServer.Api.Security.ApiKeyAuthenticationHandler>(
        "ApiKey", options => { });

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for easy API exploration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MO5 RAG Server API v1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<Mo5.RagServer.Infrastructure.Data.RagDbContext>();

    try
    {
        // Étape 1 : Forcer la création physique des tables sans passer par les fichiers de migration
        logger.LogInformation("Checking database schema...");
        await context.Database.EnsureCreatedAsync(); 
        logger.LogInformation("Database tables are confirmed/created.");
        
        // Étape 2 : Seeding (seulement si les tables sont là)
        logger.LogInformation("Seeding database...");
        await Mo5.RagServer.Infrastructure.Data.DbInitializer.InitializeAsync(context, logger);
        logger.LogInformation("Database is ready.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Critical error during database setup.");
        // Optionnel : ne pas stopper l'app, ou throw si vous voulez que le container restart
    }
}

try
{
    Log.Information("Starting MO5 RAG Server API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
/// <summary>
/// Entry point partial class used by integration tests to host the application.
/// </summary>
public partial class Program { }
