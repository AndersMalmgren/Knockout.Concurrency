using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Knockout.Concurrency.Demo.Common;
using Knockout.Concurrency.Demo.Common.Extensions;
using Knockout.Concurrency.Demo.Events;
using Knockout.Concurrency.Demo.Models;

namespace Knockout.Concurrency.Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventAggregator eventAggregator;
        private static int idCounter = 200;

        public HomeController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetDog()
        {
            return new Dog
            {
                Id = 1, 
                Name = "Wolfie", 
                Stray = true, 
                Puppies = new[]
                {
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
                }
            }
            .AsJson();
        }

        [HttpPost]
        public JsonResult Save(Message<Dog> dog)
        {
            SimulateSave(dog.Data);
            eventAggregator.Publish(dog);
            return dog.Data.AsJson();
        }

        private void SimulateSave(Dog dog)
        {
            dog.Puppies
                .Where(p => p.Id == 0)
                .ForEach(p => p.Id = idCounter++);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
