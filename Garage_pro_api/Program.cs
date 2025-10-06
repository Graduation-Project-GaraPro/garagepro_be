using BusinessObject.Authentication;
using System.Text;
using DataAccessLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services;
using BusinessObject;
using Garage_pro_api.DbInit;
using Repositories;
using Services.EmailSenders;
using Repositories.PolicyRepositories;
using Services.PolicyServices;
using Services.Authentication;
using Garage_pro_api.Middlewares;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BusinessObject.Policies;
using Repositories.RoleRepositories;
using Services.RoleServices;
using Garage_pro_api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Garage_pro_api.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Google;
using Services.SmsSenders;
using BusinessObject.Roles;
using Repositories.BranchRepositories;
using Services.BranchServices;
using Repositories.ServiceRepositories;
using Services.ServiceServices;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Thêm thông tin cho Swagger UI
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    // Thêm cấu hình cho Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddDbContext<MyAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser,ApplicationRole> (options =>
{
    options.Password.RequiredLength = 1;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
    options.Lockout.MaxFailedAccessAttempts = int.MaxValue;
    // User Policy
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<MyAppDbContext>()
.AddDefaultTokenProviders()
.AddPasswordValidator<RealTimePasswordValidator<ApplicationUser>>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<ISmsSender, FakeSmsSender>();
}
else
{
    // Production: Đăng ký Twilio hoặc dịch vụ SMS thật
    // builder.Services.AddTransient<ISmsSender, TwilioSmsSender>();
}

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SmartScheme";          // mặc định chọn scheme "thông minh"
    options.DefaultChallengeScheme = "SmartScheme";
})
.AddPolicyScheme("SmartScheme", "JWT or Cookie", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // Ưu tiên JWT nếu có header Bearer
        string authorization = context.Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

        // Nếu không thì fallback về Cookie
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/api/auth/login";   // đường dẫn login
    options.LogoutPath = "/api/auth/logout"; // đường dẫn logout
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddScoped<DbInitializer>();

builder.Services.AddScoped<ISecurityPolicyRepository, SecurityPolicyRepository>();
builder.Services.AddScoped<ISecurityPolicyService, SecurityPolicyService>();
builder.Services.AddMemoryCache(); // Cho IMemoryCache
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// OrderStatus and Label repositories and services
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<ILabelService, LabelService>();
builder.Services.AddScoped<IColorService, ColorService>();

// RepairOrder repository and service
builder.Services.AddScoped<IRepairOrderRepository, RepairOrderRepository>();
builder.Services.AddScoped<IRepairOrderService, Services.RepairOrderService>();

// Job repository and service
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, Services.JobService>();

// Role and Permission services
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>(); // This was missing

builder.Services.RemoveAll<IPasswordValidator<ApplicationUser>>();
builder.Services.AddScoped<IPasswordValidator<ApplicationUser>, RealTimePasswordValidator<ApplicationUser>>();
builder.Services.AddScoped<DynamicAuthenticationService>();

//builder.Services.AddScoped<ISystemLogRepository, SystemLogRepository>();
//builder.Services.AddScoped<ISystemLogService, SystemLogService>();

builder.Services.AddScoped<DynamicAuthenticationService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();


builder.Services.AddScoped<IBranchRepository, BranchRepository>();

builder.Services.AddScoped<IBranchService, BranchService>();

builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();


builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
builder.Services.AddScoped<IOperatingHourRepository, OperatingHourRepository>();

// Đăng ký Authorization Handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// Đăng ký Policy Provider thay thế mặc định
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder  
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();            // phải chạy trước để gắn User hợp lệ
app.UseSecurityPolicyEnforcement();

app.MapControllers();
// Initialize database

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();

    if (!dbContext.SecurityPolicies.Any())
    {
        dbContext.SecurityPolicies.Add(new SecurityPolicy
        {
            Id = Guid.NewGuid(),
            MinPasswordLength = 8,
            RequireSpecialChar = true,
            RequireNumber = true,
            RequireUppercase = true,
            SessionTimeout = 30,
            MaxLoginAttempts = 5,
            AccountLockoutTime = 15,
            MfaRequired = false,
            PasswordExpiryDays = 90,
            EnableBruteForceProtection = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.Initialize();
}
app.Run();