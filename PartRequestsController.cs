using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.Models;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PartRequestsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPartRequests()
    {
        var requests = await _context.PartRequests
            .Include(r => r.Customer)
            .ThenInclude(c => c!.User)
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePartRequest(PartRequest request)
    {
        var customer = await _context.Customers.FindAsync(request.CustomerId);

        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        request.Status = "Pending";
        request.RequestedAt = DateTime.UtcNow;

        _context.PartRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(request);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateRequestStatus(int id, string status)
    {
        var request = await _context.PartRequests.FindAsync(id);

        if (request == null)
            return NotFound(new { message = "Part request not found" });

        request.Status = status;
        await _context.SaveChangesAsync();

        return Ok(request);
    }
}