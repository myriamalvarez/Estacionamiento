using Estacionamiento.Data;
using Estacionamiento.Models;
using Estacionamiento.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Estacionamiento.Controllers
{
    public class UsuarioController : ControladorBase
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanioMaximoBytes = 2 * 1024 * 1024; // 2 MB, alcanza para un avatar

        private readonly EstacionamientoContext _context;
        private readonly IWebHostEnvironment _entorno;
        private readonly ServicioSeguridad _seguridad;

        public UsuarioController(EstacionamientoContext context, IWebHostEnvironment entorno, ServicioSeguridad seguridad)
        {
            _context = context;
            _entorno = entorno;
            _seguridad = seguridad;
        }

        // ------------------------------------------------------------------
        // Login / Logout
        // ------------------------------------------------------------------

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string nombreUsuario, string password, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
            if (usuario == null || !usuario.Activo || !_seguridad.VerificarPassword(password, usuario.Password))
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View();
            }

            var token = _seguridad.GenerarToken(usuario);

            // El JWT va en una cookie HttpOnly (no accesible desde JS); el middleware de JwtBearer
            // la lee de ahí (ver Program.cs).
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddHours(8)
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccesoDenegado() => View();

        // ------------------------------------------------------------------
        // ABM de usuarios (solo administradores, salvo el propio perfil)
        // ------------------------------------------------------------------

        [Authorize(Policy = "Administrador")]
        public IActionResult Index() => View();

        [Authorize(Policy = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ListarJson(int pagina = 1, int tamanioPagina = 10, string? term = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1) tamanioPagina = 10;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var consulta = _context.Usuarios.Where(u => u.Activo);
            if (!string.IsNullOrWhiteSpace(term))
                consulta = consulta.Where(u => u.NombreUsuario.Contains(term) || u.Apellido.Contains(term));

            var total = await consulta.CountAsync();
            var items = await consulta
                .OrderBy(u => u.Apellido).ThenBy(u => u.Nombre)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(u => new { u.Id, u.NombreUsuario, u.Nombre, u.Apellido, u.Rol })
                .ToListAsync();

            return Json(new
            {
                items, total, pagina, tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }

        // Atajo para que cualquier usuario logueado vea su propio perfil sin conocer su Id.
        [Authorize]
        public IActionResult Perfil() => RedirectToAction(nameof(Details), new { id = UsuarioActualId });

        // Un empleado solo ve su propio perfil; un administrador puede ver cualquiera.
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            if (!EsAdministrador && id != UsuarioActualId) return Forbid();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.UsuarioActualId = UsuarioActualId;
            return View(usuario);
        }

        [Authorize(Policy = "Administrador")]
        public IActionResult Create() => View(new Usuario());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            if (usuario.Password.Length < 6)
            {
                ModelState.AddModelError(nameof(Usuario.Password), "La contraseña debe tener al menos 6 caracteres.");
                return View(usuario);
            }

            usuario.Password = _seguridad.HashearPassword(usuario.Password);

            try
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Details), new { id = usuario.Id });
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(Usuario.NombreUsuario), "Ya existe un usuario con ese nombre de usuario.");
                usuario.Password = string.Empty;
                return View(usuario);
            }
        }

        // Un empleado solo puede editar su propio perfil; un administrador cualquiera.
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            if (!EsAdministrador && id != UsuarioActualId) return Forbid();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (!EsAdministrador && id != UsuarioActualId) return Forbid();
            if (id != usuario.Id) return BadRequest();

            // Password y Avatar se cambian en sus propias acciones.
            ModelState.Remove(nameof(Usuario.Password));
            if (!ModelState.IsValid) return View(usuario);

            var existente = await _context.Usuarios.FindAsync(id);
            if (existente == null) return NotFound();

            existente.NombreUsuario = usuario.NombreUsuario;
            existente.Nombre = usuario.Nombre;
            existente.Apellido = usuario.Apellido;

            // Un empleado no puede cambiarse el rol ni desactivarse a sí mismo.
            if (EsAdministrador)
            {
                existente.Rol = usuario.Rol;
                existente.Activo = usuario.Activo;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(Usuario.NombreUsuario), "Ya existe otro usuario con ese nombre de usuario.");
                return View(usuario);
            }
        }

        // Si cambia SU PROPIA clave debe confirmar la actual; un administrador que resetea la de otro no.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CambiarPassword(
            int id, string? passwordActual, string nuevaPassword, string confirmarPassword)
        {
            if (!EsAdministrador && id != UsuarioActualId) return Forbid();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (id == UsuarioActualId && !_seguridad.VerificarPassword(passwordActual ?? string.Empty, usuario.Password))
            {
                TempData["Error"] = "La contraseña actual no es correcta.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
            {
                TempData["Error"] = "La contraseña nueva debe tener al menos 6 caracteres.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (nuevaPassword != confirmarPassword)
            {
                TempData["Error"] = "Las contraseñas nuevas no coinciden.";
                return RedirectToAction(nameof(Details), new { id });
            }

            usuario.Password = _seguridad.HashearPassword(nuevaPassword);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Contraseña actualizada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Un solo archivo por usuario (avatar_{id}.ext); si tenía otro con distinta extensión, se borra.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SubirAvatar(int id, IFormFile archivo)
        {
            if (!EsAdministrador && id != UsuarioActualId) return Forbid();

            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Elegí una imagen para subir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                TempData["Error"] = "Formato no permitido. Usá JPG, PNG o WEBP.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (archivo.Length > TamanioMaximoBytes)
            {
                TempData["Error"] = "La imagen supera el tamaño máximo permitido (2 MB).";
                return RedirectToAction(nameof(Details), new { id });
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var carpeta = Path.Combine(_entorno.WebRootPath, "uploads", "avatares");
            var nombreArchivo = $"avatar_{id}{extension}";

            try
            {
                Directory.CreateDirectory(carpeta);
                foreach (var existente in Directory.GetFiles(carpeta, $"avatar_{id}.*"))
                    System.IO.File.Delete(existente);

                using var stream = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.Create);
                await archivo.CopyToAsync(stream);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                TempData["Error"] = "No se pudo guardar el avatar en el servidor. Probá de nuevo.";
                return RedirectToAction(nameof(Details), new { id });
            }

            usuario.Avatar = $"/uploads/avatares/{nombreArchivo}";
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Avatar actualizado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EliminarAvatar(int id)
        {
            if (!EsAdministrador && id != UsuarioActualId) return Forbid();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            if (usuario.Avatar != null)
            {
                var ruta = Path.Combine(_entorno.WebRootPath,
                    usuario.Avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            }

            usuario.Avatar = null;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Avatar eliminado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Baja lógica: solo un administrador, y no puede eliminarse a sí mismo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id == UsuarioActualId)
            {
                TempData["Error"] = "No podés eliminar tu propio usuario.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Usuario eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}