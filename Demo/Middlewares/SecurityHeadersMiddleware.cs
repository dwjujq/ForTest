﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SilverSoft.Middlewares.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SilverSoft.Middlewares
{
    /// <summary>
    /// An ASP.NET middleware for adding security headers.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SecurityHeadersPolicy _policy;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Instantiates a new <see cref="SecurityHeadersMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="policy">An instance of the <see cref="SecurityHeadersPolicy"/> which can be applied.</param>
        public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersPolicy policy, IConfiguration configuration)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            _next = next;
            _policy = policy;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.Response;

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var headers = response.Headers;

            if (_configuration.GetValue<bool>("SetSecurityHeaders", false))
            {
                foreach (var headerValuePair in _policy.SetHeaders)
                {
                    headers[headerValuePair.Key] = headerValuePair.Value;
                }

                foreach (var header in _policy.RemoveHeaders)
                {
                    headers.Remove(header);
                }
            }

            await _next(context);
        }

    }
}
