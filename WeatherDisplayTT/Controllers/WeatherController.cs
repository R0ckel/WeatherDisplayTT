using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WeatherDisplayTT.Models.Domain;
using WeatherDisplayTT.Services.WeatherService;
using WeatherDisplayTT.ViewModels;
using WeatherDisplayTT.ViewModels.Weather;

namespace WeatherDisplayTT.Controllers;

public class WeatherController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWeatherService _weatherService;

    public WeatherController(IHttpContextAccessor httpContextAccessor,
                             IWeatherService weatherService)
    {
        _httpContextAccessor = httpContextAccessor;
        _weatherService = weatherService;
    }

    public async Task<IActionResult> Index()
    {
        var city = GetLastCityFromCookies();
        var weatherViewModel = new WeatherViewModel
        {
            LastCity = city
        };

        if (city != null)
        {
            var weatherResult = await _weatherService.FetchWeatherAsync(city.Key);

            if (weatherResult.Success)
            {
                weatherViewModel.Weather = weatherResult.Values.FirstOrDefault();
            }
            else
            {
                weatherViewModel.Errors = weatherResult.Errors;
            }
        }

        return View(weatherViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SearchCity(string cityName)
    {
        var result = await _weatherService.FetchCitiesByName(cityName);
        var weatherViewModel = new WeatherViewModel();

        if (result.Success)
        {
            weatherViewModel.Cities = result.Values;
        }
        else
        {
            weatherViewModel.Errors = result.Errors;
        }

        return View("Index", weatherViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ShowWeather(string locationKey)
    {
        var cityResult = await _weatherService.FetchCityInfoByLocationKey(locationKey);
        var weatherViewModel = new WeatherViewModel();

        if (cityResult.Success)
        {
            var city = cityResult.Values.FirstOrDefault();
            weatherViewModel.LastCity = city;

            var weatherResult = await _weatherService.FetchWeatherAsync(locationKey);

            if (weatherResult.Success)
            {
                var weather = weatherResult.Values.FirstOrDefault();
                weatherViewModel.Weather = weather;

                if (weather?.DailyForecasts.FirstOrDefault()?.IsRainExpected == true)
                {
                    var cityAlerted = HasCityAlertedToday(locationKey);

                    if (!cityAlerted)
                    {
                        SetAlertedCityForToday(locationKey);
                        weatherViewModel.AlertMessage = $"Rain is expected in {city?.LocalizedName}. Take an umbrella!";
                    }
                }

                if (weather != null)
                {
                    SaveLastCityInCookies(city);
                }
            }
            else
            {
                weatherViewModel.Errors = weatherResult.Errors;
            }
        }
        else
        {
            weatherViewModel.Errors = cityResult.Errors;
        }

        return View("Index", weatherViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private void SaveLastCityInCookies(CitySearchValue? city)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(7),
            HttpOnly = true
        };

        Response.Cookies.Append("LastCity", JsonConvert.SerializeObject(city), cookieOptions);
    }

    private CitySearchValue? GetLastCityFromCookies()
    {
        if (_httpContextAccessor.HttpContext == null) return null;
        var lastCityJson = _httpContextAccessor.HttpContext.Request.Cookies["LastCity"];

        return lastCityJson != null ? JsonConvert.DeserializeObject<CitySearchValue>(lastCityJson) : null;
    }

    private void SetAlertedCityForToday(string locationKey)
    {
        if (_httpContextAccessor.HttpContext == null) return;
        var alerts = _httpContextAccessor.HttpContext.Request.Cookies["AlertedCities"];
        var alertedCities = alerts != null ? alerts.Split(',').ToList() : [];

        alertedCities.Add(locationKey);

        var options = new CookieOptions { Expires = DateTime.UtcNow.Date.AddDays(1), HttpOnly = true };
        Response.Cookies.Append("AlertedCities", string.Join(",", alertedCities), options);
    }

    private bool HasCityAlertedToday(string locationKey)
    {
        if (_httpContextAccessor.HttpContext == null) return false;
        var alerts = _httpContextAccessor.HttpContext.Request.Cookies["AlertedCities"];
        return alerts?.Split(',').Contains(locationKey) ?? false;
    }
}
