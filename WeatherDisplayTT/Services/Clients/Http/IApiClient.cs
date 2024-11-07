namespace WeatherDisplayTT.Services.Clients.Http;

public interface IApiClient
{
    Task<T?> DeleteAsync<T>(string url, object? param = null);

    Task<T?> GetAsync<T>(string url, object? param = null);

    Task<T?> PostAsync<T>(string url, object? param = null);

    Task<T?> PutAsync<T>(string url, object? param = null);

    Task<T?> PatchAsync<T>(string url, object? param = null);
}
