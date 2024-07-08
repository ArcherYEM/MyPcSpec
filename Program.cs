using Microsoft.EntityFrameworkCore;
using MyPCSpec.Models;
using MyPCSpec.Services;
using MyPCSpec.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<MpsContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MPS_Database"),
    new MySqlServerVersion(new Version(8, 0, 21))));

//서비스 등록
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
