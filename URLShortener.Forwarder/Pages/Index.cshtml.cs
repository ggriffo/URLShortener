using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using URLShortner.Model.Data;

namespace URLShortener.Forwarder.Pages;

public class IndexModel : PageModel
{
    //private readonly ILogger<IndexModel> _logger;
    private readonly DataBaseContext _context;
    private IConfiguration _configuration;

    public IndexModel(DataBaseContext context, IConfiguration iConfig)
    {
        _context = context;
        _configuration = iConfig;
    }

    public void OnGet()
    {

    }

    [BindProperty]
    public Model URLModel { get; set; } = default!;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        URLShortner.URLController uRLController = new URLShortner.URLController(_context, _configuration);
        var returnurl = await uRLController.Create(URLModel.FullURL);

        URLModel.HashURL = returnurl.Value?.HashURL;

        return Page();
    }    
}

public class Model
{
    public string HashURL { get; set; }
    public string FullURL { get; set; }
}
