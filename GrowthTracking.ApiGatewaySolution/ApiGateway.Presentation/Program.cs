using ApiGateway.Presentation.Middleware;
using GrowthTracking.ShareLibrary.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// For development - ignore SSL certificate validation issues
if (builder.Environment.IsDevelopment())
{
    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
}

builder.Services.AddOcelot().AddCacheManager(x => x.WithDictionaryHandle());

// Authentication disabled for testing purposes
// JWTAuthencationScheme.AddJWTAuthencationScheme(builder.Services, builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors();

app.UseMiddleware<AttachSignatureToRequest>();

app.UseOcelot().Wait();

app.Run();

