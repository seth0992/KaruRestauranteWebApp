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

                // Verificar si el token ha expirado de forma más robusta
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(sessionModel.Token);

                // Compara fechas directamente en lugar de usar Ticks
                if (DateTime.UtcNow > jwtToken.ValidTo)
                {
                    // Agregar un log para diagnóstico
                    Console.WriteLine($"Token expirado: {jwtToken.ValidTo}");

                    // Redirigir a token expirado en lugar de logout directamente
                    await MarkUserAsLoggedOut();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Agregar validación para tokens que están a punto de expirar (5 minutos)
                if (jwtToken.ValidTo - DateTime.UtcNow < TimeSpan.FromMinutes(5))
                {
                    // Intentar refresh automático con ApiClient
                    // Esto sería implementado como un servicio extra
                }

                var identity = GetClaimsIdentity(sessionModel.Token);
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                // Agrega un log del error para mejor diagnóstico
                Console.WriteLine($"Error en autenticación: {ex.Message}");

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
