using Serilog;
using Azure.Identity;
using AspNetCoreRateLimit;
using ArtGallery.API.Extensions;
using ArtGallery.API.Middleware;
using ArtGallery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault (if enabled)
var keyVaultEnabled = builder.Configuration.GetValue<bool>("KeyVault:Enabled");
if (keyVaultEnabled)
{
    var keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
        
        Log.Information("Azure Key Vault configured: {VaultUrl}", keyVaultUrl);
    }
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add custom services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapperConfiguration();
builder.Services.AddValidationConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddSwaggerConfiguration();

// Add rate limiting
builder.Services.AddRateLimitingConfiguration(builder.Configuration);

// Add response caching
builder.Services.AddCachingConfiguration(builder.Configuration);
builder.Services.AddResponseCaching();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Art Gallery API v1");
        c.RoutePrefix = "swagger";
    });
}

// Global exception handling
app.UseExceptionHandling();

// Rate limiting (before other middleware)
app.UseIpRateLimiting();

// Request logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Response caching
app.UseResponseCaching();

// CORS
app.UseCors("AllowVueApp");

app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Database initialization (disabled for Oracle - use separate migration scripts)
if (builder.Configuration.GetValue<bool>("Database:AutoMigrate", false))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // For Oracle, we typically don't use EF migrations
        // Instead, verify the connection is working
        await context.Database.CanConnectAsync();
        Log.Information("Successfully connected to OLTP Oracle database");
        
        var dwContext = services.GetRequiredService<DwDbContext>();
        await dwContext.Database.CanConnectAsync();
        Log.Information("Successfully connected to DW Oracle database");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while connecting to the database");
    }
}

Log.Information("Art Gallery API is starting...");
Log.Information("Oracle OLTP Connection: {Connection}", 
    MaskConnectionString(builder.Configuration.GetConnectionString("OltpConnection") ?? ""));
Log.Information("Oracle DW Connection: {Connection}", 
    MaskConnectionString(builder.Configuration.GetConnectionString("DwConnection") ?? ""));

app.Run();

// Helper method to mask sensitive parts of connection string
static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return "[Not Configured]";
    
    // Mask password
    var masked = System.Text.RegularExpressions.Regex.Replace(
        connectionString, 
        @"Password=([^;]+)", 
        "Password=****",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    
    return masked;
}

