using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using TravelAgencyAPI.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace TravelAgencyAPI.Controllers
{
    [Route("api/[controller]")] // Definiuje bazową ścieżkę routingu jako /api/clients
    [ApiController] // Oznacza klasę jako kontroler API
    public class ClientsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Konstruktor wstrzykujący konfigurację (dostęp do connection string)
        public ClientsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET /api/clients/{id}/trips - Pobiera wycieczki konkretnego klienta
        [HttpGet("{id}/trips")]
        public IActionResult GetClientTrips(int id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                
                // 1. Sprawdź czy klient istnieje
                var checkClientCommand = new SqlCommand("SELECT COUNT(*) FROM Client WHERE IdClient = @IdClient", connection);
                checkClientCommand.Parameters.AddWithValue("@IdClient", id);
                
                connection.Open();
                if ((int)checkClientCommand.ExecuteScalar() == 0)
                    return NotFound("Client not found"); // 404 jeśli klient nie istnieje
                
                // 2. Pobierz wycieczki klienta
                var command = new SqlCommand(
                    @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                     ct.RegisteredAt, ct.PaymentDate
                     FROM Trip t
                     JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
                     WHERE ct.IdClient = @IdClient
                     ORDER BY t.DateFrom DESC", connection);
                command.Parameters.AddWithValue("@IdClient", id);
                
                var reader = command.ExecuteReader();
                var trips = new List<ClientTripResponse>();
                
                // 3. Mapowanie wyników na obiekty
                while (reader.Read())
                {
                    trips.Add(new ClientTripResponse
                    {
                        Trip = new Trip
                        {
                            IdTrip = (int)reader["IdTrip"],
                            Name = (string)reader["Name"],
                            Description = (string)reader["Description"],
                            DateFrom = (DateTime)reader["DateFrom"],
                            DateTo = (DateTime)reader["DateTo"],
                            MaxPeople = (int)reader["MaxPeople"]
                        },
                        RegisteredAt = (DateTime)reader["RegisteredAt"],
                        PaymentDate = reader["PaymentDate"] as DateTime?
                    });
                }
                
                return Ok(trips); // 200 OK z listą wycieczek
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // 500 dla błędów serwera
            }
        }

        // POST /api/clients - Tworzy nowego klienta
        [HttpPost]
        public IActionResult CreateClient([FromBody] ClientCreateRequest request)
        {
            // Automatyczna walidacja modelu na podstawie atrybutów w ClientCreateRequest
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400 jeśli dane są nieprawidłowe
            
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                // 1. Wstawienie nowego klienta i zwrócenie jego ID
                var command = new SqlCommand(
                    @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                     VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                     SELECT SCOPE_IDENTITY();", connection);
                
                // Parametryzowane zapytanie (ochrona przed SQL Injection)
                command.Parameters.AddWithValue("@FirstName", request.FirstName);
                command.Parameters.AddWithValue("@LastName", request.LastName);
                command.Parameters.AddWithValue("@Email", request.Email);
                command.Parameters.AddWithValue("@Telephone", request.Telephone);
                command.Parameters.AddWithValue("@Pesel", request.Pesel);
                
                connection.Open();
                var newId = Convert.ToInt32(command.ExecuteScalar());
                
                // 201 Created z nagłówkiem Location i ID nowego klienta
                return CreatedAtAction(nameof(GetClientTrips), new { id = newId }, new { IdClient = newId });
            }
            catch (SqlException ex) when (ex.Number == 2627) // Duplicate key error
            {
                return Conflict("Client with this PESEL already exists"); // 409 Conflict dla duplikatu PESEL
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT /api/clients/{id}/trips/{tripid} - Rejestruje klienta na wycieczkę
        [HttpPut("{id}/trips/{tripid}")]
        public IActionResult AssignClientToTrip(int id, int tripid)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();
                
                // 1. Walidacja czy klient istnieje
                var checkClientCommand = new SqlCommand("SELECT COUNT(*) FROM Client WHERE IdClient = @IdClient", connection);
                checkClientCommand.Parameters.AddWithValue("@IdClient", id);
                if ((int)checkClientCommand.ExecuteScalar() == 0)
                    return NotFound("Client not found");
                    
                // 2. Walidacja czy wycieczka istnieje i pobranie max uczestników
                var checkTripCommand = new SqlCommand("SELECT COUNT(*), MaxPeople FROM Trip WHERE IdTrip = @IdTrip GROUP BY MaxPeople", connection);
                checkTripCommand.Parameters.AddWithValue("@IdTrip", tripid);
                var tripResult = checkTripCommand.ExecuteReader();
                
                if (!tripResult.HasRows)
                {
                    tripResult.Close();
                    return NotFound("Trip not found");
                }
                
                tripResult.Read();
                var maxPeople = (int)tripResult["MaxPeople"];
                tripResult.Close();
                
                // 3. Sprawdzenie liczby uczestników
                var countParticipantsCommand = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip", connection);
                countParticipantsCommand.Parameters.AddWithValue("@IdTrip", tripid);
                var currentParticipants = (int)countParticipantsCommand.ExecuteScalar();
                
                if (currentParticipants >= maxPeople)
                    return BadRequest("Trip is already full"); // 400 jeśli brak miejsc
                    
                // 4. Sprawdzenie czy klient już jest zapisany
                var checkAssignmentCommand = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", connection);
                checkAssignmentCommand.Parameters.AddWithValue("@IdClient", id);
                checkAssignmentCommand.Parameters.AddWithValue("@IdTrip", tripid);
                if ((int)checkAssignmentCommand.ExecuteScalar() > 0)
                    return BadRequest("Client is already assigned to this trip");
                    
                // 5. Rejestracja klienta
                var insertCommand = new SqlCommand(
                    @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                     VALUES (@IdClient, @IdTrip, GETDATE())", connection);
                insertCommand.Parameters.AddWithValue("@IdClient", id);
                insertCommand.Parameters.AddWithValue("@IdTrip", tripid);
                
                insertCommand.ExecuteNonQuery();
                
                return Ok("Client successfully assigned to trip"); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE /api/clients/{id}/trips/{tripid} - Usuwa rejestrację klienta z wycieczki
        [HttpDelete("{id}/trips/{tripid}")]
        public IActionResult RemoveClientFromTrip(int id, int tripid)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();
                
                // 1. Sprawdzenie czy przypisanie istnieje
                var checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", connection);
                checkCommand.Parameters.AddWithValue("@IdClient", id);
                checkCommand.Parameters.AddWithValue("@IdTrip", tripid);
                
                if ((int)checkCommand.ExecuteScalar() == 0)
                    return NotFound("Assignment not found"); // 404 jeśli nie znaleziono
                    
                // 2. Usunięcie rejestracji
                var deleteCommand = new SqlCommand(
                    "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", connection);
                deleteCommand.Parameters.AddWithValue("@IdClient", id);
                deleteCommand.Parameters.AddWithValue("@IdTrip", tripid);
                
                deleteCommand.ExecuteNonQuery();
                
                return NoContent(); // 204 No Content dla operacji DELETE
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}