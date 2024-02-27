using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortner.Model;
using URLShortner.Model.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace URLShortner;

[ApiController]
[Route("")]
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

    [HttpGet]
    [Route("{hashUrl}")]
    public async Task<ActionResult<URLModel>> GetURL([FromRoute]string hashUrl)
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

        url.HashURL = configuration["Application:BaseUrl"] + hashUrl;

        return url;
    }

    [HttpPost]
    public async Task<ActionResult<URLModel>> Create(string urlAddress)
    {
        if (_context.Items.Where(c => c.FullURL == urlAddress).FirstOrDefault() == null)
        {
            Regex validateUrl = new Regex(UrlRegex);

            if (!validateUrl.IsMatch(urlAddress))
            {
                return BadRequest();
            }

            string token =  GenerateToken();

            URLModel urlModel = new()
            {
                FullURL = urlAddress,
                HashURL =  token,
                Clicked = 0,
                CreatedDate = DateTime.Now
            };

            _context.Add<URLModel>(urlModel);
            await _context.SaveChangesAsync();

            return await GetURL(token);
        }
        else
        {
            var first = await _context.Items.FirstAsync<URLModel>(x => x.FullURL == urlAddress);
            return await GetURL(first.HashURL);
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
        return urlsafe.Substring(new  Random().Next(0, urlsafe.Length), new  Random().Next(2, 8));
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
