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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();