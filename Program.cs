using Microsoft.EntityFrameworkCore;
using RazorInMemoryDemo.Models;
using RazorInMemoryDemo.Hubs;
using Microsoft.AspNetCore.Authorization;
using RazorInMemoryDemo.Middleware;
using RazorInMemoryDemo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminPolicy");
});

// Add in-memory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BooksDb"));

// Add session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add SignalR
builder.Services.AddSignalR();

// Add authentication and authorization
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "UserLoginCookie";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Events.OnRedirectToLogin = context =>
        {
            // If the user was logged in but is now banned, clear their cookies
            if (context.Request.Path.StartsWithSegments("/Account/Logout/Banned"))
            {
                // Cookie will be cleared by the Logout page
            }
            return Task.CompletedTask;
        };
    });

// Add custom requirement for active user check
builder.Services.AddScoped<IAuthorizationHandler, RazorInMemoryDemo.ActiveUserHandler>();

builder.Services.AddAuthorization(options =>
{
    // Policy to check if user is active (not banned)
    options.AddPolicy("ActiveUser", policy =>
        policy.Requirements.Add(new RazorInMemoryDemo.ActiveUserRequirement()));

    // Admin policy now requires the user to be active as well
    options.AddPolicy("AdminPolicy", policy => 
        policy.RequireClaim("Role", "Admin")
              .AddRequirements(new RazorInMemoryDemo.ActiveUserRequirement()));
    
    // User policy now requires the user to be active
    options.AddPolicy("UserPolicy", policy => 
        policy.RequireAssertion(context => 
            context.User.HasClaim(c => 
                (c.Type == "Role" && (c.Value == "Admin" || c.Value == "User"))))
              .AddRequirements(new RazorInMemoryDemo.ActiveUserRequirement()));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();

// Add ban check middleware (must be after session and before authentication)
app.UseUserBanCheck();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<UserHub>("/userHub"); // Add SignalR Hub endpoint

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
