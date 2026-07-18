using BusinessObjects.Enums;
using Client.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Server:Api"]);
});

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/auth/login";
        options.AccessDeniedPath = "/auth/access-denied";
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();

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

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
