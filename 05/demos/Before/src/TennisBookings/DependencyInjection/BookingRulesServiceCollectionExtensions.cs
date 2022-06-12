using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TennisBookings.DependencyInjection
{
	public static class BookingRulesServiceCollectionExtensions
	{
		public static IServiceCollection AddBookingRules(this IServiceCollection services)
		{
			services.AddSingleton<ICourtBookingRule, ClubIsOpenRule>();
			services.AddSingleton<ICourtBookingRule, MaxBookingLengthRule>();
			services.AddSingleton<ICourtBookingRule, MaxPeakTimeBookingLengthRule>();
			services.AddScoped<ICourtBookingRule, MemberBookingsMustNotOverlapRule>();
			services.AddScoped<ICourtBookingRule, MemberCourtBookingsMaxHoursPerDayRule>();

			// Example of adding a new rule to the application
			services.AddSingleton<ICourtBookingRule, MustBeInFutureBookingRule>();

			services.TryAddScoped<IBookingRuleProcessor, BookingRuleProcessor>();

			return services;
		}
	}
}
