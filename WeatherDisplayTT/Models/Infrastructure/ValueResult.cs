namespace WeatherDisplayTT.Models.Infrastructure;

public class ValueResult<T> : Result
{
    public List<T> Values { get; set; } = [];
}
