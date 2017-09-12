using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Demo.Web.Middlewares
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions Options;
        private readonly JsonSerializerSettings _serializerSettings;
        IPasswordHasher<IdentityUser> PasswordHasher;
        UserManager<IdentityUser> UserManager;

        public TokenProviderMiddleware(RequestDelegate next, IOptions<TokenProviderOptions> options
            , UserManager<IdentityUser> userManager, IPasswordHasher<IdentityUser> passwordHasher)
        {
            _next = next;

            this.UserManager = userManager;
            this.PasswordHasher = passwordHasher;

            Options = options.Value;
            ThrowIfInvalidOptions(Options);

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals(Options.Path, StringComparison.Ordinal))
            {
                return _next(context);
            }

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
                || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }


            return GenerateToken(context);
        }

        #region ThrowIfInvalidOptions

        private static void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (options.Expiration == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }


            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }


        }

        #endregion

        #region Generate Token

        private async Task GenerateToken(HttpContext context)
        {
            var username = context.Request.Form["username"];
            var password = context.Request.Form["password"];


            var user = await this.UserManager.FindByNameAsync(username);


            if (user == null || PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Success)
            {
                context.Response.StatusCode = 400;
                var error = new
                {
                    error = "Error"
                };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error, _serializerSettings));
                return;
            }


            var jwt = await GetJwtSecurityToken(user);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var tokenResponse = new 
            {
                Access_token = encodedJwt,
                Token_type = "bearer",
                Expires_in = Options.Expiration.TotalSeconds.ToString(),
                Expires = DateTime.UtcNow.Add(Options.Expiration).ToString()
            };

            var reponse = new 
            {
                Data = tokenResponse
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonConvert.SerializeObject(reponse, _serializerSettings));
        }

        private async Task<JwtSecurityToken> GetJwtSecurityToken(IdentityUser user)
        {
            var userClaims = await this.UserManager.GetClaimsAsync(user);

            return new JwtSecurityToken(
                expires: DateTime.UtcNow.Add(Options.Expiration),
                claims: GetTokenClaims(user).Union(userClaims),
                signingCredentials: Options.SigningCredentials
            );
        }

        private IEnumerable<Claim> GetTokenClaims(IdentityUser user)
        {
            return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id)
            };
        }

        #endregion

    }
}
