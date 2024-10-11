using AutoMapper;
using GeekShopping.ProductAPI.Config;
using GeekShopping.ProductAPI.Model.Context;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configurar a string de conexão e adicionar o DbContext
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

// Add services to the container.
builder.Services.AddControllers();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
