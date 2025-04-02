using Microsoft.EntityFrameworkCore;
using BanHang.Models;
using BanHang.Models.Identity;
using BanHang.Reposirories.Interfaces;
using BanHang.Reposirories.Implementations;
using BanHang.Services.Interfaces;
using BanHang.Services.Implementations;
using BanHang.Models.Settings;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Register DB Context 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HostConnectionString")));

// Register Identity services
/*builder.Services.AddI<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();*/
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
        options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
/*
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
*/

// Register Cloudinary 
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// Đăng ký các repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
// Đăng ký các services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IOrderService, OrderService>();



/*
builder.Services.ConfigureApplicationCookie(options =>
{
    // Chỉ cho phép cookie được truy cập qua HTTP, không thể truy cập bằng JavaScript
    options.Cookie.HttpOnly = true;
    // Đường dẫn chuyển hướng khi người dùng chưa đăng nhập
    options.LoginPath = "/Admin/Login";
    // Đường dẫn chuyển hướng khi người dùng đăng xuất
    options.LogoutPath = "/Admin/Logout";
    // Đường dẫn chuyển hướng khi người dùng không có quyền truy cập
    options.AccessDeniedPath = "/Admin/AccessDenied";
    // Đặt thời gian tồn tại của cookie là 30 phút
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});*/

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

// Thêm middleware Session
app.UseSession();

app.Run();
