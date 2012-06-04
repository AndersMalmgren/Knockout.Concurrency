using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knockout.Concurrency.Demo.Models
{
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Puppy> Puppies { get; set; }
        public bool Stray { get; set; }
    }
}