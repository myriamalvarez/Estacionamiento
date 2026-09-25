using System.ComponentModel.DataAnnotations;

namespace Estacionamiento.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(100, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZÀ-ÖØ-öø-ÿ\s]+$", ErrorMessage = "El campo {0} solo puede contener letras")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(100, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-ZÀ-ÖØ-öø-ÿ\s]+$", ErrorMessage = "El campo {0} solo puede contener letras")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(8, ErrorMessage = "El campo {0} debe tener {2} caracteres", MinimumLength = 7)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El campo {0} solo debe contener números")]
        [Display(Name = "DNI")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(15, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres", MinimumLength = 7)]
        [RegularExpression(@"^[0-9\-\+\s\(\)]+$", ErrorMessage = "El campo {0} formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El campo {0} debe ser un correo electrónico válido")]
        [StringLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
        public ICollection<Abono> Abonos { get; set; } = new List<Abono>();
    }
}