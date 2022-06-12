#region Global Usings
global using Microsoft.AspNetCore.Identity;

global using TennisBookings;
global using TennisBookings.Data;
global using TennisBookings.Domain;
global using TennisBookings.Extensions;
global using TennisBookings.Configuration;
global using TennisBookings.Caching;
global using TennisBookings.Shared.Weather;
global using TennisBookings.Services.Bookings;
global using TennisBookings.Services.Greetings;
global using TennisBookings.Services.Unavailability;
global using TennisBookings.Services.Bookings.Rules;
global using TennisBookings.Services.Notifications;
global using TennisBookings.Services.Time;
global using TennisBookings.Services.Staff;
global using TennisBookings.Services.Courts;
global using TennisBookings.Services.Security;
global using Microsoft.EntityFrameworkCore;
#endregion

using Microsoft.Data.Sqlite;
using TennisBookings.BackgroundService;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TennisBookings.Services.Membership;
using TennisBookings.DependencyInjection;
using TennisBookings.Middleware;
using TennisBookings.Services;
using Autofac.Extensions.DependencyInjection;
using Autofac;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

var services = builder.Services;

//services.TryAddSingleton<IWeatherForecaster, AmazingWeatherForecaster>();
//services.TryAddSingleton<IWeatherForecaster, RandomWeatherForecaster>();
//services.Decorate<IWeatherForecaster, CachedWeatherForecaster>();

builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
	builder.RegisterType<RandomWeatherForecaster>()
		.As<IWeatherForecaster>()
		.SingleInstance();
	builder.RegisterDecorator<CachedWeatherForecaster, IWeatherForecaster>();
});

#region Service Descriptor Examples

//var servicesDescriptor1 = new ServiceDescriptor(typeof(IWeatherForecaster),
//	typeof(AmazingWeatherForecaster), ServiceLifetime.Singleton);

//var servicesDescriptor2 = ServiceDescriptor.Describe(typeof(IWeatherForecaster),
//	typeof(AmazingWeatherForecaster), ServiceLifetime.Singleton);

//var servicesDescriptor3 = ServiceDescriptor.Singleton(typeof(IWeatherForecaster),
//	typeof(AmazingWeatherForecaster));

//var servicesDescriptor4 = ServiceDescriptor.Singleton<IWeatherForecaster,
//	AmazingWeatherForecaster>();

//services.Add(servicesDescriptor1);

#endregion

services.TryAddScoped<ICourtBookingService, CourtBookingService>();
services.TryAddSingleton<IUtcTimeService, TimeService>();

services.TryAddScoped<IBookingService, BookingService>();
services.TryAddScoped<ICourtService, CourtService>();

services.TryAddScoped<ICourtBookingManager, CourtBookingManager>();
services.Configure<BookingConfiguration>(builder.Configuration.GetSection("CourtBookings"));
services.TryAddSingleton<INotificationService, EmailNotificationService>();

services.AddBookingRules();

//services.AddScoped<IUnavailabilityProvider, ClubClosedUnavailabilityProvider>();
//services.AddScoped<IUnavailabilityProvider, ClubClosedUnavailabilityProvider>();
//services.AddScoped<IUnavailabilityProvider, UpcomingHoursUnavailabilityProvider>();
//services.AddScoped<IUnavailabilityProvider, OutsideCourtUnavailabilityProvider>();
//services.AddScoped<IUnavailabilityProvider, CourtBookingUnavailabilityProvider>();

services.TryAddEnumerable(new ServiceDescriptor[]
{
	ServiceDescriptor.Scoped<IUnavailabilityProvider, ClubClosedUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, ClubClosedUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, UpcomingHoursUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, OutsideCourtUnavailabilityProvider>(),
	ServiceDescriptor.Scoped<IUnavailabilityProvider, CourtBookingUnavailabilityProvider>(),
});

services.AddTransient<IMembershipAdvertBuilder, MembershipAdvertBuilder>();
services.AddSingleton<IMembershipAdvert>(sp =>
{
	var builder = sp.GetRequiredService<IMembershipAdvertBuilder>();
	builder.WithDiscount(10m);
	var advert = builder.Build();
	return advert;
});

services.TryAddSingleton<GreetingService>();
services.TryAddSingleton<IHomePageGreetingService>(sp =>
	sp.GetRequiredService<GreetingService>());
services.TryAddSingleton<ILoggedInUserGreetingService>(sp =>
	sp.GetRequiredService<GreetingService>());

services.AddScoped<ICourtMaintenanceService, CourtMaintenanceService>();

services.TryAddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));

services.Configure<ClubConfiguration>(builder.Configuration.GetSection("ClubSettings"));
services.Configure<BookingConfiguration>(builder.Configuration.GetSection("CourtBookings"));
services.Configure<FeaturesConfiguration>(builder.Configuration.GetSection("Features"));
services.Configure<MembershipConfiguration>(builder.Configuration.GetSection("Membership"));

services.TryAddSingleton<IBookingConfiguration>(sp =>
	sp.GetRequiredService<IOptions<BookingConfiguration>>().Value);

services.TryAddScoped<LastRequestMiddleware>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Bookings");
    options.Conventions.AuthorizePage("/BookCourt");
    options.Conventions.AuthorizePage("/FindAvailableCourts");
    options.Conventions.Add(new PageRouteTransformerConvention(new SlugifyParameterTransformer()));
});

#region InternalSetup
using var connection = new SqliteConnection("Filename=:memory:");
//using var connection = new SqliteConnection("Filename=test.db");
connection.Open();

// Add services to the container.
builder.Services.AddDbContext<TennisBookingsDbContext>(options => options.UseSqlite(connection));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<TennisBookingsUser, TennisBookingsRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<TennisBookingsDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddHostedService<InitialiseDatabaseService>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.AccessDeniedPath = "/identity/account/access-denied";
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LastRequestMiddleware>();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
