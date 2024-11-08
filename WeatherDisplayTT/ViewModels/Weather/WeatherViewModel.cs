using WeatherDisplayTT.Models.Domain;

namespace WeatherDisplayTT.ViewModels.Weather;

public class WeatherViewModel
{
    public List<CitySearchValue>? Cities { get; set; }
    public WeatherForecast? Weather { get; set; }
    public List<string>? Errors { get; set; }
    public string? AlertMessage { get; set; }
    public CitySearchValue? LastCity { get; set; }
}
