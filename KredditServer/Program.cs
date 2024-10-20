using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using kreddit_app.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();

// Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ObjectIdJsonConverter()); // Correctly add the converter here
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Services
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    new MongoClient(mongoDbSettings.ConnectionString));
builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbSettings.DatabaseName);
});

// Custom Services
builder.Services.AddRazorPages();
builder.Services.AddHttpClient(); // Ensure you add HttpClient without JsonOptions

builder.Services.AddScoped<ApiService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("MyCorsPolicy");

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers(); // Map API controllers
    endpoints.MapFallbackToFile("index.html"); // Fallback to serve index.html for client-side routing
});

app.Run();
