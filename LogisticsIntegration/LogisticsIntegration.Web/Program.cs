using Microsoft.EntityFrameworkCore;
using LogisticsIntegration.Application.Services;
using LogisticsIntegration.Domain.Interfaces;
using LogisticsIntegration.Infrastructure.Data;
using LogisticsIntegration.Infrastructure.Repositories;
using LogisticsIntegration.Infrastructure.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();

builder.Services.AddScoped<OrderService>();

// builder.Services.AddScoped<DeliveryService>();

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
    pattern: "{controller=Order}/{action=Index}/{id?}"); 

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    Console.WriteLine("Web uygulamasýnda veritabaný migration'larý uygulandý.");
}

app.Run();