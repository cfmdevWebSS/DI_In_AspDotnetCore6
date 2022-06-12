using Microsoft.Extensions.Options;

namespace TennisBookings.Services.Bookings.Rules
{
	public class MaxBookingLengthRule : ICourtBookingRule
	{
		private readonly IBookingConfiguration _bookingConfiguration;

		public MaxBookingLengthRule(IBookingConfiguration config)
		{
			_bookingConfiguration = config;
		}

		public Task<bool> CompliesWithRuleAsync(CourtBooking booking)
		{
			var bookingLength = booking.EndDateTime - booking.StartDateTime;

			var compliesWithRule = bookingLength <= TimeSpan.FromHours(_bookingConfiguration.MaxRegularBookingLengthInHours);

			return Task.FromResult(compliesWithRule);
		}

		public string ErrorMessage => "Booking is longer than allowed booking length";
	}
}
