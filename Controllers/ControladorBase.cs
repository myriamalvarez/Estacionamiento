using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamiento.Controllers
{
    // Acceso al usuario logueado, leído de los claims del JWT.
    public abstract class ControladorBase : Controller
    {
        public const string RolAdministrador = "Administrador";
        public const string RolEmpleado = "Empleado";

        protected int? UsuarioActualId
        {
            get
            {
                var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(valor, out var id) ? id : null;
            }
        }

        protected bool EsAdministrador => User.IsInRole(RolAdministrador);
    }
}