using ContaMente.Contexts;
using ContaMente.Models;
using ContaMente.Repositories;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;
using ContaMente.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DotNetEnv;
using Hangfire;
using Hangfire.PostgreSql;
using ContaMente.Middlewares;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddHangfire(config =>
config.UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IMovimentacaoService, MovimentacaoService>();
builder.Services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IResponsavelService, ResponsavelService>();
builder.Services.AddScoped<IResponsavelRepository, ResponsavelRepository>();
builder.Services.AddScoped<IParcelaService, ParcelaService>();
builder.Services.AddScoped<IParcelaRepository, ParcelaRepository>();
builder.Services.AddScoped<IRecorrenciaService, RecorrenciaService>();
builder.Services.AddScoped<IRecorrenciaRepository, RecorrenciaRepository>();
builder.Services.AddScoped<ICartaoService, CartaoService>();
builder.Services.AddScoped<ICartaoRepository, CartaoRepository>();
builder.Services.AddScoped<IUserConfigurationService, UserConfigurationService>();
builder.Services.AddScoped<IUserConfigurationRepository, UserConfigurationRepository>();
builder.Services.AddScoped<ITipoPagamentoService, TipoPagamentoService>();
builder.Services.AddScoped<IMovimentacaoParcelaService, MovimentacaoParcelaService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints() 
    .AddDefaultTokenProviders();

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = 401,
            mensagem = "Usuário não autenticado.",
            detalhe = "É necessário fazer login para acessar este recurso."
        });
        return context.Response.WriteAsync(result);
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = 403,
            mensagem = "Acesso negado.",
            detalhe = "Você não tem permissão para acessar este recurso."
        });
        return context.Response.WriteAsync(result);
    };
});

var app = builder.Build();

app.UseHangfireDashboard();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware(typeof(GlobalErrorHandlingMiddleware));

app.UseHttpsRedirection();

app.UseCors(options => options
    .WithOrigins(Environment.GetEnvironmentVariable("CORS_ORIGIN")!)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<User>();

app.Run();
