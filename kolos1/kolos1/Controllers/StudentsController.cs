
using kolos1.Services;

namespace kolos1.Controllers;
using kolos1.DTOs;
using kolos1.Exceptions;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("[controller]")]
public class StudentsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStudentsDetails([FromQuery] string? fName)
    {
        return Ok(await service.GetStudentDetailsAsync(fName));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto body)
    {
        try
        {
            var student = await service.CreateStudentAsync(body);
            return Created($"students/{student.Id}", student);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}