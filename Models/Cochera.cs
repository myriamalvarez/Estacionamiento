using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estacionamiento.Models
{
    public class Cochera
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(20, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Número de Cochera")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(30)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Libre"; // Libre, Ocupada, etc.

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo de Vehículo Compatible")]
        public int IdTipoVehiculo { get; set; }
        [ForeignKey("IdTipoVehiculo")]
        public TipoVehiculo? TipoVehiculo { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public ICollection<Estadia> Estadias { get; set; } = new List<Estadia>();
    }
}