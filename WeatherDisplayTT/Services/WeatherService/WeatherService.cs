using WeatherDisplayTT.DTOs.Weather;
using WeatherDisplayTT.Models.Domain;
using WeatherDisplayTT.Models.Infrastructure;
using WeatherDisplayTT.Services.Clients.Http;

namespace WeatherDisplayTT.Services.WeatherService;

public class WeatherService : IWeatherService
{
    private readonly IApiClient _apiClient;
    private readonly string _apiKey;

    public WeatherService(Dictionary<string, IApiClient> namedClients, IConfiguration configuration)
    {
        _apiClient = namedClients["AccuWeatherApiClient"];
        _apiKey = configuration["AccuWeather:ApiKey"] ?? "";
    }

    public async Task<ValueResult<CitySearchValue>> FetchCitiesByName(string cityQuery)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<CitySearchValue>>(
                "locations/v1/cities/search",
                new GetCitiesByQueryRequest()
                {
                    apikey = _apiKey,
                    q = cityQuery
                });
            if (response == null)
            {
                return new ValueResult<CitySearchValue>()
                {
                    Success = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = [$"Fetching cities resulted in empty weather server response"]
                };
            }
            if (response.Count == 0)
            {
                return new ValueResult<CitySearchValue>()
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = [$"No cities by this name found"]
                };
            }
            return new ValueResult<CitySearchValue>()
            {
                Values = [.. response]
            };
        }
        catch (Exception ex)
        {
            return new ValueResult<CitySearchValue>()
            {
                Success = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Errors = [$"Exception while fetching cities by name: {ex.Message}"]
            };
        }
    }

    public async Task<ValueResult<CitySearchValue>> FetchCityInfoByLocationKey(string locationKey)
    {
        try
        {
            var response = await _apiClient.GetAsync<CitySearchValue>(
                $"locations/v1/{locationKey}",
                new GetCitiesByQueryRequest()
                {
                    apikey = _apiKey,
                });
            if (response == null)
            {
                return new ValueResult<CitySearchValue>()
                {
                    Success = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = [$"Fetching cities resulted in empty weather server response"]
                };
            }
            return new ValueResult<CitySearchValue>()
            {
                Values = [response]
            };
        }
        catch (Exception ex)
        {
            return new ValueResult<CitySearchValue>()
            {
                Success = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Errors = [$"Exception while fetching cities by name: {ex.Message}"]
            };
        }
    }

    public async Task<ValueResult<WeatherForecast>> FetchWeatherAsync(string locationKey)
    {
        try
        {
            var response = await _apiClient.GetAsync<WeatherForecast>(
                $"forecasts/v1/daily/1day/{locationKey}",
                new BaseAccuWeatherRequest()
                {
                    apikey = _apiKey
                });
            if (response == null || response.DailyForecasts == null)
            {
                return new ValueResult<WeatherForecast>()
                {
                    Success = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = [$"Fetching cities resulted in empty weather server response"]
                };
            }
            return new ValueResult<WeatherForecast>()
            {
                Values = [response]
            };
        }
        catch (Exception ex)
        {
            return new ValueResult<WeatherForecast>()
            {
                Success = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Errors = [$"Exception while fetching cities by name: {ex.Message}"]
            };
        }
    }
}
