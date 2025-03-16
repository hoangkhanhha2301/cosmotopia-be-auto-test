using CloudinaryDotNet;
using Cosmetics.DTO.User;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Cosmetics.Repositories;
using Cosmetics.Repositories.UnitOfWork;
using Cosmetics.Service.Affiliate;
using Cosmetics.Service.OTP;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;



var builder = WebApplication.CreateBuilder(args);
// JWT Configuration
builder.Services.Configure<AppSetting>(builder.Configuration.GetSection("AppSettings"));
var secretKey = builder.Configuration["AppSettings:SecretKey"];
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ClockSkew = TimeSpan.Zero,
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(3); // Set cookie expiration time to 3 hours
    options.SlidingExpiration = true;
});

// CORS Configuration
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("corspolicy", build =>
    {
        build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    });
});

// Swagger Configuration
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
    option.EnableAnnotations();
});



// Add services to the container
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 443; // Cổng HTTPS của SmarterASP.NET
});


builder.Services.AddDbContext<ComedicShopDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add IHttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>(sp => new EmailService(
    smtpServer: "smtp.gmail.com",
    smtpPort: 587,
    smtpEmail: "courtb454@gmail.com",
    smtpPassword: "riqc mncz rljd wjrv",
    logger: sp.GetRequiredService<ILogger<EmailService>>()
));


builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

//Cloudinary
var cloudinaryAccount = new Account(
        builder.Configuration["Cloudinary:CloudName"],
        builder.Configuration["Cloudinary:ApiKey"],
        builder.Configuration["Cloudinary:ApiSecret"]
    );


var cloudinary = new Cloudinary(cloudinaryAccount);
builder.Services.AddSingleton(cloudinary);

// Add Repositories and Interfaces
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAffiliateProfileRepository, AffiliateProfileRepository>();
//builder.Services.AddScoped<IAffiliateLinkRepository, AffiliateLinkRepository>();
//Add Services
builder.Services.AddHostedService<ExpiredOtpCleanerService>();
// Đăng ký IAffiliateService với AffiliateService
builder.Services.AddScoped<IAffiliateService, AffiliateService>();

// Đăng ký IProductService (nếu đã có ProductService)
builder.Services.AddScoped<IProductService, ProductService>();



// Learn more about configuring Swagger/OpenAPI at
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Other service registrations

var app = builder.Build();

// Configure middleware, static files, authentication, authorization, etc.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("corspolicy");

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
