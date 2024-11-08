using WeatherDisplayTT.Services.Clients.Http;
using WeatherDisplayTT.Services.WeatherService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("AccuWeatherHttpClient", client =>
{
    client.BaseAddress = new Uri("https://dataservice.accuweather.com/");
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddScoped(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    return new Dictionary<string, IApiClient>
    {
        {
            "AccuWeatherApiClient", new ApiClient(httpClientFactory.CreateClient("AccuWeatherHttpClient"))
        }
    };
});
// Note: we can also move names like this to separate static class to avoid using magis strings...
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
