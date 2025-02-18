using Serilog;
using System.Text;
using OpenTelemetry.Trace;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Infrastructure.Services;
using CurrencyExchange.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CurrencyExchange.Infrastructure.ResilienceProvider;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using CurrencyExchange.Infrastructure.SwaggerConfig;



var builder = WebApplication.CreateBuilder(args);



// Configuration
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(opts =>
        {
            opts.AgentHost = configuration["OpenTelemetry:Jaeger:Host"];
            opts.AgentPort = Convert.ToInt32(configuration["OpenTelemetry:Jaeger:Port"]);
        }));


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// Add Swagger with Bearer Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency Converter API", Version = "v1" });

    // Add Bearer Token support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    c.OperationFilter<AddClientIdHeaderParameter>();

});


// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
        };
    });

//// Authorization
builder.Services.AddAuthorization();

// Caching (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["CacheSettings:RedisConnection"];
});

// Dependency Injection
builder.Services.AddScoped<ICurrencyConverter, FrankFrutImplementation>();
builder.Services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();


// HTTP client with Polly resilience
builder.Services.AddHttpClient("FrankfurterClient", client =>
{
    client.BaseAddress = new Uri(configuration["FrankfurterApi:BaseUrl"]);
})
.AddPolicyHandler(ResilienceProvider.GetRetryPolicy())
.AddPolicyHandler(ResilienceProvider.GetCircuitBreakerPolicy());

//api versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); 
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// API Rate Limiting
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Converter API");
        });
}
app.UseMiddleware<ValidateClientMiddleWare>();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<RateLimitMiddleWare>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
