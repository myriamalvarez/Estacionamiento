using System.Text;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Estacionamiento.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddDbContext<EstacionamientoContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)).UseSnakeCaseNamingConvention());// Para el mapeo de las tablas en la base de datos con nombres en snake_case
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ServicioSeguridad>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["TokenAuthentication:Issuer"],
            ValidAudience = builder.Configuration["TokenAuthentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenAuthentication:SecretKey"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Si el request trae el header Authorization (API) se usa ese; si no, la cookie (web).
                if (!context.Request.Headers.ContainsKey("Authorization"))
                
                    context.Token = context.Request.Cookies["access_token"];
                
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
               //La API responde 401 normal; la web redirige a login
                if (context.Request.Path.StartsWithSegments("/api"))
                   return Task.CompletedTask;

                context.HandleResponse();
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Usuario/Login?returnUrl={returnUrl}");
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                //La API responde 403 normal; la web redirige a "acceso denegado"
                if (!context.Request.Path.StartsWithSegments("/api"))
                    context.Response.Redirect("/Usuario/AccesoDenegado");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    // Exige login en TODAS las acciones salvo las marcadas [AllowAnonymous] (Login, AccesoDenegado, login de la API).
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Necesario para servir lo que se sube en runtime (avatares y comprobantes en /uploads/...).
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
