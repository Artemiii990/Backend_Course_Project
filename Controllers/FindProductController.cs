using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

[ApiController]
[Route("api/[controller]")]
public class FindProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public FindProductController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/products
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] string? search)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            // фильтруем по названию продукта (case-insensitive)
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%"));
        }

        var products = await query
            .Select(p => new
            {
                id = p.Id,
                title = p.Name,
                price = p.Price,
                imageUrl = p.ImageUrl
            })
            .ToListAsync();

        return Ok(products);
    }
    
    [HttpGet("category/{slug}")]
    public async Task<IActionResult> GetByCategory(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest("Slug is required");

        slug = slug.Trim().ToLower();

        var products = await _context.Products
            .Where(p => p.CategorySlug != null &&
                        p.CategorySlug.ToLower() == slug)
            .ToListAsync();

        return Ok(products);
    }
    
    
}