namespace Knockout.ConcurrencyDemoCore.Models;

public class Dog
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<Puppy> Puppies { get; set; }
    public bool Stray { get; set; }
}