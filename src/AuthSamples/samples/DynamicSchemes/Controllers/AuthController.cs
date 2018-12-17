using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AuthSamples.DynamicSchemes.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IOptionsMonitorCache<SimpleOptions> _optionsCache;
        private readonly IOptionsMonitorCache<OpenIdConnectOptions> _openIdConnectOptionsCache;
        private readonly OpenIdConnectPostConfigureOptions _openIdConnectPostConfigureOptions;

        public AuthController(
            IAuthenticationSchemeProvider schemeProvider,
            IOptionsMonitorCache<SimpleOptions> optionsCache,
            IOptionsMonitorCache<OpenIdConnectOptions> openIdConnectOptionsCache,
            OpenIdConnectPostConfigureOptions openIdConnectPostConfigureOptions)
        {
            _schemeProvider = schemeProvider;
            _optionsCache = optionsCache;
            _openIdConnectOptionsCache = openIdConnectOptionsCache;
            _openIdConnectPostConfigureOptions = openIdConnectPostConfigureOptions;
        }

        public IActionResult Remove(string scheme)
        {
            _schemeProvider.RemoveScheme(scheme);
            _optionsCache.TryRemove(scheme);
            return Redirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(string scheme, string optionsMessage)
        {
            if (await _schemeProvider.GetSchemeAsync(scheme) == null)
            {
                _schemeProvider.AddScheme(new AuthenticationScheme(scheme, scheme, typeof(SimpleAuthHandler)));
            }
            else
            {
                _optionsCache.TryRemove(scheme);
            }
            _optionsCache.TryAdd(scheme, new SimpleOptions { DisplayMessage = optionsMessage });
            return Redirect("/");
        }

        [HttpPost]
        public IActionResult AddOpenIdConnect()
        {
            _schemeProvider.AddScheme(new AuthenticationScheme("oidc", "OpenID Connect", typeof(OpenIdConnectHandler)));
            var options = new OpenIdConnectOptions
            {
                MetadataAddress = "https://XXX/.well-known/openid-configuration",
                ClientId = "XXX",
                ClientSecret = "XXX",
                CallbackPath = "/signin-oidc",
            };
            _openIdConnectPostConfigureOptions.PostConfigure("oidc", options);
            _openIdConnectOptionsCache.TryAdd("oidc", options);
            return Redirect("/");
        }
    }
}
