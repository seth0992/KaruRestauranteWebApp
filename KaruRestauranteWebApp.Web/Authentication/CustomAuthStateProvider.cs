using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KaruRestauranteWebApp.Web.Authentication
{
    public class CustomAuthStateProvider(ProtectedLocalStorage localStorage) : AuthenticationStateProvider
    {
        //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        //{
        //    var sessionModel = (await localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;
        //    var identity = sessionModel == null ? new ClaimsIdentity() : GetClaimsIdentity(sessionModel.Token);
        //    var user = new ClaimsPrincipal(identity);
        //    return new AuthenticationState(user);
        //}
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var sessionModel = (await localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;

                if (sessionModel == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Verificar si el token ha expirado
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(sessionModel.Token);

                // Comprobar si el token está expirado
                if (DateTime.UtcNow.Ticks > jwtToken.ValidTo.Ticks)
                {
                    // Si está expirado, intentar refresh automático
                    await MarkUserAsLoggedOut();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var identity = GetClaimsIdentity(sessionModel.Token);
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                // Si ocurre un error, limpiar estado de autenticación
                await MarkUserAsLoggedOut();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task MarkUserAsAuthenticated(LoginResponseModel model)
        {

            await localStorage.SetAsync("sessionState", model);
            var identity = GetClaimsIdentity(model.Token);
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

        }

        private ClaimsIdentity GetClaimsIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            return new ClaimsIdentity(claims, "jwt");
        }

        public async Task MarkUserAsLoggedOut()
        {
            await localStorage.DeleteAsync("sessionState");
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }
}
