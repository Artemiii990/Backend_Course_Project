using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public OrdersController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ===============================
    // Получить заказы текущего пользователя
    // ===============================
    [HttpGet("myorders")]
    [Authorize]
    public async Task<IActionResult> GetMyOrders()
    {
        var email = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return BadRequest($"Пользователь с email {email} не найден");

        var orders = await _context.Orders
            .Where(o => o.UserId == user.Id)
            .Select(o => new
            {
                o.Id,
                o.ProductName,
                o.Quantity,
                o.Price,
                o.Date,
                o.Status
            })
            .ToListAsync();

        return Ok(orders);
    }

    // ===============================
    // Создать заказ
    // ===============================
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (dto == null)
            return BadRequest("Данные заказа не переданы");

        var email = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return BadRequest($"Пользователь с email {email} не найден");

        var order = new Order
        {
            UserId = user.Id,
            ProductName = dto.ProductName ?? "",
            Quantity = dto.Quantity,
            Price = dto.Price,
            Date = DateTime.Now,
            Status = "Processing"
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            order.Id,
            order.ProductName,
            order.Quantity,
            order.Price,
            order.Date,
            order.Status
        });
    }

    // ===============================
    // Удалить заказ (админ)
    // ===============================
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Заказ удалён" });
    }

    // ===============================
    // Все заказы (админ)
    // ===============================
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Select(o => new
            {
                o.Id,
                Email = o.User.Email,
                o.ProductName,
                o.Quantity,
                o.Price,
                o.Date,
                o.Status
            })
            .ToListAsync();

        return Ok(orders);
    }

    // ===============================
    // Все пользователи (админ)
    // ===============================
    [HttpGet("all-users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.UserName
            })
            .ToListAsync();

        return Ok(users);
    }

    // ===============================
    // Изменить статус заказа (админ)
    // ===============================
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        order.Status = dto.Status;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            order.Id,
            order.Status
        });
    }
    
    // ===============================
    // Отменить заказ Пользователь
    // ===============================
    
    
    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return BadRequest("User not found");

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        if (order == null)
            return NotFound();

        if (order.Status != "Processing")
            return BadRequest("Нельзя отменить этот заказ");

        order.Status = "Cancelled";

        await _context.SaveChangesAsync();

        return Ok(order);
    }
}