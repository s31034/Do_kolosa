using Vet.Models;

namespace Vet;

public class Database
{
    private static List<Animal> Animals = 
    [
        new ()
        {
            Id = 1,
            Name = "Pimpek",
            Category = "Dog",
            CoatColor = "black",
            Weight = 25.3
        },
        new ()
        {
            Id = 2,
            Name = "Zoe",
            Category = "Dog",
            CoatColor = "ginger",
            Weight = 20.2
        }
    ];

    private static List<Visit> Visits =
    [
        new ()
        {
            Id = 1,
            Animal = Animals[0],
            Date = DateTime.Now,
            Description = "TODO",
            Price = 50.00,
        },
        new ()
        {
            Id = 2,
            Animal = Animals[0],
            Date = DateTime.Now + new TimeSpan(1,0,0,0),
            Description = "TODO",
            Price = 50.00,
        }
    ];
    
    public static List<Animal> GetAnimals()
    {
        return Animals;
    }

    public static void AddAnimal(Animal animal)
    {
        Animals.Add(animal);
    }

    public static void RemoveAnimal(Animal animal)
    {
        Animals.Remove(animal);
    }

    public static List<Visit> GetVisits()
    {
        return Visits;
    }

    public static void AddVisit(Visit visit)
    {
        Visits.Add(visit);
    }
}