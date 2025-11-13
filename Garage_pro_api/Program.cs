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
using AutoMapper;
using Repositories.InspectionAndRepair;
using Services.InspectionAndRepair;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;
using System.Text.Json.Serialization;
using Repositories.Statistical;
using Services.Statistical;
using Microsoft.EntityFrameworkCore.Migrations;
using Services.RepairHistory;
using Repositories.RepairHistory;
using Repositories.LogRepositories;
using Services.LogServices;
using Serilog;
using Garage_pro_api.DbInterceptor;
using Microsoft.Extensions.Options;
using VNPAY.NET;
using Garage_pro_api.Hubs;
using Services.Hubs;
using Repositories.RepairProgressRepositories;
using Services.RepairProgressServices;
using Garage_pro_api.BackgroundServices;
using Services.UserServices;

using Repositories.PaymentRepositories;
using Services.FCMServices;
using Repositories.EmergencyRequestRepositories;
using Services.EmergencyRequestService;
using Services.GeocodingServices;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Client;
using Utils.RepairRequests;
using BusinessObject.PayOsModels;
using Services.PayOsClients;
using Services.PaymentServices;
using Repositories.WebhookInboxRepositories;
var builder = WebApplication.CreateBuilder(args);

// OData Model Configuration
var modelBuilder = new ODataConventionModelBuilder();
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", modelBuilder.GetEdmModel())
    )
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
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

    c.CustomSchemaIds(type => type.FullName);
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    // Th�m c?u h�nh cho Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",

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
    cfg.AddProfile<RepairMappingProfile>();
    cfg.AddProfile<InspectionTechnicianProfile>();
    cfg.AddProfile<JobTechnicianProfile>();
    cfg.AddProfile<QuotationProfile>();
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
        OnTokenValidated = async context =>
        {

            var expTicks = context.Principal?.FindFirst("pwd_exp_at")?.Value;
            if (long.TryParse(expTicks, out var t))
            {
                var pwdExpireAtUtc = new DateTime(t, DateTimeKind.Utc);
                if (DateTime.UtcNow >= pwdExpireAtUtc)
                {
                    var isAdmin = context.Principal?.IsInRole("Admin") ?? false;
                    if (isAdmin)
                        return;
                    // Cho phép vài đường dẫn whitelisted
                    var path = context.HttpContext.Request.Path;
                    if (!path.StartsWithSegments("/api/auth/change-password") &&
                        !path.StartsWithSegments("/api/auth/logout"))
                    {
                        context.Fail("PASSWORD_EXPIRED");
                        return;
                    }
                }
            }


            // 1) Lấy claim policyUpdatedAt từ principal (không cast token)
            var ticksStr = context.Principal?.FindFirst("policyUpdatedAt")?.Value;

            if (long.TryParse(ticksStr, out var ticks))
            {
                var tokenPolicyTime = new DateTime(ticks, DateTimeKind.Utc);

                // 2) Lấy policy hiện tại (đã cache 1 phút trong service của bạn)
                var policySvc = context.HttpContext.RequestServices.GetRequiredService<ISecurityPolicyService>();
                var policy = await policySvc.GetCurrentAsync();

                // 3) So sánh: token phát trước lần cập nhật policy gần nhất -> fail
                if (policy != null && tokenPolicyTime < policy.UpdatedAt)
                {
                    context.Fail("TOKEN_ISSUED_BEFORE_POLICY_UPDATE");
                    return;
                }
            }

            // 4) Gắn hint sắp hết hạn (< 1 phút)
            //    Lưu ý: ValidTo theo UTC, nên so với DateTime.UtcNow
            var remaining = context.SecurityToken.ValidTo.ToUniversalTime() - DateTime.UtcNow;
            if (remaining.TotalMinutes < 1)
            {
                context.Response.Headers["X-Session-Expiring-Soon"] = "true";
            }

            // 5) Log tham khảo
            Console.WriteLine($"✅ JWT validated. User={context.Principal?.Identity?.Name}, remaining={remaining}");
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
//builder.Services.AddScoped<IRevenueService, RevenueService>();

builder.Services.AddScoped<IFeedBackRepository, FeedBackRepository>();
builder.Services.AddScoped<IFeedBackService, FeedBackService>();

// Trong Program.cs
builder.Services.AddScoped<IWebhookInboxRepository, WebhookInboxRepository>();

// OrderStatus and Label repositories and services
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<ILabelService, LabelService>();

// RepairOrder repository and service
builder.Services.AddScoped<IRepairOrderRepository, RepairOrderRepository>();
builder.Services.AddScoped<IRepairOrderService, Services.RepairOrderService>();


// Technician repository and service
builder.Services.AddScoped<IJobTechnicianRepository, JobTechnicianRepository>();
builder.Services.AddScoped<IJobTechnicianService, JobTechnicianService>();
builder.Services.AddScoped<IInspectionTechnicianRepository, InspectionTechnicianRepository>();
builder.Services.AddScoped<IInspectionTechnicianService, InspectionTechnicianService>();
builder.Services.AddScoped<ISpecificationRepository, SpecificationRepository>();
builder.Services.AddScoped<ISpecificationService, SpecificationService>();
builder.Services.AddScoped<IRepairRepository, RepairRepository>();
builder.Services.AddScoped<IRepairService, RepairService>();
builder.Services.AddScoped<IStatisticalRepository, StatisticalRepository>();
builder.Services.AddScoped<IStatisticalService, StatisticalService>();
builder.Services.AddScoped<IRepairHistoryRepository, RepairHistoryRepository>();
builder.Services.AddScoped<IRepairHistoryService, RepairHistoryService>();



builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.Decorate<IPermissionService, CachedPermissionService>();


builder.Services.AddScoped<IRepairProgressRepository, RepairProgressRepository>();
builder.Services.AddScoped<IRepairProgressService, RepairProgressService>();
// Add this line to register the SignalR hub
builder.Services.AddSignalR();


// Job repository and service
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();

//builder.Services.AddScoped<IJobService>(provider =>
//{
//    var jobRepository = provider.GetRequiredService<IJobRepository>();
//    return new Services.JobService(jobRepository);
//});

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

//emergency
builder.Services.AddScoped<IEmergencyRequestRepository, EmergencyRequestRepository>();
builder.Services.AddScoped<IEmergencyRequestService, EmergencyRequestService>();
// Quotation services
builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationRepository, Repositories.QuotationRepositories.QuotationRepository>();
builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationServiceRepository, Repositories.QuotationRepositories.QuotationServiceRepository>();
// Update to use the new QuotationServicePartRepository
// builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationPartRepository, Repositories.QuotationRepositories.QuotationPartRepository>();
builder.Services.AddScoped<Repositories.QuotationRepositories.IQuotationServicePartRepository, Repositories.QuotationRepositories.QuotationServicePartRepository>();
builder.Services.AddScoped<Services.QuotationServices.IQuotationService>(provider =>
{
    var quotationRepository = provider.GetRequiredService<Repositories.QuotationRepositories.IQuotationRepository>();
    var quotationServiceRepository = provider.GetRequiredService<Repositories.QuotationRepositories.IQuotationServiceRepository>();
    var quotationServicePartRepository = provider.GetRequiredService<Repositories.QuotationRepositories.IQuotationServicePartRepository>();
    var serviceRepository = provider.GetRequiredService<Repositories.ServiceRepositories.IServiceRepository>();
    var partRepository = provider.GetRequiredService<Repositories.PartRepositories.IPartRepository>();
    var repairOrderRepository = provider.GetRequiredService<Repositories.IRepairOrderRepository>();
    var jobService = provider.GetRequiredService<Services.IJobService>(); // Add this
    var mapper = provider.GetRequiredService<IMapper>();
    
    return new Services.QuotationServices.QuotationManagementService(
        quotationRepository,
        quotationServiceRepository,
        quotationServicePartRepository,
        serviceRepository,
        partRepository,
        repairOrderRepository,
        jobService, // Add this parameter
        mapper);
});
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
builder.Services.AddScoped<IPartService, PartService>();

builder.Services.AddHostedService<LogCleanupService>();

// repair request
builder.Services.AddScoped<IRequestPartRepository, RequestPartRepository>();
builder.Services.AddScoped<IRequestServiceRepository, RequestServiceRepository>();
builder.Services.AddScoped<IRepairRequestRepository, RepairRequestRepository>();
builder.Services.AddScoped<IRepairRequestService, RepairRequestService>();
// Nếu dùng Scoped lifetime như các repository khác
builder.Services.AddScoped<IRepairImageRepository, RepairImageRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// vehicle
//vehicle

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IVehicleBrandRepository, VehicleBrandRepository>();
builder.Services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
builder.Services.AddScoped<IVehicleColorRepository, VehicleColorRepository>();
builder.Services.AddScoped<IVehicleBrandServices, VehicleBrandService>();
builder.Services.AddScoped<IVehicleModelService, VehicleModelService>();
builder.Services.AddScoped<IVehicleColorService, VehicleColorService>();

//PAYMENT
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();




// Repositories & Services
builder.Services.AddScoped<IPromotionalCampaignRepository, PromotionalCampaignRepository>();
builder.Services.AddScoped<IPromotionalCampaignService, PromotionalCampaignService>();



builder.Services.AddScoped<IRevenueService, RevenueService>();



// Inspection services
builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<IInspectionService>(provider =>
{
    var inspectionRepository = provider.GetRequiredService<IInspectionRepository>();
    var repairOrderRepository = provider.GetRequiredService<IRepairOrderRepository>();
    var quotationService = provider.GetRequiredService<Services.QuotationServices.IQuotationService>();
    return new InspectionService(inspectionRepository, repairOrderRepository, quotationService);
});

builder.Services.AddScoped<IGeocodingService, GoongGeocodingService>();

// Technician services
builder.Services.AddScoped<ITechnicianService, TechnicianService>();

// Repair Request services - Adding missing registrations
builder.Services.AddScoped<Repositories.Customers.IRepairRequestRepository, Repositories.Customers.RepairRequestRepository>();
builder.Services.AddScoped<Services.Customer.IRepairRequestService>(provider =>
{
    var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
    var cloudinaryService = provider.GetRequiredService<ICloudinaryService>();
    var mapper = provider.GetRequiredService<IMapper>();
    var repairOrderService = provider.GetRequiredService<IRepairOrderService>();
    var vehicleService = provider.GetRequiredService<IVehicleService>();
    
    return new Services.Customer.RepairRequestService(
        unitOfWork,
        cloudinaryService,
        mapper,
        repairOrderService,
        vehicleService
    );
});

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

builder.Services.AddHttpClient();
builder.Services.AddScoped<IFacebookMessengerService, FacebookMessengerService>();


builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
// Database configuration - FIXED: Removed misplaced async and fixed the configuration
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<MyAppDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

builder.Services.Configure<PayOsOptions>(builder.Configuration.GetSection("PayOs"));
builder.Services.AddHttpClient<IPayOsClient, PayOsClient>();

builder.Services.AddHostedService<CampaignExpirationService>();
builder.Services.AddHostedService<PayOsWebhookProcessor>();

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

// Register FcmService
builder.Services.AddScoped<IFcmService, FcmService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAndAndroid", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:3001",
                "http://192.168.1.96:5117",
                "http://192.168.1.98:5117",
                "http://10.42.97.46:5117",
                "http://10.224.41.46:5117",
                "http://10.0.2.2:7113" // Android emulator
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

RepairRequestAppConfig.Initialize(builder.Configuration);

// Cấu hình Kestrel lắng nghe mọi IP với HTTP & HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5117, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });

    options.ListenAnyIP(7113, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1; // 👈 Bắt buộc thêm dòng này
    });
});

var app = builder.Build();
app.UseCors("AllowFrontendAndAndroid");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



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
app.MapHub<RepairHub>("/hubs/repair");

app.UseAuthentication();

app.UseMiddleware<UserActivityMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.UseSecurityPolicyEnforcement();
app.MapControllers();

// Add this line to map the SignalR hub
app.MapHub<Services.Hubs.RepairOrderHub>("/api/repairorderhub");
app.MapHub<Garage_pro_api.Hubs.OnlineUserHub>("/api/onlineuserhub");


//Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
    Console.WriteLine("Applying pending migrations...");
    // dbContext.Database.Migrate(); // Commented out to avoid conflict with existing tables

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