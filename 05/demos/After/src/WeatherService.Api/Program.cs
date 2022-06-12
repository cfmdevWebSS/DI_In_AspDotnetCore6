using TennisBookings.Shared.Weather;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IWeatherForecaster, RandomWeatherForecaster>();

var app = builder.Build();

app.MapGet("/weather/{city}", async (string city, IWeatherForecaster forecaster) =>
	{
		var forecast = await forecaster.GetCurrentWeatherAsync(city);
		return forecast.Weather;
	});

app.Run();
