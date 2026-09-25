using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estacionamiento.Models
{
    public class Tarifa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(30)]
        [Display(Name = "Unidad de Tiempo")]
        public string UnidadTiempo { get; set; } = string.Empty; // Hora, Día

        public bool Activa { get; set; } = true;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Tipo de Vehículo")]
        public int IdTipoVehiculo { get; set; }
        [ForeignKey("IdTipoVehiculo")]
        public TipoVehiculo? TipoVehiculo { get; set; }

        // Propiedades de navegación
        public ICollection<Estadia> Estadias { get; set; } = new List<Estadia>();
    }
}
