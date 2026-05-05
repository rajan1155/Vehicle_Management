using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.Models;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(Customer customer)
    {
        if (customer.User == null)
            return BadRequest(new { message = "User details are required" });

        customer.User.Role = "Customer";

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchCustomer(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "Keyword is required" });

        keyword = keyword.ToLower();

        var customers = await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Where(c =>
                c.Id.ToString().Contains(keyword) ||
                c.PhoneNumber.ToLower().Contains(keyword) ||
                c.User!.FullName.ToLower().Contains(keyword) ||
                c.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(keyword)))
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetCustomerHistory(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        var invoices = await _context.SalesInvoices
            .Where(s => s.CustomerId == id)
            .ToListAsync();

        return Ok(new
        {
            customer,
            salesHistory = invoices
        });
    }
    // GET: api/customers/report/top-spenders
    [HttpGet("report/top-spenders")]
    public async Task<IActionResult> GetTopSpenders()
    {
        var result = await _context.SalesInvoices
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalSpent = g.Sum(x => x.GrandTotal)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }

    // GET: api/customers/report/regular
    [HttpGet("report/regular")]
    public async Task<IActionResult> GetRegularCustomers()
    {
        var result = await _context.SalesInvoices
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalPurchases = g.Count()
            })
            .OrderByDescending(x => x.TotalPurchases)
            .ToListAsync();

        return Ok(result);
    }

    // GET: api/customers/report/credit
    [HttpGet("report/credit")]
    public async Task<IActionResult> GetCustomersWithCredit()
    {
        var customers = await _context.Customers
            .Include(c => c.User)
            .Where(c => c.CreditBalance > 0)
            .ToListAsync();

        return Ok(customers);
    }
}