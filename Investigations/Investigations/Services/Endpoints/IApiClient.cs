using Investigations.DataContracts;

using System.Collections.Immutable;

namespace Investigations.Services.Endpoints
{
	[Headers("Content-Type: application/json")]
	public interface IApiClient
	{
		[Get("/api/weatherforecast")]
		Task<ApiResponse<IImmutableList<WeatherForecast>>> GetWeather(CancellationToken cancellationToken = default);
	}
}