using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Vet.Models;
using Vet.Models.DTOs;

namespace Vet.Controllers;

[ApiController]
[Route("[controller]")]
public class AnimalsController(IMapper mapperService) : ControllerBase
{
    [HttpGet]
    // Pobieranie listy zwierzat
    public IActionResult GetAnimals(
        [FromQuery] string? name // opcjonalny parametr do filtrowania
    )
    {
        // Pobranie wszystkich zwierzat z "bazy danych"
        var result = Database.GetAnimals();

        // Jezeli parametr zostal podany przy zapytaniu,
        // filtrujemy sobie dodatkowo nasza liste zwierzat
        if (name is not null)
        {
            result = result.Where(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        // Zwracamy rezultat
        return Ok(mapperService.Map<IEnumerable<AnimalGetDTO>>(result));
    }

    [HttpGet]
    [Route("{id:int}")]
    // Pobieranie konkretnego zwierzecia po id
    public IActionResult GetAnimalById(
        [FromRoute] int id
    )
    {
        // Proba pobrania zwierzecia po id z "bazy"
        var result = Database.GetAnimals().FirstOrDefault(x => x.Id == id);
        
        // Sprawdzenie, czy zwierze istnieje - jezeli nie, to zwracamy odpowiedni kod z komunikatem
        if (result is null)
        {
            return NotFound("Animal with given id does not exist");
        }

        // Jezeli zwierze istnieje, to je zwracamy
        return Ok(mapperService.Map<AnimalGetDTO>(result));
    }

    [HttpPost]
    // Dodawanie nowego zwierzecia
    public IActionResult AddAnimal(
        [FromBody] AnimalAddDTO animalDto
    )
    {
        // Generowanie nowego id
        var nextId = Database.GetAnimals().Max(x => x.Id) + 1;
        
        // Utworzenie obiektu animal
        var animal = new Animal
        {
            Id = nextId,
            Name = animalDto.Name,
            Weight = animalDto.Weight,
            CoatColor = animalDto.CoatColor,
            Category = animalDto.Category,
        };
        
        // Dodanie zwierzecia do bazy
        Database.AddAnimal(animal);
        
        return Created($"animals/{nextId}", animal);
    }

    [HttpPut]
    [Route("{id:int}")]
    // Podmiana zwierzecia o danym id (pelna aktualizacja)
    public IActionResult ReplaceAnimalById(
        [FromRoute] int id,
        [FromBody] AnimalAddDTO animalDto
    )
    {
        // Proba pobrania zwierzecia po id z "bazy"
        var animalToUpdate = Database.GetAnimals().FirstOrDefault(x => x.Id == id);
        
        // Sprawdzenie, czy zwierze istnieje - jezeli nie, to zwracamy odpowiedni kod z komunikatem
        if (animalToUpdate is null)
        {
            return NotFound("Animal with given id does not exist");
        }
        
        // Aktualizacja wartosci po referencji
        animalToUpdate.Name = animalDto.Name;
        animalToUpdate.Category = animalDto.Category;
        animalToUpdate.Weight = animalDto.Weight;
        animalToUpdate.CoatColor = animalDto.CoatColor;
        
        // Zwrocenie kodu informujacego poprawnosc wykonania zadania
        return NoContent();
    }

    [HttpDelete]
    [Route("{id:int}")]
    // Usuniecie zwierzecia o podanym id
    public IActionResult RemoveAnimalById(
        [FromRoute] int id
    )
    {
        // Proba pobrania zwierzecia po id z "bazy"
        var animalToRemove = Database.GetAnimals().FirstOrDefault(x => x.Id == id);
        
        // Sprawdzenie, czy zwierze istnieje - jezeli nie, to zwracamy odpowiedni kod z komunikatem
        if (animalToRemove is null)
        {
            return NotFound("Animal with given id does not exist");
        }
        
        // Usuniecie zwierzecia z bazy
        Database.RemoveAnimal(animalToRemove);

        // Zwrocenie kodu informujacego poprawnosc wykonania zadania
        return NoContent();
    }

    [HttpGet]
    [Route("{animalId:int}/visits")]
    // Pobranie wizyt danego zwierzecia
    public IActionResult GetAnimalsVisits(
        [FromRoute] int animalId
    )
    {
        // Pobranie zwierzecia i sprawdzenie, czy istnieje
        var animal = Database.GetAnimals().FirstOrDefault(x => x.Id == animalId);
        if (animal is null)
        {
            return NotFound("Animal with given id does not exist");
        }
        
        // Pobranie wizyt danego zwierzecia
        var visits = Database.GetVisits().Where(v => v.Animal == animal).ToList();
        
        return Ok(mapperService.Map<IEnumerable<VisitGetDTO>>(visits));
    }

    [HttpPost]
    [Route("{animalId:int}/visits")]
    // Dodanie wizyty dla danego zwierzecia
    public IActionResult AddVisitForAnimal(
        [FromRoute] int animalId,
        [FromBody] VisitAddDTO visitDto
    )
    {
        // Pobranie zwierzecia i sprawdzenie, czy istnieje
        var animal = Database.GetAnimals().FirstOrDefault(x => x.Id == animalId);
        if (animal is null)
        {
            return NotFound("Animal with given id does not exist");
        }
        
        var nextId = Database.GetVisits().Max(x => x.Id) + 1;

        var visit = new Visit
        {
            Id = nextId,
            Animal = animal,
            Date = visitDto.Date,
            Description = visitDto.Description,
            Price = visitDto.Price
        };
        
        Database.AddVisit(visit);
        
        return Created($"animals/{animalId}/visits", visit);
    }
}