using System.ComponentModel.DataAnnotations;

namespace Estacionamiento.Models
{
    public class TipoVehiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres")]
        [Display(Name = "Descripción del Tipo")]
        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
        public ICollection<Cochera> Cocheras { get; set; } = new List<Cochera>();
        public ICollection<Tarifa> Tarifas { get; set; } = new List<Tarifa>();
    }
}