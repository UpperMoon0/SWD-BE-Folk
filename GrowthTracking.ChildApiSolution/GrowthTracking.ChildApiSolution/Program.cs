using ChildApi.Infrastructure.Data;
using ChildApi.Infrastructure.Mapping;
using ChildApi.Infrastructure.Repositories;
using ChildApi.Application.Interfaces;
using ChildApi.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using GrowthTracking.ShareLibrary.Response;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Thêm Controllers vào container
builder.Services.AddControllers();

// Thêm Swagger cho tài liệu API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();

// Cấu hình DbContext với connection string từ file cấu hình (appsettings.json)
builder.Services.AddDbContext<ChildDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Mapster configuration
MapsterConfiguration.RegisterMappings();

// Đăng ký repository interfaces và implementations
builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();

// Đăng ký ParentIdCache và RabbitMQ Consumer (BackgroundService)
builder.Services.AddSingleton<ParentIdCache>();
builder.Services.AddHostedService<ParentEventConsumer>();

// Add authentication with a default scheme but don't enforce it
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            RequireSignedTokens = false,
            RequireExpirationTime = false
        };
    });

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            Console.WriteLine($"Error: {contextFeature.Error}");
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiResponse 
            { 
                Success = false, 
                Message = "Internal Server Error. Please try again later.",
                Data = app.Environment.IsDevelopment() ? contextFeature.Error.ToString() : null
            }));
        }
    });
});

// Cấu hình Swagger cho môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChildApi v1");
        c.RoutePrefix = string.Empty;
    });
}

// Add CORS for API testing
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

// Uncomment this line to use authentication - we're keeping it commented
// but now the app has a default scheme configured
// app.UseAuthentication();

// Keep authorization middleware for role-based permissions if needed later
app.UseAuthorization();

app.MapControllers();

app.Run();
