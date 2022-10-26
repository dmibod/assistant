using Microsoft.AspNetCore.Mvc;

namespace myapp.Controllers;

[ApiController]
[Route("myapp")]
public class MainController : ControllerBase
{
    [HttpGet("all")]
    public ActionResult<IEnumerable<Person>> GetAll()
    {
        return new []
        {
            new Person { Name = "Ana" },
            new Person { Name = "Felipe" },
            new Person { Name = "Emillia" }
        };
    }
}

public class Person
{
    public string Name { get; set; }
}
