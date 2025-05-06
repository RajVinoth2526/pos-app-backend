using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Managers;
using ClientAppPOSWebAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ProductsManager>();

builder.Services.AddScoped<ProductsService>();

builder.Services.AddScoped<SystemManager>();

builder.Services.AddScoped<SystemService>();


// Add services to the container.
builder.Services.AddRazorPages();

// Add DbContext service to the container
builder.Services.AddDbContext<POSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});



var app = builder.Build();

app.UseCors("AllowAll");  // Add CORS middleware

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<POSDbContext>();
    db.Database.Migrate();
}

    

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // <-- ADD THIS!
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // Map API controllers
app.Run();
