using Blazored.Toast.Services;
using KaruRestauranteWebApp.Models.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using KaruRestauranteWebApp.Web.Authentication;

namespace KaruRestauranteWebApp.Web;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IToastService _toastService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private bool _isRefreshing = false;
    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public ApiClient(
        HttpClient httpClient,
        NavigationManager navigationManager,
        ProtectedLocalStorage localStorage,
        IToastService toastService,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        _localStorage = localStorage;
        _toastService = toastService;
        _authStateProvider = authStateProvider;
    }

    private async Task<string> GetTokenAsync()
    {
        var sessionState = (await _localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;
        return sessionState?.Token ?? string.Empty;
    }

    private async Task SetAuthorizationHeader()
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<bool> RefreshTokenAsync()
    {
        try
        {
            await _semaphore.WaitAsync();
            if (_isRefreshing) return true;

            _isRefreshing = true;
            var sessionState = (await _localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;

            if (sessionState == null || string.IsNullOrEmpty(sessionState.RefreshToken))
            {
                return false;
            }

            // Quitar el header de autorización para la llamada de refresh
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var response = await _httpClient.GetAsync($"api/Auth/loginByRefreshToken?refreshToken={sessionState.RefreshToken}");

            if (response.IsSuccessStatusCode)
            {
                var newSession = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
                if (newSession != null)
                {
                    await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(newSession);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error al refrescar el token: {ex.Message}");
            return false;
        }
        finally
        {
            _isRefreshing = false;
            _semaphore.Release();
        }
    }

    private async Task HandleUnauthorizedResponse()
    {
        var refreshSuccess = await RefreshTokenAsync();
        if (!refreshSuccess)
        {
            await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
            _navigationManager.NavigateTo("/login", true);
        }
    }

    public async Task<T?> PatchAsync<T>(string url)
    {
        var response = await _httpClient.PatchAsync(url, null);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
        return default;
    }
    public async Task<T?> GetFromJsonAsync<T>(string requestUri)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.GetAsync(requestUri);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TResponse, TRequest>(string requestUri, TRequest content)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync(requestUri, content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.PostAsJsonAsync(requestUri, content);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    public async Task<TResponse?> PutAsync<TResponse, TRequest>(string requestUri, TRequest content)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync(requestUri, content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.PutAsJsonAsync(requestUri, content);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string requestUri)
    {
        try
        {
            await SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(requestUri);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    response = await _httpClient.DeleteAsync(requestUri);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            await HandleErrorResponse(response);
            return default;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return default;
        }
    }


    public async Task<List<T>> GetListAsync<T>(string requestUri) where T : class
    {
        try
        {
            // Obtener la respuesta HTTP directamente para poder examinar su contenido
            await SetAuthorizationHeader();
            var httpResponse = await _httpClient.GetAsync(requestUri);

            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    httpResponse = await _httpClient.GetAsync(requestUri);
                }
            }

            // Leer el contenido JSON sin procesar
            string rawJson = await httpResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Raw JSON response: {rawJson}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                _toastService.ShowError($"Error en la solicitud: {httpResponse.StatusCode}");
                return new List<T>();
            }

            // Primero intentamos deserializar a BaseResponseModel
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var baseResponse = System.Text.Json.JsonSerializer.Deserialize<BaseResponseModel>(rawJson, options);
                if (baseResponse != null && baseResponse.Success && baseResponse.Data != null)
                {
                    // Convertir Data a JSON y luego a List<T>
                    string dataJson = System.Text.Json.JsonSerializer.Serialize(baseResponse.Data);
                    Console.WriteLine($"Data JSON: {dataJson}");

                    try
                    {
                        var list = System.Text.Json.JsonSerializer.Deserialize<List<T>>(dataJson, options);
                        return list ?? new List<T>();
                    }
                    catch (Exception dataEx)
                    {
                        Console.WriteLine($"Error deserializing Data to List<{typeof(T).Name}>: {dataEx.Message}");

                        // Intento alternativo: Podría ser un objeto individual, no una lista
                        try
                        {
                            // Si es un objeto único, lo envolvemos en una lista
                            var singleItem = System.Text.Json.JsonSerializer.Deserialize<T>(dataJson, options);
                            if (singleItem != null)
                            {
                                return new List<T> { singleItem };
                            }
                        }
                        catch
                        {
                            // Ignoramos este error y continuamos
                        }
                    }
                }

                // Si llegamos aquí, algo falló en la deserialización
                _toastService.ShowError("No se pudieron procesar los datos recibidos");
                return new List<T>();
            }
            catch (Exception baseEx)
            {
                Console.WriteLine($"Error deserializing to BaseResponseModel: {baseEx.Message}");

                // Intentar deserializar directamente a List<T>
                try
                {
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<T>>(rawJson, options);
                    return list ?? new List<T>();
                }
                catch (Exception listEx)
                {
                    Console.WriteLine($"Error deserializing directly to List<{typeof(T).Name}>: {listEx.Message}");
                    _toastService.ShowError($"Error al procesar los datos: {listEx.Message}");
                    return new List<T>();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error in GetListAsync: {ex.Message}");
            _toastService.ShowError($"Error: {ex.Message}");
            return new List<T>();
        }
    }
    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var error = await response.Content.ReadAsStringAsync();
        _toastService.ShowError($"Error: {response.StatusCode} - {error}");
    }

    private void HandleException(Exception ex)
    {
        _toastService.ShowError($"Error en la solicitud: {ex.Message}");
    }
}

