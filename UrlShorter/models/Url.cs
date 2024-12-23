namespace UrlShorter.Models;

public class Url
{
    public long UserId { get; set; }
    public required string ShortedUrl { get; set; }
    public required string OriginalUrl { get; set; }
}