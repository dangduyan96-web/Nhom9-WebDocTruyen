using Microsoft.EntityFrameworkCore;
using System;
using WebDocTruyen.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Đổi sang AddControllersWithViews() để hỗ trợ View (.cshtml) và bỏ qua vòng lặp cycle JSON
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Hỗ trợ CORS cho Mobile App (Expo SDK / React Native)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký AppDbContext kết nối SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký HttpContextAccessor và Session
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Sắp xếp lại thứ tự Middleware chuẩn xác của ASP.NET Core
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Bật CORS cho Mobile App kết nối API
app.UseCors("AllowAll");

// UseSession bắt buộc phải nằm sau UseRouting và trước UseAuthorization
app.UseSession();
app.UseAuthorization();

// Định tuyến dành riêng cho khu vực Admin (Đặt lên trước route mặc định)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Định tuyến về Home/Index khi mở web
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();