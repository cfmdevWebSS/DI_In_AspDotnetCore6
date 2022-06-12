using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TennisBookings.Pages
{
	public class BookingsModel : PageModel
    {
        private readonly UserManager<TennisBookingsUser> _userManager;
        private readonly ICourtBookingService _courtBookingService;
		private readonly ILoggedInUserGreetingService _greetingService;
		private readonly IDistributedCache<UserGreeting> _cache;

		public BookingsModel(
			UserManager<TennisBookingsUser> userManager,
			ICourtBookingService courtBookingService,
			ILoggedInUserGreetingService greetingService,
			IDistributedCache<UserGreeting> cache)
        {
            _userManager = userManager;
            _courtBookingService = courtBookingService;
			_greetingService = greetingService;
			_cache = cache;
		}

        public IEnumerable<IGrouping<DateTime, CourtBooking>> CourtBookings { get; set; } = Array.Empty<IGrouping<DateTime, CourtBooking>>();

        public string Greeting { get; private set; } = "Hello";

        [TempData]
        public bool BookingSuccess { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (user == null)
                return new ChallengeResult();

            if (user.Member is not null)
			{
				var cacheKey = $"user_greeting_{user.Id}";

				var (isCached, greeting) = await _cache.TryGetValueAsync(cacheKey);

				if (!isCached)
				{
					greeting = _greetingService.GetLoggedInGreeting(user.Member.Forename);
					await _cache.SetAsync(cacheKey, greeting, 60);
				}

				Greeting = greeting?.Greeting ?? "Hi friend!";

				var bookings = await _courtBookingService.GetFutureBookingsForMemberAsync(user.Member);
				CourtBookings = bookings.GroupBy(x => x.StartDateTime.Date);
			}

            return Page();
        }
    }
}
