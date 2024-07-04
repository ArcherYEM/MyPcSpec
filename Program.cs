using Microsoft.EntityFrameworkCore;
using MyPCSpec.Models;
using MyPCSpec.Services;
using MyPCSpec.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MpsContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MPS_Database"),
    new MySqlServerVersion(new Version(8, 0, 21))));

builder.Services.AddScoped<IMemberService, MemberService>();

var app = builder.Build();

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
