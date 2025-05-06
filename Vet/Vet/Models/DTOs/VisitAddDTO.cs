namespace Vet.Models.DTOs;

public class VisitAddDTO
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
}