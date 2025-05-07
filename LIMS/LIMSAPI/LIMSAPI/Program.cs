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

builder.Services.AddControllers();  
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());


app.MapControllers();

app.Run();
