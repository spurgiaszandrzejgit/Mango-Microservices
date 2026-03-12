using Mango.Services.CouponAPI.DbContexts;
using Mango.Services.CouponAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGrpcService<CouponService>();
app.MapGet("/", () => "Coupon gRPC service is running.");

app.Run();