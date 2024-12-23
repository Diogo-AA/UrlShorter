using Microsoft.AspNetCore.Mvc;
using UrlShorter.Models;
using UrlShorter.Db;

namespace UrlShorter.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly DbController _dbController;

    public UserController(DbController dbController)
    {
        _dbController = dbController;
    }

    [HttpGet]
    public List<User> Get()
    {
        return _dbController.GetAllUsers();
    }

    [HttpGet("/users/{id:long}")]
    public ActionResult<User> Get(long id)
    {
        var user = _dbController.GetUser(id);

        if (user is null)
            return NotFound("User not found");
        return Ok(user);
    }

    [HttpGet("/users/{username}")]
    public ActionResult<User> Get(string username)
    {
        var user = _dbController.GetUser(username);

        if (user is null)
            return NotFound("User not found");
        return Ok(user);
    }

    [HttpPost]
    public ActionResult<User> Post([FromBody] string username)
    {
        var user = _dbController.AddUser(username);

        if (user is null)
            return BadRequest("User already exists");
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpDelete("/users/{id:long}")]
    public ActionResult Delete(long id)
    {
        return _dbController.DeleteUser(id) ? Ok("User deleted correctly") : NotFound("User not found");
    }
}
