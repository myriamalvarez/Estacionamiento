using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estacionamiento.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Estadía")]
        public int IdEstadia { get; set; }
        [ForeignKey("IdEstadia")]
        public Estadia? Estadia { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Display(Name = "Usuario que registró el cobro")]
        public int IdUsuarioRegistro { get; set; }
        [ForeignKey("IdUsuarioRegistro")]
        public Usuario? UsuarioRegistro { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de Pago")]
        public DateTime FechaHoraPago { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Método de Pago")]
        public string MetodoPago { get; set; } = string.Empty; // Efectivo, Transferencia, etc.

        public bool Estado { get; set; } = true;

        [StringLength(255)]
        [Display(Name = "Comprobante Digital")]
        public string? Comprobante { get; set; } // Ruta del archivo PDF o imagen del pago
    }
}