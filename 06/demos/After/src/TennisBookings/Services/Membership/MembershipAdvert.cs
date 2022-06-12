namespace TennisBookings.Services.Membership
{
	public class MembershipAdvert : IMembershipAdvert
	{
		public delegate MembershipAdvert Factory(decimal offerPrice, decimal discount);

		public MembershipAdvert(decimal offerPrice, decimal discount)
		{
			OfferPrice = offerPrice;
			Saving = discount;
		}

		public decimal OfferPrice { get; }

		public decimal Saving { get; }
	}
}
