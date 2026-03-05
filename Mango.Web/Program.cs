using Mango.Web;
using Mango.Web.Services;
using Mango.Web.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// =====================
//  Add services
// =====================

builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

SD.ProductAPIBase = builder.Configuration["ServiceUrl:ProductAPI"]!;

builder.Services.AddHttpClient("MangoAPI", client =>
{
    client.BaseAddress = new Uri(SD.ProductAPIBase);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";

})
    .AddCookie("Cookies", c => c.ExpireTimeSpan = TimeSpan.FromMinutes(10))
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = builder.Configuration["ServiceUrl:IdentityAPI"]!;

    options.ClientId = "mango.web";
    options.ClientSecret = "mango-web-secret";

    options.ResponseType = "code";
    options.UsePkce = true;

    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("mango");

    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";
});

//tst commit

builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileService, FileService>();


var app = builder.Build();

// =====================
// Configure pipeline
// =====================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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