using GeekShopping.Web.Services;
using GeekShopping.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Get the base address for the ProductAPI from the configuration.
var baseAddress = builder.Configuration.GetSection("ServiceUrls")["ProductAPI"];

// Validate the base address.
if (string.IsNullOrEmpty(baseAddress))
{
    throw new InvalidOperationException("The base address for the ProductAPI is not configured.");
}

// Add the ProductService to the DI container.
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    // Set the base address of the ProductAPI.
    client.BaseAddress = new Uri(baseAddress);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
