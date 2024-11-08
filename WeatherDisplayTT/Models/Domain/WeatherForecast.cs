namespace WeatherDisplayTT.Models.Domain;

using System;
using System.Collections.Generic;

public class WeatherForecast
{
    public HeadlineInfo Headline { get; set; } = new();
    public List<DailyForecast> DailyForecasts { get; set; } = [];

    public class HeadlineInfo
    {
        public DateTime EffectiveDate { get; set; } = new();
        public int EffectiveEpochDate { get; set; }
        public string Text { get; set; } = string.Empty;
        public string MobileLink { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }

    public class DailyForecast
    {
        public DateTime Date { get; set; } = new();
        public int EpochDate { get; set; }
        public TemperatureInfo Temperature { get; set; } = new();
        public DayNightInfo Day { get; set; } = new();
        public DayNightInfo Night { get; set; } = new();

        public bool IsRainExpected => Day.HasPrecipitation || Night.HasPrecipitation;
    }

    public class TemperatureInfo
    {
        public Measurement Minimum { get; set; } = new();
        public Measurement Maximum { get; set; } = new();
    }

    public class Measurement
    {
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int UnitType { get; set; }
    }

    public class DayNightInfo
    {
        public int Icon { get; set; }
        public string IconPhrase { get; set; } = string.Empty;
        public bool HasPrecipitation { get; set; }
    }
}
