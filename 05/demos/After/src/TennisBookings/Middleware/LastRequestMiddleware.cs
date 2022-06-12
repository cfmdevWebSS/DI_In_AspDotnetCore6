namespace TennisBookings.Middleware
{
	public class LastRequestMiddleware : IMiddleware
	{
		private readonly IUtcTimeService _utcTimeService;
		private readonly UserManager<TennisBookingsUser> _userManager;

		public LastRequestMiddleware(
			IUtcTimeService utcTimeService,
			UserManager<TennisBookingsUser> userManager)
		{
			_utcTimeService = utcTimeService;
			_userManager = userManager;
		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			if (context.User.Identity is not null &&
				context.User.Identity.IsAuthenticated)
			{
				var user = await _userManager
					.FindByNameAsync(context.User.Identity.Name);

				if (user is not null)
				{
					user.LastRequestUtc = _utcTimeService.CurrentUtcDateTime;
					await _userManager.UpdateAsync(user);
				}
			}

			await next(context);
		}
	}
}
