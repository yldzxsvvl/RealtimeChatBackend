using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Core.Models;
using Microsoft.OpenApi.Models;
using API.Hubs;
using System.Text.Json.Serialization;
using System.IO;

var builder = WebApplication.CreateBuilder(args);


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddSignalR();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5222", "null") 
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddSingleton(jwtSettings);

builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorageSettings"));
var fileStorageSettings = builder.Configuration.GetSection("FileStorageSettings").Get<FileStorageSettings>()!;
builder.Services.AddSingleton(fileStorageSettings);


builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
var redisSettings = builder.Configuration.GetSection("RedisSettings").Get<RedisSettings>()!;
builder.Services.AddSingleton(redisSettings);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();


builder.Services.AddSingleton<ICacheService, InMemoryCacheService>(); 

var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chatHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Swagger (API dokümantasyonu) ayarları
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealtimeChatBackend", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

var app = builder.Build();

// wwwroot klasöründeki dosyaları sunmak için ayarlar
var webRootPath = app.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRootPath))
{
    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    if (!Directory.Exists(webRootPath))
    {
        Directory.CreateDirectory(webRootPath);
    }
}
var uploadsPath = Path.Combine(webRootPath, fileStorageSettings.UploadsFolder);
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware (ara yazılım) sıralaması
app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
