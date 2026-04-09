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
}