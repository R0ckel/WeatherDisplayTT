namespace WeatherDisplayTT.Models.Infrastructure;

public class Result
{
    public int StatusCode { get; set; } = StatusCodes.Status200OK;
    public bool Success { get; set; } = true;
    public List<string> Errors { get; set; } = [];
}
