using Azure.Core;
using LIMSAPI.Helpers;
using LIMSAPI.Helpers.Email;
using LIMSAPI.Helpers.SMS;
using LIMSAPI.RepositryLayer;
using LIMSAPI.RepositryLayer.Account.UserLogin;
using LIMSAPI.RepositryLayer.Account.UserRegistration;
using LIMSAPI.RepositryLayer.Email.EmailRepositry;
using LIMSAPI.RepositryLayer.OTP.OTPRespository;
using LIMSAPI.RepositryLayer.SMSRepository;
using LIMSAPI.ServiceLayer;
using LIMSAPI.ServiceLayer.Account.UserLogin;
using LIMSAPI.ServiceLayer.Account.UserRegistration;
using LIMSAPI.ServiceLayer.Email.EmailService;
using LIMSAPI.ServiceLayer.OTP.OTPService;
using LIMSAPI.ServiceLayer.SMS.SMSService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddScoped<AddFilter>();
builder.Services.AddScoped<DuplicateChecker>();
builder.Services.AddScoped<LIMSRepositryInterface, LIMSRepositry>();
builder.Services.AddScoped<LIMSServiceInterface, LIMSService>();

builder.Services.AddScoped<IUserRegistrationRL, UserRegistrationRL>();
builder.Services.AddScoped<IUserRegistrationSL, UserRegistrationSL>();

builder.Services.AddScoped<IUserLoginSL, UserLoginSL>();
builder.Services.AddScoped<IUserLoginRL, UserLoginRL>();

builder.Services.AddScoped<IMailRepositry, MailRepositry>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddScoped<IOTPRepository, OTPRepository>();

builder.Services.AddScoped<ISMSRepository, SMSRepository>();
builder.Services.AddScoped<ISMSService, SMSService>();
builder.Services.Configure<SMSSettings>(builder.Configuration.GetSection("SMSSettings"));

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
});

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

app.UseSession();

app.MapControllers();

app.Run();
