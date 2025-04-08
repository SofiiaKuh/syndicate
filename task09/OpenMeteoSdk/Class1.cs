using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenMeteoSdk;

public class Class1
{
	private static readonly HttpClient _httpClient = new();

	public async Task<string> GetWeatherForecastAsync(double latitude, double longitude)
	{
		var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m&current_weather=true";
		var response = await _httpClient.GetAsync(url);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}
}
