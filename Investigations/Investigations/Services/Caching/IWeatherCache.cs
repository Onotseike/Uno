using Investigations.DataContracts;

using System.Collections.Immutable;

namespace Investigations.Services.Caching
{
	public interface IWeatherCache
	{
		ValueTask<IImmutableList<WeatherForecast>> GetForecast(CancellationToken token);
	}
}