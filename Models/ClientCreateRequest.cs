using System.ComponentModel.DataAnnotations;

namespace TravelAgencyAPI.Models;

public class ClientCreateRequest
{
    [Required]
    [MaxLength(120)]
    public string FirstName { get; set; }
        
    [Required]
    [MaxLength(120)]
    public string LastName { get; set; }
        
    [Required]
    [MaxLength(120)]
    [EmailAddress]
    public string Email { get; set; }
        
    [Required]
    [MaxLength(120)]
    public string Telephone { get; set; }
        
    [Required]
    [MaxLength(120)]
    public string Pesel { get; set; }
}
