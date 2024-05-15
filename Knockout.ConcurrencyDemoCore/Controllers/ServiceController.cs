using Knockout.ConcurrencyDemoCore.Events;
using Knockout.ConcurrencyDemoCore.Models;
using Microsoft.AspNetCore.Mvc;
using SignalR.EventAggregatorProxy.Extensions;

namespace Knockout.ConcurrencyDemoCore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceController : Controller
{
    private readonly IEventAggregator _eventAggregator;
    private static int _idCounter = 200;

    public ServiceController(IEventAggregator eventAggregator)
    {
        this._eventAggregator = eventAggregator;
    }

    [HttpGet]
    public Dog Get()
    {
        return new Dog
        {
            Id = 1,
            Name = "Wolfie",
            Stray = true,
            Puppies = [
                new Puppy
                {
                    Id = 1,
                    Name = "Sugar"
                },
                new Puppy
                {
                    Id = 2,
                    Name = "Fluffy"
                }
            ]
        };
    }

    [HttpPost("Save")]
    public Dog Save(Message<Dog> dog)
    {
        SimulateSave(dog.Data);
        _eventAggregator.Publish(dog);
        return dog.Data;
    }

    private void SimulateSave(Dog dog)
    {
        dog.Puppies
            .Where(p => p.Id == 0)
            .ForEach(p => p.Id = _idCounter++);
    }
}
