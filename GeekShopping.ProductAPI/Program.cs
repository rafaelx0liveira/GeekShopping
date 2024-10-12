using Asp.Versioning;
using AutoMapper;
using GeekShopping.ProductAPI.Config;
using GeekShopping.ProductAPI.Model.Context;
using GeekShopping.ProductAPI.Repository;
using GeekShopping.ProductAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configure connection to MySQL database
var connectionString = builder.Configuration.GetSection("MySQLConnection")["MySQLConnectionString"];
builder.Services.AddDbContext<MySQLContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add AutoMapper configuration
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();

// Add AutoMapper to the DI container
builder.Services.AddSingleton(mapper);

// Add services to the container.

/*
    O parâmetro AppDomain.CurrentDomain.GetAssemblies() instrui o AutoMapper a escanearem 
    todos os assemblies carregados no domínio da aplicação em busca de classes de mapeamento 
    (classes que herdam de Profile ou implementam um mapeamento manual).
*/
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add repositories and implementations to the DI container
builder.Services.AddScoped<IProductRepository, ProductRepository>();


// Add API versioning to the DI container
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add versioned API explorer to generate correct Swagger documentation
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration to support API versioning
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "GeekShopping V1", Version = "v1.0" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "GeekShopping V2", Version = "v2.0" });
});

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Swagger endpoints for each version
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GeekShopping V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();