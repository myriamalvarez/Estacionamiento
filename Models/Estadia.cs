using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estacionamiento.Models
{
    public class Estadia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Vehículo")]
        public int IdVehiculo { get; set; }
        [ForeignKey("IdVehiculo")]
        public Vehiculo? Vehiculo { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Cochera")]
        public int IdCochera { get; set; }
        [ForeignKey("IdCochera")]
        public Cochera? Cochera { get; set; }

        [Display(Name = "Tarifa")]
        public int? IdTarifa { get; set; }
        [ForeignKey("IdTarifa")]
        public Tarifa? Tarifa { get; set; }

        [Display(Name = "Abono")]
        public int? IdAbono { get; set; }
        [ForeignKey("IdAbono")]
        public Abono? Abono { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Usuario que registró")]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de Ingreso")]
        public DateTime FechaHoraIngreso { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de Egreso")]
        public DateTime? FechaHoraEgreso { get; set; }

        public bool Estado { get; set; } = true; // Activa / Finalizada

        // Propiedades de navegación
        public Pago? Pago { get; set; }
    }
}