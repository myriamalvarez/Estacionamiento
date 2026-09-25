using System.ComponentModel.DataAnnotations;

namespace Estacionamiento.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(255, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(30)]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty; // Administrador / Empleado

        public bool Activo { get; set; } = true;

        [StringLength(255)]
        [Display(Name = "Avatar")]
        public string? Avatar { get; set; } // Ruta del archivo de imagen de perfil

        // Propiedades de navegación
        public ICollection<Estadia> Estadias { get; set; } = new List<Estadia>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}