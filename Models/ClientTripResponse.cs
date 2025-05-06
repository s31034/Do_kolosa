namespace TravelAgencyAPI.Models;

public class ClientTripResponse
{
    public Trip Trip { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
}