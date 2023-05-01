
namespace Investigations.Business.Models
{
	public record AppConfig
	{
		public string? Title { get; init; }
        public string? OpenAPIKey { get; init; }
    }
}