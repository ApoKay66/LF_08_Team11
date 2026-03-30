using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainBusters_Grp11_LF08.Pages;

public class IndexModel : PageModel
{
    public string WelcomeMessage { get; set; } = string.Empty;
    public void OnGet()
    {
        WelcomeMessage = "Y'all wanna bust sum brainzz?";
    }
}
