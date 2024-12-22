using Microsoft.AspNetCore.Mvc;

namespace UrlShorter;

[ApiController]
[Route("[controller]")]
public class HelloWorld
{
    [HttpGet]
    public string Get()
    {
        return "Hello World!";
    }
}
