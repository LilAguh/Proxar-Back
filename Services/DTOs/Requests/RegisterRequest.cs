using System.ComponentModel.DataAnnotations;

namespace Services.DTOs.Requests;

public class RegisterRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la empresa es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre de la empresa debe tener entre 2 y 100 caracteres")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El slug es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El slug debe tener entre 2 y 50 caracteres")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones")]
    public string CompanySlug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }
}
