using Estacionamiento.Data;
using Estacionamiento.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Api
{
    // Web API REST: autenticación. Devuelve un JWT que el cliente (Postman, una app móvil, etc.)
    // manda después en el header "Authorization: Bearer <token>".
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly EstacionamientoContext _context;
        private readonly ServicioSeguridad _seguridad;

        public AuthController(EstacionamientoContext context, ServicioSeguridad seguridad)
        {
            _context = context;
            _seguridad = seguridad;
        }

        public class LoginRequest
        {
            public string NombreUsuario { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario);

            if (usuario == null || !usuario.Activo || !_seguridad.VerificarPassword(request.Password, usuario.Password))
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

            return Ok(new
            {
                token = _seguridad.GenerarToken(usuario),
                usuario = usuario.NombreUsuario,
                rol = usuario.Rol,
                expiraEnHoras = 8
            });
        }
    }
}