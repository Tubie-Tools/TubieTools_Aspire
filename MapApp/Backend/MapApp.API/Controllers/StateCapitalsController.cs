using MapApp.API.Data;
using MapApp.API.DTOs;
using MapApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace MapApp.API.Controllers;

/// <summary>
/// Controller for managing state capital information
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StateCapitalsController : ControllerBase
{
    private readonly MapAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<StateCapitalsController> _logger;

    public StateCapitalsController(MapAppDbContext context, IMapper mapper, ILogger<StateCapitalsController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all state capitals
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StateCapitalDto>>> GetAllCapitals()
    {
        try
        {
            var capitals = await _context.StateCapitals.ToListAsync();
            var dtos = _mapper.Map<List<StateCapitalDto>>(capitals);

            // Add pin colors based on sales status
            foreach (var dto in dtos)
            {
                dto.PinColor = dto.HasSoldProducts ? "#FF6B6B" : "#D3D3D3"; // Red for sold, gray for not sold
            }

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all capitals");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get state capital by state code
    /// </summary>
    [HttpGet("{stateCode}")]
    public async Task<ActionResult<StateCapitalDto>> GetCapitalByState(string stateCode)
    {
        try
        {
            var capital = await _context.StateCapitals.FirstOrDefaultAsync(c => c.StateCode == stateCode);
            if (capital == null)
                return NotFound($"Capital for state {stateCode} not found");

            var dto = _mapper.Map<StateCapitalDto>(capital);
            dto.PinColor = dto.HasSoldProducts ? "#FF6B6B" : "#D3D3D3";
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving capital for state {StateCode}", stateCode);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all capitals by region
    /// </summary>
    [HttpGet("region/{region}")]
    public async Task<ActionResult<IEnumerable<StateCapitalDto>>> GetCapitalsByRegion(string region)
    {
        try
        {
            var capitals = await _context.StateCapitals
                .Where(c => c.Region == region)
                .ToListAsync();

            if (!capitals.Any())
                return NotFound($"No capitals found in region {region}");

            var dtos = _mapper.Map<List<StateCapitalDto>>(capitals);
            foreach (var dto in dtos)
            {
                dto.PinColor = dto.HasSoldProducts ? "#FF6B6B" : "#D3D3D3";
            }

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving capitals by region {Region}", region);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get capitals where we have sold products
    /// </summary>
    [HttpGet("sales/sold-to")]
    public async Task<ActionResult<IEnumerable<StateCapitalDto>>> GetCapitalsWithSales()
    {
        try
        {
            var capitals = await _context.StateCapitals
                .Where(c => c.HasSoldProducts)
                .OrderByDescending(c => c.TotalSalesAmount)
                .ToListAsync();

            var dtos = _mapper.Map<List<StateCapitalDto>>(capitals);
            foreach (var dto in dtos)
            {
                dto.PinColor = "#FF6B6B"; // Always red for sold
            }

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving capitals with sales");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get sales statistics
    /// </summary>
    [HttpGet("sales/statistics")]
    public async Task<ActionResult<SalesStatisticsDto>> GetSalesStatistics()
    {
        try
        {
            var capitals = await _context.StateCapitals.ToListAsync();

            var stats = new SalesStatisticsDto
            {
                TotalStates = capitals.Count,
                StatesWithSales = capitals.Count(c => c.HasSoldProducts),
                StatesWithoutSales = capitals.Count(c => !c.HasSoldProducts),
                TotalSalesAmount = capitals.Sum(c => c.TotalSalesAmount),
                TotalProductsSold = capitals.Sum(c => c.ProductsSold),
                AverageSalesPerState = capitals.Where(c => c.HasSoldProducts).Any() 
                    ? capitals.Where(c => c.HasSoldProducts).Average(c => c.TotalSalesAmount)
                    : 0,
                TopSellingStates = capitals
                    .Where(c => c.HasSoldProducts)
                    .OrderByDescending(c => c.TotalSalesAmount)
                    .Take(10)
                    .Select(c => new TopSellingStateDto 
                    { 
                        StateCode = c.StateCode, 
                        StateName = c.StateName, 
                        TotalSales = c.TotalSalesAmount,
                        ProductsSold = c.ProductsSold
                    })
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update capital sales information
    /// </summary>
    [HttpPut("{stateCode}/sales")]
    public async Task<IActionResult> UpdateCapitalSales(string stateCode, [FromBody] UpdateSalesDto dto)
    {
        try
        {
            var capital = await _context.StateCapitals.FirstOrDefaultAsync(c => c.StateCode == stateCode);
            if (capital == null)
                return NotFound($"Capital for state {stateCode} not found");

            capital.HasSoldProducts = dto.HasSoldProducts;
            capital.TotalSalesAmount = dto.TotalSalesAmount;
            capital.ProductsSold = dto.ProductsSold;
            capital.LastSaleDate = dto.LastSaleDate ?? DateTime.UtcNow;
            capital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sales for state {StateCode}", stateCode);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class SalesStatisticsDto
{
    public int TotalStates { get; set; }
    public int StatesWithSales { get; set; }
    public int StatesWithoutSales { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int TotalProductsSold { get; set; }
    public decimal AverageSalesPerState { get; set; }
    public List<TopSellingStateDto> TopSellingStates { get; set; } = new();
}

public class TopSellingStateDto
{
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int ProductsSold { get; set; }
}

public class UpdateSalesDto
{
    public bool HasSoldProducts { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int ProductsSold { get; set; }
    public DateTime? LastSaleDate { get; set; }
}
