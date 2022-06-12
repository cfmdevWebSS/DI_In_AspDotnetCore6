namespace TennisBookings.Services.Bookings.Rules
{
	public class MustBeInFutureBookingRule : ICourtBookingRule
	{
		private readonly IUtcTimeService _utcTimeService;

		public MustBeInFutureBookingRule(IUtcTimeService utcTimeService)
		{
			_utcTimeService = utcTimeService;
		}

		public string ErrorMessage => "Booking must be in the future!";

		public Task<bool> CompliesWithRuleAsync(CourtBooking booking)
		{
			return Task.FromResult(booking.StartDateTime > _utcTimeService
				.CurrentUtcDateTime);
		}
	}
}
