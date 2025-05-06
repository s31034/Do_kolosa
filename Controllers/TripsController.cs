using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using TravelAgencyAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TravelAgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TripsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetTrips()
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                var command = new SqlCommand(
                    @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                     STRING_AGG(c.Name, ', ') AS Countries
                     FROM Trip t
                     JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                     JOIN Country c ON ct.IdCountry = c.IdCountry
                     GROUP BY t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
                     ORDER BY t.DateFrom DESC", connection);
                
                connection.Open();
                var reader = command.ExecuteReader();
                
                var trips = new List<Trip>();
                while (reader.Read())
                {
                    trips.Add(new Trip
                    {
                        IdTrip = (int)reader["IdTrip"],
                        Name = (string)reader["Name"],
                        Description = (string)reader["Description"],
                        DateFrom = (DateTime)reader["DateFrom"],
                        DateTo = (DateTime)reader["DateTo"],
                        MaxPeople = (int)reader["MaxPeople"],
                        Countries = (string)reader["Countries"]
                    });
                }
                
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}