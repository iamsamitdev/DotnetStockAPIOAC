using System.ComponentModel.DataAnnotations;

namespace StockAPI.Models;
public class LoginModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username is too long")]
    [MinLength(3, ErrorMessage = "Username is too short")]
    public string? Username { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}