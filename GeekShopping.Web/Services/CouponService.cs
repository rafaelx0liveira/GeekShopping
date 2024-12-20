﻿using System.Net.Http.Headers;
using GeekShopping.Web.Utils;
using System.Net;
using GeekShopping.Web.Services.Interfaces;
using GeekShopping.Web.Models;

namespace GeekShopping.Web.Services;

public class CouponService : ICouponService
{
    private readonly HttpClient _client;
    public const string BasePath = "api/v1/coupon";

    public CouponService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<CouponViewModel?> GetCoupon(string code, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{BasePath}/{code}");
        if (response.StatusCode != HttpStatusCode.OK) return new CouponViewModel();
        return await response.ReadContentAs<CouponViewModel?>();
    }
}