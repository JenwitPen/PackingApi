using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace PackingApi.Authentication
{
    public class UserKeyValidators
    {
        private readonly RequestDelegate _next;

        public UserKeyValidators(RequestDelegate next)
        {
            _next = next;

        }

        public async Task Invoke(HttpContext context)
        {
            string token = "";
            if (context.Request.Headers.TryGetValue("api-key", out StringValues headerValue))
            {
                token = headerValue.ToString();
            }

            if (String.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 400; //Bad Request                
                await context.Response.WriteAsync("User Key is missing");
                return;
            }
            else
            {
                if (!CheckValidUserKey(token))
                {
                    context.Response.StatusCode = 401; //UnAuthorized
                    await context.Response.WriteAsync("Invalid User Key");
                    return;
                }
            }

            await _next.Invoke(context);
        }
        private bool CheckValidUserKey(string reqkey)
        {
            var userkeyList = new List<string>();
            userkeyList.Add("1234");

            if (userkeyList.Contains(reqkey))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    #region ExtensionMethod
    public static class UserKeyValidatorsExtension
    {
        public static IApplicationBuilder ApplyUserKeyValidation(this IApplicationBuilder app)
        {
            app.UseMiddleware<UserKeyValidators>();
            return app;
        }
    }
    #endregion
}
