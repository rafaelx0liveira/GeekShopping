using GeekShopping.Web.Models;
using GeekShopping.Web.Services.Interfaces;
using GeekShopping.Web.Utils;

namespace GeekShopping.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _client;
        public const string _basePath = "api/v1/product";

        public ProductService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IEnumerable<ProductModel>> FindAllProducts()
        {
            var response = await _client.GetAsync(_basePath);

            return await response.ReadContentAs<List<ProductModel>>();
        }

        public async Task<ProductModel> FindProductById(long id)
        {
            var response = await _client.GetAsync($"{_basePath}/{id}");

            return await response.ReadContentAs<ProductModel>();
        }

        public async Task<ProductModel> CreateProduct(ProductModel product)
        {
            var response = await _client.PostAsJson(_basePath, product);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error: {response.ReasonPhrase}");
            }

            return await response.ReadContentAs<ProductModel>();
        }

        public async Task<ProductModel> UpdateProduct(ProductModel product)
        {
            var response = await _client.PutAsJson(_basePath, product);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error: {response.ReasonPhrase}");
            }

            return await response.ReadContentAs<ProductModel>();
        }

        public async Task<bool> DeleteProductById(long id)
        {
            var response = await _client.DeleteAsync($"{_basePath}/{id}");

            if (!response.IsSuccessStatusCode) {
                throw new HttpRequestException($"Error: {response.ReasonPhrase}");
            }

            return await response.ReadContentAs<bool>();
        }
    }
}
