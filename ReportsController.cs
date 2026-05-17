using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    // Daily Report
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport()
    {
        var today = DateTime.UtcNow.Date;

        var sales = await _context.SalesInvoices
            .Where(s => s.SaleDate.Date == today)
            .ToListAsync();

        var totalSales = sales.Sum(s => s.GrandTotal);

        return Ok(new
        {
            Date = today,
            TotalInvoices = sales.Count,
            TotalSalesAmount = totalSales
        });
    }

    // Monthly Report
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport()
    {
        var now = DateTime.UtcNow;

        var sales = await _context.SalesInvoices
            .Where(s => s.SaleDate.Month == now.Month &&
                        s.SaleDate.Year == now.Year)
            .ToListAsync();

        var totalSales = sales.Sum(s => s.GrandTotal);

        return Ok(new
        {
            Month = now.Month,
            Year = now.Year,
            TotalInvoices = sales.Count,
            TotalSalesAmount = totalSales
        });
    }

    // Yearly Report
    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearlyReport()
    {
        var year = DateTime.UtcNow.Year;

        var sales = await _context.SalesInvoices
            .Where(s => s.SaleDate.Year == year)
            .ToListAsync();

        var totalSales = sales.Sum(s => s.GrandTotal);

        return Ok(new
        {
            Year = year,
            TotalInvoices = sales.Count,
            TotalSalesAmount = totalSales
        });
    }
}