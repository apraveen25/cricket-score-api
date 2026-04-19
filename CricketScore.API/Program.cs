using System.Text;
using CricketScore.Application;
using CricketScore.Infrastructure;
using CricketScore.Infrastructure.Persistence;
using CricketScore.API.Hubs;
using CricketScore.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.OpenApi.Models;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    builder.Services.AddControllers();
    builder.Services.AddSignalR();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        // Add JWT Bearer auth to Swagger
        var jwtScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer {token}' to authenticate",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        c.AddSecurityDefinition("Bearer", jwtScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtScheme, new string[] { } }
        });
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var jwtSettings = builder.Configuration.GetSection("Jwt");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        context.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CricketCors", policy =>
            policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CricketScore API v1");
            c.RoutePrefix = string.Empty; // serve at application root
        });
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseCors("CricketCors");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<ScoreHub>("/hubs/score");

    await InitializeCosmosDbAsync(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}

static async Task InitializeCosmosDbAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var cosmosContext = scope.ServiceProvider.GetService<CosmosDbContext>();
        if (cosmosContext is null)
        {
            Log.Warning("CosmosDbContext not registered; skipping initialization.");
            return;
        }

        await cosmosContext.InitializeAsync();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to initialize Cosmos DB. The application will continue to run, but data storage may be unavailable.\nEnsure the Cosmos DB emulator or configured account is running and the connection settings are correct.");
    }
}
