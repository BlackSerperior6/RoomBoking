using Microsoft.AspNetCore.Authentication.Cookies;
using RoomBooking;
using RoomBooking.Interfaces;
using RoomBooking.Services;
using RoomBooking.Wrappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDatabaseConnectionFactory, DatabaseConnectionFactory>();

// Register user context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextWrapper, UserContext>();

builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RoomBooking",
        Version = "v1",
        Description = "Documentation for room booking service",
        Contact = new OpenApiContact
        {
            Name = "BlackSerperior6",
            Email = "m330.nek76@gmail.com"
        }
    });

    options.OperationFilter<RazorPageOperationFilter>();
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication";
        options.LogoutPath = "/LogOut";
        options.AccessDeniedPath = "/AccessDenied";
        options.ReturnUrlParameter = "returnUrl";
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RoomBooking.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.MapControllers();

app.Run();

public class RazorPageOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType?.IsSubclassOf(typeof(PageModel)) == true)
        {
            operation.Description ??= "Razor Page endpoint";
            operation.Summary ??= "Page handler";
        }
    }
}
