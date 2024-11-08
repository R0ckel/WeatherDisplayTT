namespace WeatherDisplayTT.DTOs.Weather;

public class GetCitiesByQueryRequest : BaseAccuWeatherRequest
{
    public string q { get; set; } = string.Empty;
}
