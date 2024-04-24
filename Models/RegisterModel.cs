using System.ComponentModel.DataAnnotations;

namespace StockAPI.Models;

public class RegisterModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username is too long")]
    [MinLength(3, ErrorMessage = "Username is too short")]
    public required string Username { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}