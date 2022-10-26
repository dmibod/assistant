namespace myapp.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("people")]
public class MainController : ControllerBase
{
    private static List<Person> persons = new()
    {
        new Person { Name = "Ana" },
        new Person { Name = "Felipe" },
        new Person { Name = "Emillia" }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Person>> GetAll()
    {
        return persons;
    }

    [HttpGet("{name}")]
    public ActionResult<Person> One(string name)
    {
        var person = persons.FirstOrDefault(p => p.Name == name);

        return person == null ? this.NotFound() : person;
    }
    
    [HttpPatch("{name}")]
    public ActionResult<Person> Rename(string name, string newName)
    {
        var person = persons.FirstOrDefault(p => p.Name == name);

        if (person != null)
        {
            person.Name = newName;
            
            return person;
        }
        
        return this.NotFound();
    }
    
    [HttpDelete("{name}")]
    public ActionResult<Person> Remove(string name)
    {
        var person = persons.FirstOrDefault(p => p.Name == name);

        if (person != null)
        {
            persons.Remove(person);
            return person;
        }

        return this.NotFound();
    }
}

public class Person
{
    public string Name { get; set; }
}