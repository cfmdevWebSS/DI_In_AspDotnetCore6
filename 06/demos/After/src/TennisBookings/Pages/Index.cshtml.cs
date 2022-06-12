using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TennisBookings.Services.Membership;

namespace TennisBookings.Pages
{
	public class IndexModel : PageModel
	{
		private readonly IWeatherForecaster _weatherForecaster;
		private readonly ILogger<IndexModel> _logger;
		private readonly IHomePageGreetingService _greetingService;
		private readonly FeaturesConfiguration _config;

		public IndexModel(
			IWeatherForecaster weatherForecaster,
			ILogger<IndexModel> logger,
			IOptionsSnapshot<FeaturesConfiguration> options,
			IMembershipAdvert advert,
			IHomePageGreetingService greetingService)
		{
			_weatherForecaster = weatherForecaster;
			_logger = logger;
			Advert = advert;
			_greetingService = greetingService;
			_config = options.Value;
		}

		public string WeatherDescription { get; private set; } =
			"We don't have the latest weather information right now, " +
			"please check again later.";

		public bool ShowWeatherForecast { get; private set; }
		public bool ShowGreeting => true;
		public string Greeting => _greetingService.GetRandomHomePageGreeting();

		public IMembershipAdvert Advert { get; }

		public async Task OnGet()
		{
			ShowWeatherForecast = _config.EnableWeatherForecast;

			if (ShowWeatherForecast)
			{
				try
				{
					var currentWeather = await _weatherForecaster
						.GetCurrentWeatherAsync("Eastbourne");

					switch (currentWeather.Weather.Summary)
					{
						case "Sun":
							WeatherDescription = "It's sunny right now. " +
								"A great day for tennis!";
							break;

						case "Cloud":
							WeatherDescription = "It's cloudy at the moment " +
								"and the outdoor courts are in use.";
							break;

						case "Rain":
							WeatherDescription = "We're sorry but it's raining here. " +
								"No outdoor courts in use.";
							break;

						case "Snow":
							WeatherDescription = "It's snowing!! Outdoor courts will " +
								"remain closed until the snow has cleared.";
							break;
					}
				}
				catch
				{
					_logger.LogError("Oh no!!");
				}
			}
		}
	}
}
