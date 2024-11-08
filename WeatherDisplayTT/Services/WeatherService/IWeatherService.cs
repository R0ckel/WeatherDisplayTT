using WeatherDisplayTT.Models.Domain;
using WeatherDisplayTT.Models.Infrastructure;

namespace WeatherDisplayTT.Services.WeatherService;

public interface IWeatherService
{
    Task<ValueResult<WeatherForecast>> FetchWeatherAsync(string locationKey);

    Task<ValueResult<CitySearchValue>> FetchCitiesByName(string cityName);

    Task<ValueResult<CitySearchValue>> FetchCityInfoByLocationKey(string locationKey);
}
