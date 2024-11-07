using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WeatherDisplayTT.Services.Clients.Http;

public class ApiClient : IApiClient
{
    protected readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
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
                    System.Text.Json.JsonSerializer.Serialize(param, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");
                request = new HttpRequestMessage(method, url) { Content = stringContent };
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            return JsonConvert.DeserializeObject<T>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        });
    }

    protected string ToQueryString(object? obj)
    {
        if (obj == null) return string.Empty;

        var json = System.Text.Json.JsonSerializer.Serialize(obj, _jsonOptions);
        var jObj = JObject.Parse(json);
        var query = new List<string>();
        foreach (var property in jObj.Properties())
        {
            AddQueryParameter(query, property.Name, property.Value);
        }
        return string.Join("&", query);
    }
}
