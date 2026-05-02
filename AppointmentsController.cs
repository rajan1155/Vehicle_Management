using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.Models;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .ThenInclude(c => c!.User)
            .Include(a => a.Vehicle)
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment(Appointment appointment)
    {
        if (appointment == null)
            return BadRequest();

        appointment.AppointmentDate =
            appointment.AppointmentDate.ToUniversalTime();

        appointment.Status = "Pending";

        _context.Appointments.Add(appointment);

        await _context.SaveChangesAsync();

        return Ok(appointment);
    }
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound(new { message = "Appointment not found" });

        appointment.Status = status;
        await _context.SaveChangesAsync();

        return Ok(appointment);
    }
}