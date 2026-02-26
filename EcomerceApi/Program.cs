using Business.Data;
using Business.Logic;
using Business.Logic.AuthLogic;
using Business.Logic.CategoryLogic;
using Business.Logic.CustomerAddress;
using Business.Logic.CartLogic;
using Business.Logic.OrderLogic;
using Business.Logic.OrderStatusLogic;
using Business.Logic.OrderTypeLogic;
using Business.Logic.ProductLogic;
using Business.Logic.ProfileLogic;
using Business.Logic.UserLogic;
using Business.Mappings;
using Core.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Business.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBusiness(builder.Configuration);
builder.Services.AddScoped(typeof(IGenericRepository<>), (typeof(GenericRepository<>)));
builder.Services.AddTransient<UserResponse>();


builder.Services.AddTransient<AuthResponse>();
builder.Services.AddTransient<ProfileResponse>();
builder.Services.AddTransient<UserResponse>();
builder.Services.AddTransient<CategoryResponse>();
builder.Services.AddTransient<CustomerAddressResponse>();
builder.Services.AddTransient<OrderResponse>();
builder.Services.AddTransient<OrderStatusResponse>();
builder.Services.AddTransient<OrderTypeResponse>();
builder.Services.AddTransient<ProductResponse>();
builder.Services.AddTransient<CartResponse>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = int.MaxValue; });

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EcomerceDbContext>(x => x.UseSqlServer(connectionString!));

// HttpClient para MercadoPago REST API
builder.Services.AddHttpClient("MercadoPago", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri("https://api.mercadopago.com/");
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config["MercadoPago:AccessToken"]);
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();