using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Policies;
using BusinessObject.Roles;
using DataAccessLayer;
using Garage_pro_api.Authorization;
using Garage_pro_api.DbInit;
using Garage_pro_api.Mapper;
using Garage_pro_api.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.BranchRepositories;
using Repositories.CampaignRepositories;
using Repositories.Customers;
using Repositories.PartCategoryRepositories;
using Repositories.PartRepositories;
using Repositories.PolicyRepositories;
using Repositories.QuotationRepositories;
using Repositories.RepairRequestRepositories;
using Repositories.RoleRepositories;
using Repositories.ServiceRepositories;
using Repositories.UnitOfWork;
using Repositories.VehicleRepositories;
using Services;
using Services.Authentication;
using Services.BranchServices;
using Services.CampaignServices;
using Services.Cloudinaries;
using Services.Customer;
using Services.EmailSenders;
using Services.PartCategoryServices;
using Services.PolicyServices;
using Services.QuotationServices;
using Services.RoleServices;
using Services.ServiceServices;
using Services.SmsSenders;
using Services.VehicleServices;
using System.Text;
using Microsoft.AspNetCore.OData;
using Repositories.VehicleRepositories;
using AutoMapper;
using Repositories.CampaignRepositories;
using Services.CampaignServices;
using Repositories.LogRepositories;
using Services.LogServices;
using Serilog;
using Garage_pro_api.DbInterceptor;
using Microsoft.Extensions.Options;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Add SignalR services
builder.Services.AddSignalR();

// Add services to the container.

builder.Services.AddControllers()
    .AddOData(options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName); // dùng FullName để phân biệt
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

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
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
    builder.Services.AddSingleton<ISmsSender, FakeSmsSender>();
}

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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
            Console.WriteLine("JWT Authentication failed:");
            Console.WriteLine(context.Exception.ToString());
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine("Authorization header: " + context.Request.Headers["Authorization"]);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("JWT validated successfully!");
            Console.WriteLine("User: " + context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

builder.Services.AddSignalR();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// Register session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<ISystemLogRepository, SystemLogRepository>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ISecurityPolicyRepository, SecurityPolicyRepository>();
builder.Services.AddScoped<ISecurityPolicyService, SecurityPolicyService>();
builder.Services.AddMemoryCache(); // Cho IMemoryCache
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IFeedBackRepository, FeedBackRepository>();
builder.Services.AddScoped<IFeedBackService, FeedBackService>();

// OrderStatus and Label repositories and services
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<ILabelService, LabelService>();

// RepairOrder repository and service
builder.Services.AddScoped<IRepairOrderRepository, RepairOrderRepository>();
builder.Services.AddScoped<IRepairOrderService, Services.RepairOrderService>();

// Add this line to register the SignalR hub
builder.Services.AddSignalR();

// Job repository and service
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService>(provider =>
{
    var jobRepository = provider.GetRequiredService<IJobRepository>();
    return new Services.JobService(jobRepository);
});

// Role and Permission services
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>(); // This was missing

// Customer service
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Vehicle repository and services
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IVehicleIntegrationService, VehicleIntegrationService>();

// Quotation services
builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationRepository, Repositories.QuotationRepositories.QuotationRepository>();
builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationServiceRepository, Repositories.QuotationRepositories.QuotationServiceRepository>();
// Update to use the new QuotationServicePartRepository
// builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationPartRepository, Repositories.QuotationRepositories.QuotationPartRepository>();
builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationServicePartRepository, Repositories.QuotationRepositories.QuotationServicePartRepository>();
builder.Services.AddScoped<Services.QuotationServices.IQuotationService, Services.QuotationServices.QuotationManagementService>(); // Updated to use the correct implementation
builder.Services.AddScoped<IRepairOrderRepository, RepairOrderRepository>(); // Add this line

// Vehicle brand, model, and color repositories
builder.Services.AddScoped<IVehicleBrandRepository, VehicleBrandRepository>();
builder.Services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
builder.Services.AddScoped<IVehicleColorRepository, VehicleColorRepository>();

builder.Services.RemoveAll<IPasswordValidator<ApplicationUser>>();
builder.Services.AddScoped<IPasswordValidator<ApplicationUser>, RealTimePasswordValidator<ApplicationUser>>();
builder.Services.AddScoped<DynamicAuthenticationService>();

builder.Services.AddScoped<DynamicAuthenticationService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IBranchService, BranchService>();

builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceService, ServiceService>();

builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
builder.Services.AddScoped<IServiceCategoryService, ServiceCategoryService>();

builder.Services.AddScoped<IPartCategoryRepository, PartCategoryRepository>();
builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();

builder.Services.AddScoped<IOperatingHourRepository, OperatingHourRepository>();
builder.Services.AddScoped<IPartRepository, PartRepository>();

builder.Services.AddHostedService<LogCleanupService>();

// Service Quotation
builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IQuotationService, Services.QuotationServices.QuotationManagementService>();

// repair request
builder.Services.AddScoped<IRequestPartRepository, RequestPartRepository>();
builder.Services.AddScoped<IRequestServiceRepository, RequestServiceRepository>();
builder.Services.AddScoped<IRepairRequestRepository, RepairRequestRepository>();
builder.Services.AddScoped<IRepairRequestService, RepairRequestService>();
// Nếu dùng Scoped lifetime như các repository khác
builder.Services.AddScoped<IRepairImageRepository, RepairImageRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// vehicle
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IVehicleBrandRepository, VehicleBrandRepository>();
builder.Services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
builder.Services.AddScoped<IVehicleColorRepository, VehicleColorRepository>();
builder.Services.AddScoped<VehicleBrandService, VehicleBrandService>();
builder.Services.AddScoped<IVehicleModelService, VehicleModelService>();
builder.Services.AddScoped<IVehicleColorService, VehicleColorService>();

// Repositories & Services
builder.Services.AddScoped<IPromotionalCampaignRepository, PromotionalCampaignRepository>();
builder.Services.AddScoped<IPromotionalCampaignService, PromotionalCampaignService>();

// Inspection services
builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<IInspectionService, InspectionService>();

// Technician services
builder.Services.AddScoped<ITechnicianService, TechnicianService>();

// Repair Request services - Adding missing registrations
builder.Services.AddScoped<Repositories.Customers.IRepairRequestRepository, Repositories.Customers.RepairRequestRepository>();
builder.Services.AddScoped<Services.Customer.IRepairRequestService, Services.Customer.RepairRequestService>();

// Adding missing RequestPart and RequestService repository registrations
builder.Services.AddScoped<Repositories.RepairRequestRepositories.IRequestPartRepository, Repositories.RepairRequestRepositories.RequestPartRepository>();
builder.Services.AddScoped<Repositories.RepairRequestRepositories.IRequestServiceRepository, Repositories.RepairRequestRepositories.RequestServiceRepository>();

// Đăng ký Authorization Handler
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// Đăng ký Policy Provider thay thế mặc định
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);

// Database configuration - FIXED: Removed misplaced async and fixed the configuration
builder.Services.AddDbContext<MyAppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Truyền IServiceProvider thay vì ILogService
    options.AddInterceptors(
        new DatabaseLoggingInterceptor(serviceProvider, slowQueryThresholdMs: 2000) // 2 giây
    );
});

// VNPAY config
builder.Services.AddSingleton<IVnpay>(sp =>
{
    var config = builder.Configuration;

    var vnpay = new Vnpay();
    vnpay.Initialize(
        config["Vnpay:TmnCode"],
        config["Vnpay:HashSecret"],
        config["Vnpay:BaseUrl"],     // https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
        config["Vnpay:CallbackUrl"]  // ReturnUrl (callback client)
    );

    return vnpay;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAndAndroid", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:3000",       // frontend web
                "https://10.0.2.2:7113",       // Android Emulator
                "http://192.168.1.96:7113",   // LDPlayer / LAN
                "http://10.42.97.46:5117"

            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });


});

// Cấu hình Kestrel lắng nghe mọi IP với HTTP & HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5117);  // HTTP (tùy chọn)
    options.ListenAnyIP(7113, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS trên cổng 7113
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS with the specific policy for your frontend
app.UseCors("AllowFrontend");
app.UseCors("AllowAll");
app.UseSession();

//app.UseSecurityPolicyEnforcement();
//app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    Console.WriteLine("==== Incoming request ====");
    Console.WriteLine("Path: " + context.Request.Path);
    Console.WriteLine("Authorization header: " + context.Request.Headers["Authorization"]);
    await next();
});
app.MapHub<LogHub>("/logHub");

app.UseAuthentication();
app.UseMiddleware<UserActivityMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();            // phải chạy trước để gắn User hợp lệ

app.UseSecurityPolicyEnforcement();
app.MapControllers();

app.MapControllers();

// Add this line to map the SignalR hub
app.MapHub<Services.Hubs.RepairOrderHub>("/api/repairorderhub");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
    Console.WriteLine("Applying pending migrations...");
    dbContext.Database.Migrate();

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