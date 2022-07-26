﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace BasicAuthentication.Handler
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
         IOptionsMonitor<AuthenticationSchemeOptions> options,
         ILoggerFactory logger,
         UrlEncoder encoder,
         ISystemClock clock
         ) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("appsettings.json"));
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Basic ".Length).Trim();
                System.Console.WriteLine(token);
                var credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var credentials = credentialstring.Split(':');
                if (credentials[0] == Convert.ToString(config.User) &&  credentials[1] == Convert.ToString(config.Password))
                {
                    var claims = new[] { new Claim("name", credentials[0]), new Claim(ClaimTypes.Role, "Admin") };
                    var identity = new ClaimsIdentity(claims, "Basic");
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                }

                Response.StatusCode = 401;
                Response.Headers.Add("WWW-Authenticate", "Basic realm=\"dotnetthoughts.net\"");
                return Task.FromResult(AuthenticateResult.Fail("Invalid Não autorizado"));
            }
            else
            {
                Response.StatusCode = 401;
                Response.Headers.Add("WWW-Authenticate", "Basic realm=\"dotnetthoughts.net\"");
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }
    }
}
