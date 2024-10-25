using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Repository.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GeekShopping.CartAPI.Repository;

public class CouponRepository(HttpClient client) : ICouponRepository
{
    private readonly HttpClient _client = client;

    public async Task<CouponVO?> GetByCode(string couponCode, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"/api/v1/coupon/{couponCode}");

        if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Unable to connect to Coupon API");

        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK) return new CouponVO();
        return JsonSerializer.Deserialize<CouponVO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}