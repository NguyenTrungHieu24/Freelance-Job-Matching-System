using API.Configurations;
using API.Services;
using API.Services.Auth;
using API.Services.Memory;
using API.Services.Payment;
using BusinessObjects;
using BusinessObjects.Enums;
using BusinessObjects.Mapping;
using BusinessObjects.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PayOS;
using System;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultFrelancer"),
        b => {
            b.MigrationsAssembly("BusinessObjects");
            b.UseCompatibilityLevel(120); // For MSSQL 2014 :(
        }
    ));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "")
        ),
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail("Invalid token");
                return;
            }

            var db = context.HttpContext.RequestServices
                .GetRequiredService<AppDbContext>();

            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == int.Parse(userId));

            if (user == null)
            {
                context.Fail("User not found");
                return;
            }

            if (!user.IsActive)
            {
                context.Fail("Account has been deactivated");
                return;
            }
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Freelancer Job Matching System API",
        Version = "v1"
    });

    //!Todo: Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(nameof(RoleEnum.ADMIN)));

    options.AddPolicy("EmployerOnly", policy =>
        policy.RequireRole(nameof(RoleEnum.EMPLOYER)));

    options.AddPolicy("FreelancerOnly", policy =>
        policy.RequireRole(nameof(RoleEnum.FREELANCER)));

    options.AddPolicy("AdminOrEmployer", policy =>
        policy.RequireRole(
            nameof(RoleEnum.ADMIN),
            nameof(RoleEnum.EMPLOYER)
        ));

    options.AddPolicy("FinanceOnly", policy =>
        policy.RequireRole(nameof(RoleEnum.FINANCE_MANAGER)));
});

builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));

builder.Services.AddSingleton<PayOSClient>(x =>
{
    var settings = x.GetRequiredService<IOptions<PayOSSettings>>().Value;

    Console.WriteLine($"ClientId: {settings.ClientId}");
    Console.WriteLine($"ApiKey: {settings.ApiKey}");
    Console.WriteLine($"ChecksumKey: {settings.ChecksumKey}");

    return new PayOSClient(
        settings.ClientId ?? "",
        settings.ApiKey ?? "",
        settings.ChecksumKey ?? ""
    );
});
builder.Services.AddScoped<IPayOSService, PayOSService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}
builder.Environment.WebRootPath = webRootPath;

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();

// Run Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(context);
}

app.Run();
