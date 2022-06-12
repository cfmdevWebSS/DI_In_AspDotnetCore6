using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceLifetimeDemonstration.Pages
{
	public class TrimmedModel : PageModel
    {
		private readonly IGuidTrimmer _guidTrimmer;

		public TrimmedModel(IGuidTrimmer guidTrimmer)
		{
			_guidTrimmer = guidTrimmer;
		}

		public string TrimmedGuid => _guidTrimmer.TrimmedGuid();

		public void OnGet()
        {
        }
    }
}
