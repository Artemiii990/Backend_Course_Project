namespace WebApplication1.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
}