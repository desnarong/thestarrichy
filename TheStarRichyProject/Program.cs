using Localization.SqlLocalizer.DbStringLocalizer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Reflection;
using TheStarRichyProject;
using TheStarRichyProject.Services;

var builder = WebApplication.CreateBuilder(args);

// โหลดการตั้งค่า
builder.Configuration
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.AddEnvironmentVariables();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

// ตั้งค่าภาษา
var supportedCultures = new[]
{
new CultureInfo("en-US"),
new CultureInfo("th-TH"),
new CultureInfo("lo-LA"),
new CultureInfo("km-KH"),
new CultureInfo("my-MM")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Cache + Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<double>("IdleTimeout", 30));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", policy =>
    {
        policy.WithOrigins("[https://localhost:4527](https://localhost:4527)", "[http://localhost:5122](http://localhost:5122)")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IKbankApiClient, KbankApiClient>();
builder.Services.AddScoped<IProductApiClient, ProductApiClient>();
builder.Services.AddScoped<ICartApiService, CartApiService>();
builder.Services.AddScoped<IOrderApiService, OrderApiService>();
builder.Services.AddScoped<IApiService, ApiService>();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseBrowserLink();
    app.UseDeveloperExceptionPage();
}

// Localization
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowOrigin");
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Auth}/{action=Login}/{id?}");

app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
    response.StatusCode == (int)HttpStatusCode.Forbidden)
    {
        response.Redirect("/Auth/Login");
    }
    return Task.CompletedTask;
});

app.Run();
