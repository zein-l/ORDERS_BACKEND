using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen; // for IOperationFilter
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;

// Our namespaces
using Orders.Infrastructure.Data;
using Orders.Application.Abstractions;
using Orders.Application.Services;
using Orders.Infrastructure.Repositories;
using Orders.Application.Interfaces;
using Orders.Infrastructure.Services;
using Orders.Api.Middleware;

try
{
    Console.WriteLine("[BOOT] 1/8 building builder…");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Logging.ClearProviders();
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .CreateLogger();
    builder.Host.UseSerilog();

    Console.WriteLine("[BOOT] 2/8 loading config & db…");

    var cs = builder.Configuration.GetConnectionString("Default") ?? "Data Source=../orders.db";
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));

    // MVC + Swagger (with JWT security + per-operation lock via OperationFilter)
    builder.Services.AddControllers();
    builder.Services.AddValidatorsFromAssemblyContaining<Orders.Application.Validators.AddItemRequestValidator>();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders.Api", Version = "v1" });

        // JWT Bearer security so the Authorize button (and lock icons) show up
        var jwtScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter: Bearer {your JWT}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        c.AddSecurityDefinition("Bearer", jwtScheme);

        // Global requirement (lets Try-It-Out send the token by default)
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtScheme, Array.Empty<string>() }
        });

        // This adds a `security` block to every [Authorize] action -> lock icon appears
        c.OperationFilter<SwaggerAuthorizeOperationFilter>();
    });

    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    builder.Services.AddProblemDetails();

    Console.WriteLine("[BOOT] 3/8 auth setup…");

    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrdersApi";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OrdersApiClient";
    var jwtKey = builder.Configuration["Jwt:Key"] ?? new string('X', 64); // dev fallback (>=256-bit)

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.RequireHttpsMetadata = false; // dev
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
    builder.Services.AddAuthorization();

    Console.WriteLine("[BOOT] 4/8 DI registrations…");

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IAuditService, AuditService>();

    Console.WriteLine("[BOOT] 5/8 building app…");

    var app = builder.Build();

    Console.WriteLine("[BOOT] 6/8 migrating db…");
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    Console.WriteLine("[BOOT] 7/8 pipeline…");

    // Always enable Swagger (dev + prod)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders.Api v1");
        // c.RoutePrefix = "swagger"; // default is "swagger"
    });

    // If dev HTTPS certs cause issues, leave this commented.
    // app.UseHttpsRedirection();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    // Convenience endpoints
    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

    app.MapControllers();

    // Bind explicitly to HTTP (keeps it predictable even without launchSettings)
    app.Urls.Clear();
    app.Urls.Add("http://localhost:5238");

    // Print bound URLs once started (handy for troubleshooting)
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Console.WriteLine(" ---- Kestrel URLs ---- ");
        foreach (var u in app.Urls) Console.WriteLine(u);
        Console.WriteLine(" ---------------------- ");
    });

    Console.WriteLine("[BOOT] 8/8 RUN!");
    app.Run();
}
catch (Exception ex)
{
    var msg = $"FATAL: {ex.GetType().Name} - {ex.Message}{Environment.NewLine}{ex}";
    Console.Error.WriteLine(msg);
    try { System.IO.File.WriteAllText("startup-fatal.txt", msg); } catch { }
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Adds a `security` requirement to any action/class that has [Authorize],
/// which makes the lock icon appear in Swagger UI.
/// </summary>
internal sealed class SwaggerAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorizeOnController =
            context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .Any() == true;

        var hasAuthorizeOnMethod =
            context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .Any();

        if (!(hasAuthorizeOnController || hasAuthorizeOnMethod))
            return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = new List<string>()
        });
    }
}
