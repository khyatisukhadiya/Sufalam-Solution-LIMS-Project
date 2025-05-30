using Azure.Core;
using LIMSAPI.Helpers;
using LIMSAPI.RepositryLayer;
using LIMSAPI.ServiceLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddScoped<AddFilter>();
builder.Services.AddScoped<DuplicateChecker>();
builder.Services.AddScoped<LIMSRepositryInterface, LIMSRepositry>();
builder.Services.AddScoped<LIMSServiceInterface, LIMSService>();
builder.Services.AddScoped<SqlConnection>(sp =>
{
    var configuration = builder.Configuration;
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});


// Add services to the container.
//builder.Services.AddControllers().AddJsonOptions(x =>
//    x.JsonSerializerOptions.PropertyNameCaseInsensitive = true);


//builder.Services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null); response capital

builder.Services.AddControllers();
    //.ConfigureApiBehaviorOptions(options =>
    //{
    //    options.SuppressModelStateInvalidFilter = true;
    //});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Automatically redirects HTTP requests to HTTPS.

app.UseAuthorization(); // authorization checks based on user identity and roles.

app.UseCors(policy =>  // Browsers block requests from one origin to another(for security).
                       // For example: Frontend on http://localhost:4200 calling backend on https://localhost:5001 → Blocked without CORS.
                      policy.AllowAnyHeader()  // Allow all HTTP headers (like Content-Type, Authorization)
                            .AllowAnyOrigin()  // Allow requests from any domain (e.g., http://localhost:4200)
                            .AllowAnyMethod()); // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)


app.MapControllers();

app.Run();
