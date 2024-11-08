using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System.Text;

namespace WeatherDisplayTT.Services.Clients.Http;

public class ApiClient : IApiClient
{
    protected readonly JsonSerializerSettings _jsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new StringEnumConverter() }
    };

    protected readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)));

    protected HttpClient _client;

    public ApiClient(HttpClient httpClient)
    {
        _client = httpClient;
    }

    public Task<T?> GetAsync<T>(string url, object? param = null) => SendAsync<T>(HttpMethod.Get, url, param);

    public Task<T?> PostAsync<T>(string url, object? param = null) => SendAsync<T>(HttpMethod.Post, url, param);

    public Task<T?> PutAsync<T>(string url, object? param = null) => SendAsync<T>(HttpMethod.Put, url, param);

    public Task<T?> PatchAsync<T>(string url, object? param = null) => SendAsync<T>(HttpMethod.Patch, url, param);

    public Task<T?> DeleteAsync<T>(string url, object? param = null) => SendAsync<T>(HttpMethod.Delete, url, param);

    protected static void AddQueryParameter(List<string> query, string key, JToken value)
    {
        if (value.Type == JTokenType.Object)
        {
            foreach (var property in value.Children<JProperty>())
            {
                AddQueryParameter(query, $"{key}.{property.Name}", property.Value);
            }
        }
        else if (value.Type == JTokenType.Array)
        {
            for (int i = 0; i < value.Children().Count(); i++)
            {
                AddQueryParameter(query, $"{key}[{i}]", value.Children().ElementAt(i));
            }
        }
        else if (value.Type == JTokenType.Date)
        {
            query.Add($"{key}={(DateTime)value:o}");
        }
        else
        {
            query.Add($"{key}={value}");
        }
    }

    protected async Task<T?> SendAsync<T>(HttpMethod method, string url, object? param = null)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            HttpRequestMessage request;
            if (method == HttpMethod.Get || method == HttpMethod.Delete)
            {
                var queryString = ToQueryString(param);
                request = new HttpRequestMessage(method, $"{url}?{queryString}");
            }
            else
            {
                var stringContent = new StringContent(
                    JsonConvert.SerializeObject(param, _jsonSettings),
                    Encoding.UTF8,
                    "application/json");
                request = new HttpRequestMessage(method, url) { Content = stringContent };
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            try
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content, _jsonSettings);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Json parsing error: {ex.Message}");
                return default;
            }
        });
    }

    protected string ToQueryString(object? obj)
    {
        if (obj == null) return string.Empty;

        var json = JsonConvert.SerializeObject(obj, _jsonSettings);
        var jObj = JObject.Parse(json);
        var query = new List<string>();
        foreach (var property in jObj.Properties())
        {
            AddQueryParameter(query, property.Name, property.Value);
        }
        return string.Join("&", query);
    }
}
