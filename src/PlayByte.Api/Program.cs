using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlayByte.Api.Filters;
using PlayByte.Application;
using PlayByte.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog lido do appsettings: Console + File (rolling). O sink de ARQUIVO e' o
// fallback usado pelo ErrorLogger quando a gravacao no SQL Server falha.
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// Camadas (Clean Architecture).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Autenticacao JWT (US-02 c3: valida o token em endpoints protegidos).
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["SecretKey"]
                    ?? throw new InvalidOperationException("Jwt:SecretKey nao configurada."))),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// CORS para o frontend Angular (SPA em outra origem). As origens permitidas vem da
// configuracao (Cors:AllowedOrigins); em dev, o Angular roda em http://localhost:4200.
const string corsPolicy = "PlayByteSpa";
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
    options.AddPolicy(corsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

// Filtro global de excecao (depende de IExceptionLogger, scoped).
builder.Services.AddScoped<ApiExceptionFilterAttribute>();
builder.Services.AddControllers(options =>
    options.Filters.AddService<ApiExceptionFilterAttribute>());

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // /openapi/v1.json
    app.MapScalarApiReference();      // UI moderna em /scalar/v1
}

// Em Development o frontend Angular chama a API por HTTP (localhost:5080); o redirect
// para HTTPS quebraria o CORS e exigiria certificado confiável. Fora de Development,
// mantemos o redirect.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Torna a classe Program (gerada pelos top-level statements) acessivel para o
// WebApplicationFactory<Program> nos testes de integracao.
public partial class Program;
