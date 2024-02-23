using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortner.Model;
using URLShortner.Model.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace URLShortner;

[ApiController]
[Route("[controller]")]
public class URLController : Controller
{
    private readonly DataBaseContext _context;
    private IConfiguration configuration;
    private readonly string UrlRegex = "^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$";

    public URLController(DataBaseContext context, IConfiguration iConfig)
    {
        _context = context;
        configuration = iConfig;
    }

    [HttpGet("hashUrl")]
    public async Task<ActionResult<URLModel>> GetURL(string hashUrl)
    {
        //TODO: Implement REDIS cache
        var url = await this._context.FindAsync<URLModel>(hashUrl);

        if (url == null)
        {
            return NotFound();
        }

        url.Clicked ++;
        _context.Update(url);
        await _context.SaveChangesAsync();

        url.HashURL = configuration.GetSection("Application").GetSection("BaseUrl").Value + url.HashURL;

        return url;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string urlAddress)
    {
        if (_context.Items.Where(c => c.FullURL == urlAddress).FirstOrDefault() == null)
        {
            Regex validataUrl = new Regex(UrlRegex);

            if (!validataUrl.IsMatch(urlAddress))
            {
                return BadRequest();
            }

            URLModel urlModel = new()
            {
                FullURL = urlAddress,
                HashURL =  GenerateToken(),
                Clicked = 0,
                CreatedDate = DateTime.Now
            };


            _context.Add<URLModel>(urlModel);
            await _context.SaveChangesAsync();

            return new OkObjectResult(urlModel);
        }
        else
        {
            return new OkObjectResult(await _context.Items.FirstAsync<URLModel>(x => x.FullURL == urlAddress));
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        _context.Items.RemoveRange(_context.Items.Where(x => x.CreatedDate < DateTime.Now.AddDays(-30)));

        await _context.SaveChangesAsync();

        return Ok();        
    }

    private string GenerateToken() {
        string  urlsafe = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
        return urlsafe.Substring(new  Random().Next(0, urlsafe.Length), new  Random().Next(2, 6));
        // Enumerable.Range(48, 75)
        //     .Where(i => i < 58 || i > 64 && i < 91 || i > 96)
        //     .OrderBy(o => new Random().Next())
        //     .ToList()
        //     .ForEach(i => urlsafe += Convert.ToChar(i)); // Store each char into urlsafe
        // return urlsafe.Substring(new Random().Next(0, urlsafe.Length), new Random().Next(2, 6));
    }
    private string ByteArrayToString(byte[] arrInput)
    {
        int i;
        StringBuilder sOutput = new StringBuilder(arrInput.Length);
        for (i=0;i < arrInput.Length; i++)
        {
            sOutput.Append(arrInput[i].ToString("X2"));
        }
        return sOutput.ToString();
    }
}
