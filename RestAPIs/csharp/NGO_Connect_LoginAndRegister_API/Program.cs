using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NGO_Connect_LoginAndRegister_API.Data;
using NGO_Connect_LoginAndRegister_API.Models;
using System.Text;
using Steeltoe.Discovery.Eureka; // <--- NEW: Eureka Namespace
using Steeltoe.Discovery.Client; // <--- NEW: Steeltoe Namespace
using Steeltoe.Discovery;



var builder = WebApplication.CreateBuilder(args);

// ==========================================
// NEW: DISCOVERY SERVICE REGISTRATION
// ==========================================
builder.Services.AddDiscoveryClient(builder.Configuration);


// ==========================================
// 1. DATABASE CONNECTION (MySQL)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<P01NgoConnectContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ==========================================
// 2. JWT AUTHENTICATION SETUP
// ==========================================
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// ==========================================
// 3. STANDARD SERVICES
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. CORS (Allow React App)
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp",
//        policy => policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
//                        .AllowAnyHeader()
//                        .AllowAnyMethod());
//});

var app = builder.Build();


// use discovery client 
app.UseDiscoveryClient();

// ==========================================
// 4. PIPELINE CONFIGURATION
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();