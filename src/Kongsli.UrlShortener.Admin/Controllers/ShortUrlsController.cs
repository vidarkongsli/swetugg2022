using Kongsli.UrlShortener.Admin.Model;
using Kongsli.UrlShortener.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kongsli.UrlShortener.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShortUrlsController : ControllerBase
    {
        private readonly IShortUrlService _shortUrlService;
        private readonly ILogger<ShortUrlsController> _logger;

        public ShortUrlsController(IShortUrlService shortUrlService, ILogger<ShortUrlsController> logger)
        {
            _shortUrlService = shortUrlService;
            _logger = logger;
        }

        [HttpGet]
        public Task<ICollection<ShortUrl>> GetShortUrls() => _shortUrlService.Get();

        [HttpGet("{path}")]
        public Task<ShortUrl> GetShortUrl(string path) => _shortUrlService.Get(path);

        [HttpDelete("{path}")]
        public async Task<IActionResult> DeleteShortUrl(string path)
        {
            await _shortUrlService.Delete(path);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ShortUrl>> CreateShortUrl([FromBody] ShortUrl shortUrl)
        {
            _logger.LogInformation("Creating redirect for path {path} to location {location}",
                shortUrl.ShortPath, shortUrl.Location);
            if ((await _shortUrlService.Get(shortUrl.ShortPath)).IsEmpty)
            {
                await _shortUrlService.Save(shortUrl);
                return Ok(shortUrl);
            }
            _logger.LogWarning("Short path {path} already exists. Use PUT to update.", shortUrl.ShortPath);
            return BadRequest(new ErrorResponse("Already exists. Use PUT to update."));
        }

        [HttpPut]
        public async Task<ShortUrl> CreateOrUpdateShortUrl([FromBody] ShortUrl shortUrl)
        {
            _logger.LogInformation("Saving short url {path}, location is {location}",
                shortUrl.ShortPath, shortUrl.Location);
            await _shortUrlService.Save(shortUrl);
            return shortUrl;
        }
    }
}
