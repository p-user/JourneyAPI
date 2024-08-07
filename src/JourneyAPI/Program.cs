using DataAccessLayer;
using DataAccessLayer.Config;
using DataAccessLayer.Entities;
using JourneyAPI.Middlewares;
using BusinessLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Text;
using Microsoft.OpenApi.Models;
using JourneyAPI.Notifications;
using BusinessLayer.Services;
using JourneyAPI;

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();
try
{


    var builder = WebApplication.CreateBuilder(args);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    var services = builder.Services;


    services.AddDbContext<ApiDbContext>(
       (sp, opt) =>
       {
           opt.UseSqlServer(connectionString)
             .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>())
             .AddInterceptors(sp.GetRequiredService<UpdateAuditableInterceptor>());
       });

    services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

    services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddRoleManager<RoleManager<ApplicationRole>>()
    .AddDefaultTokenProviders();

    var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);
    var audience = builder.Configuration.GetSection("JwtConfig:ValidAudience").Value;
    var issuer = builder.Configuration.GetSection("JwtConfig:ValidIssuer").Value;

    var tokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        ValidateAudience = true


    };

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })

    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameters;
    });

    services.AddAuthorization();


    // Add services to the container.
    services.AddServicesBusiness();
    services.AddServices();
   


    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    // builder.Services.AddSwaggerGen();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Journey API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });

        
    });


    services.AddSignalR();
    services.AddHostedService<ServerTimeNotifier>();

    services.AddCors();

    var app = builder.Build();

    using var scope = app.Services.CreateScope();
    var databaseInitializer = scope.ServiceProvider;
    SeedData.Initialize(databaseInitializer).Wait();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ExceptionsMiddleware>();

    app.MapControllers();
    app.MapHub<NotificationsHub>("/notificationHub");

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Exception thrown!");
    throw ex;
}