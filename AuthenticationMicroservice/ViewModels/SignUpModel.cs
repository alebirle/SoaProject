using System.ComponentModel.DataAnnotations;

namespace AuthenticationMicroservice.ViewModels;

public class SignUpModel
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(20, ErrorMessage = "Your username should not be more than 20 characters long")]
    public string Username { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}
