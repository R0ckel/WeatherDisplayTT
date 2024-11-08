namespace WeatherDisplayTT.Models.Domain;

public class CitySearchValue
{
    public string Key { get; set; } = string.Empty;
    public string LocalizedName { get; set; } = string.Empty;
    public CountryInfo Country { get; set; } = new();
    public AdministrativeAreaInfo AdministrativeArea { get; set; } = new();
    public RegionInfo Region { get; set; } = new();

    public override string ToString()
    {
        return $"{LocalizedName} - {Region.LocalizedName}, {Country.LocalizedName} => {AdministrativeArea.LocalizedName} {AdministrativeArea.LocalizedType}";
    }

    public class CountryInfo
    {
        public string LocalizedName { get; set; } = string.Empty;
    }

    public class RegionInfo
    {
        public string LocalizedName { get; set; } = string.Empty;
    }

    public class AdministrativeAreaInfo
    {
        public string LocalizedName { get; set; } = string.Empty;
        public string LocalizedType { get; set; } = string.Empty;
    }
}
