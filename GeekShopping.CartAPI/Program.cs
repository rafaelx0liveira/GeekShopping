using Asp.Versioning;
using AutoMapper;
using GeekShopping.CartAPI.Config;
using GeekShopping.CartAPI.Model.Context;
using GeekShopping.CartAPI.Repository;
using GeekShopping.CartAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
builder.Services.AddScoped<ICartRepository, CartRepository>();

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "GeekShopping.CartAPI V1", Version = "v1.0" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "GeekShopping.CartAPI V2", Version = "v2.0" });

    options.EnableAnnotations(); // Enable annotations in Swagger documentation

    // Add JWT Bearer authentication (Pop-up for JWT token)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. Example: \"
        + "Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Add controllers
builder.Services.AddControllers();

// Authentication server URL
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["ServiceUrls:IdentityServer"];
        options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
    });

// Authorization policy
builder.Services.AddAuthorizationBuilder()
.AddPolicy("ApiScope", policy =>
{
    policy.RequireAuthenticatedUser();
    policy.RequireClaim("scope", "geek_shopping");
});

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();