﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityServerHost.Quickstart.UI;

public class SecurityHeadersAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var result = context.Result;
        if (result is ViewResult)
        {
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
                context.HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";

            if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
                context.HttpContext.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";

            var csp = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";

            if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
                context.HttpContext.Response.Headers["Content-Security-Policy"] = csp;
            
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
                context.HttpContext.Response.Headers["X-Content-Security-Policy"] = csp;

            var referrer_policy = "no-referrer";
            if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
                context.HttpContext.Response.Headers["Referrer-Policy"] = referrer_policy;
        }
    }
}