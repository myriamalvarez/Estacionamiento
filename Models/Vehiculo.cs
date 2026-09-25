using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estacionamiento.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(10, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Patente")]
        public string Patente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo de Vehículo")]
        public int IdTipoVehiculo { get; set; }
        [ForeignKey("IdTipoVehiculo")]
        public TipoVehiculo? TipoVehiculo { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public ICollection<Abono> Abonos { get; set; } = new List<Abono>();
        public ICollection<Estadia> Estadias { get; set; } = new List<Estadia>();
    }
}