using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Managers;
using ClientAppPOSWebAPI.Services;
using ClientAppPOSWebAPI.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ProductsManager>();

builder.Services.AddScoped<ProductsService>();

builder.Services.AddScoped<OrderManager>();

builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<UserManager>();

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<SystemManager>();

builder.Services.AddScoped<SystemService>();

// Invoice services
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<InvoicePrintService>();

// Password reset service
builder.Services.AddScoped<PasswordResetService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey123!@#";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "YourApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "YourAppUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorPages();

// Add DbContext service to the container with dynamic database provider support
builder.Services.AddDbContext<POSDbContext>(options =>
    options.ConfigureDatabase(builder.Configuration));

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
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    var provider = DatabaseConfiguration.GetDatabaseProvider(configuration);
    logger.LogInformation("Using database provider: {Provider}", provider);
    
    db.Database.Migrate();
    logger.LogInformation("Database migration completed successfully");
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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // Map API controllers
app.Run();
