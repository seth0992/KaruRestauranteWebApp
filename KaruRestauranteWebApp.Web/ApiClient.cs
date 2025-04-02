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

    //private async Task<bool> RefreshTokenAsync()
    //{
    //    try
    //    {
    //        await _semaphore.WaitAsync();
    //        if (_isRefreshing) return true;

    //        _isRefreshing = true;
    //        var sessionState = (await _localStorage.GetAsync<LoginResponseModel>("sessionState")).Value;

    //        if (sessionState == null || string.IsNullOrEmpty(sessionState.RefreshToken))
    //        {
    //            return false;
    //        }

    //        // Quitar el header de autorización para la llamada de refresh
    //        _httpClient.DefaultRequestHeaders.Authorization = null;

    //        var response = await _httpClient.GetAsync($"api/Auth/loginByRefreshToken?refreshToken={sessionState.RefreshToken}");

    //        if (response.IsSuccessStatusCode)
    //        {
    //            var newSession = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
    //            if (newSession != null)
    //            {
    //                await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(newSession);
    //                return true;
    //            }
    //        }

    //        return false;
    //    }
    //    catch (Exception ex)
    //    {
    //        _toastService.ShowError($"Error al refrescar el token: {ex.Message}");
    //        return false;
    //    }
    //    finally
    //    {
    //        _isRefreshing = false;
    //        _semaphore.Release();
    //    }
    //}

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
                await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
                _navigationManager.NavigateTo("/login", true);
                return false;
            }

            // Quitar el header de autorización para la llamada de refresh
            _httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
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

                // Si no se pudo refrescar, cerrar sesión
                await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
                _navigationManager.NavigateTo("/login", true);
                return false;
            }
            catch
            {
                // Si ocurre cualquier error en el proceso de refresh, cerrar sesión
                await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
                _navigationManager.NavigateTo("/login", true);
                return false;
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error al refrescar el token: {ex.Message}");
            await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
            _navigationManager.NavigateTo("/login", true);
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
        try
        {
            var refreshSuccess = await RefreshTokenAsync();
            if (!refreshSuccess)
            {
                await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
                _navigationManager.NavigateTo("/login", true);
            }
        }
        catch (Exception)
        {
            // Si ocurre cualquier error, asegurar el logout
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

    public async Task<List<T>> GetNestedListAsync<T>(string requestUri) where T : class
    {
        try
        {
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

            if (!httpResponse.IsSuccessStatusCode)
            {
                _toastService.ShowError($"Error en la solicitud: {httpResponse.StatusCode}");
                return new List<T>();
            }

            // Leer el contenido JSON sin procesar
            string rawJson = await httpResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Raw JSON: {rawJson}");

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                // Deserializar a BaseResponseModel
                var baseResponse = System.Text.Json.JsonSerializer.Deserialize<BaseResponseModel>(rawJson, options);

                if (baseResponse != null && baseResponse.Success && baseResponse.Data != null)
                {
                    // Serializar el objeto data
                    string dataJson = System.Text.Json.JsonSerializer.Serialize(baseResponse.Data);

                    // Verificar si tiene la propiedad $values
                    var jsonDocument = System.Text.Json.JsonDocument.Parse(dataJson);

                    if (jsonDocument.RootElement.TryGetProperty("$values", out var valuesElement))
                    {
                        // Deserializar desde $values
                        string valuesJson = valuesElement.GetRawText();
                        return System.Text.Json.JsonSerializer.Deserialize<List<T>>(valuesJson, options) ?? new List<T>();
                    }
                    else
                    {
                        // Intentar deserializar directamente
                        return System.Text.Json.JsonSerializer.Deserialize<List<T>>(dataJson, options) ?? new List<T>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al deserializar: {ex.Message}");
            }

            return new List<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general: {ex.Message}");
            _toastService.ShowError($"Error: {ex.Message}");
            return new List<T>();
        }
    }

    public async Task<List<T>> GetTransactionsListAsync<T>(string requestUri) where T : class
    {
        try
        {
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

            string rawJson = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _toastService.ShowError($"Error en la solicitud: {httpResponse.StatusCode}");
                return new List<T>();
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                // Primero intentemos deserializar como BaseResponseModel
                var baseResponse = System.Text.Json.JsonSerializer.Deserialize<BaseResponseModel>(rawJson, options);
                if (baseResponse != null && baseResponse.Success && baseResponse.Data != null)
                {
                    try
                    {
                        // Intento #1: Data como string JSON a deserializar
                        var dataJson = System.Text.Json.JsonSerializer.Serialize(baseResponse.Data);
                        return System.Text.Json.JsonSerializer.Deserialize<List<T>>(dataJson, options) ?? new List<T>();
                    }
                    catch
                    {
                        try
                        {
                            // Intento #2: Data podría tener formato $values
                            var jsonDocument = System.Text.Json.JsonDocument.Parse(
                                System.Text.Json.JsonSerializer.Serialize(baseResponse.Data));

                            if (jsonDocument.RootElement.TryGetProperty("$values", out var valuesElement))
                            {
                                string valuesJson = valuesElement.GetRawText();
                                return System.Text.Json.JsonSerializer.Deserialize<List<T>>(valuesJson, options) ?? new List<T>();
                            }
                            else
                            {
                                // Intento #3: Data podría ser un arreglo directo
                                return System.Text.Json.JsonSerializer.Deserialize<List<T>>(
                                    jsonDocument.RootElement.GetRawText(), options) ?? new List<T>();
                            }
                        }
                        catch
                        {
                            // Si todos los intentos fallan, retornar lista vacía
                            return new List<T>();
                        }
                    }
                }

                // Si no podemos deserializar como BaseResponseModel, intenta directamente
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<T>>(rawJson, options) ?? new List<T>();
                }
                catch
                {
                    return new List<T>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializando transacciones: {ex.Message}");
                return new List<T>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general en GetTransactionsListAsync: {ex.Message}");
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

