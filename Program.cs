
using CampusEats.Api.Data;
using CampusEats.Api.Models;
using CampusEats.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// DATABASE - PostgreSQL + Entity Framework Core
// ======================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")));

// ======================================================
// SERVICES
// ======================================================

builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ======================================================
// CORS - ALLOW REACT FRONTEND
// ======================================================

var frontendOrigin = builder.Configuration["Frontend:Origin"];

var allowedOrigins = new List<string>
{
    "http://localhost:5173"
};

if (!string.IsNullOrWhiteSpace(frontendOrigin))
{
    allowedOrigins.Add(frontendOrigin.TrimEnd('/'));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ======================================================
// JWT AUTHENTICATION
// ======================================================

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// ======================================================
// CONTROLLERS
// ======================================================

builder.Services.AddControllers();

// ======================================================
// SWAGGER
// ======================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste ONLY the token - no 'Bearer ' prefix."
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
            Array.Empty<string>()
        }
    });
});

// ======================================================
// BUILD APPLICATION
// ======================================================

var app = builder.Build();

// ======================================================
// SEED INITIAL MENU ITEMS
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    if (!db.MenuItems.Any())
    {
        db.MenuItems.AddRange(
            new MenuItem
            {
                Name = "Kottu Roti",
                Price = 750m,
                Category = "Mains",
                Available = true
            },

            new MenuItem
            {
                Name = "Fried Rice",
                Price = 850m,
                Category = "Mains",
                Available = true
            },

            new MenuItem
            {
                Name = "Watalappan",
                Price = 350m,
                Category = "Dessert",
                Available = true
            }
        );

        db.SaveChanges();
    }
}

// ======================================================
// HTTP REQUEST PIPELINE
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

// Allow requests from the configured React frontend
app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
