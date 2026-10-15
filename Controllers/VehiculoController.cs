using System.ComponentModel.DataAnnotations;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Estacionamiento.Controllers
{
    public class VehiculoController : ControladorBase
    {
        private readonly EstacionamientoContext _context;

        public VehiculoController(EstacionamientoContext context)
        {
            _context = context;
        }

        // Datos que viajan entre el formulario Vue y el servidor (en JSON).
        public class VehiculoDto
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "La patente es obligatoria")]
            [StringLength(20, ErrorMessage = "La patente puede tener como máximo 20 caracteres")]
            public string Patente { get; set; } = string.Empty;

            [Required(ErrorMessage = "La marca es obligatoria")]
            [StringLength(50, ErrorMessage = "La marca puede tener como máximo 50 caracteres")]
            public string Marca { get; set; } = string.Empty;

            [Required(ErrorMessage = "El modelo es obligatorio")]
            [StringLength(50, ErrorMessage = "El modelo puede tener como máximo 50 caracteres")]
            public string Modelo { get; set; } = string.Empty;

            [StringLength(30, ErrorMessage = "El color puede tener como máximo 30 caracteres")]
            public string? Color { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Elegí un tipo de vehículo")]
            public int IdTipoVehiculo { get; set; }

            public int? IdCliente { get; set; }
        }

        // La vista es un cascarón: todo el ABM (listar, alta, edición y baja) lo hace Vue por Ajax.
        public IActionResult Index() => View();

        // ---- LISTADO paginado en el servidor ----
        [HttpGet]
        public async Task<IActionResult> ListarJson(int pagina = 1, int tamanioPagina = 10, string? term = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1) tamanioPagina = 10;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var consulta = _context.Vehiculos.Where(v => v.Activo);

            if (!string.IsNullOrWhiteSpace(term))
                consulta = consulta.Where(v => v.Patente.Contains(term));

            var total = await consulta.CountAsync();

            var items = await consulta
                .OrderBy(v => v.Patente)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(v => new
                {
                    v.Id,
                    v.Patente,
                    v.Marca,
                    v.Modelo,
                    Tipo = v.TipoVehiculo!.Descripcion,
                    Cliente = v.Cliente != null ? v.Cliente.Apellido + ", " + v.Cliente.Nombre : null
                })
                .ToListAsync();

            return Json(new
            {
                items, total, pagina, tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }

        // Tipos de vehículo para el desplegable del formulario (tabla maestra chica: se puede cargar completa).
        [HttpGet]
        public async Task<IActionResult> TiposJson()
        {
            var tipos = await _context.TipoVehiculos
                .Where(t => t.Activo)
                .OrderBy(t => t.Descripcion)
                .Select(t => new { t.Id, t.Descripcion })
                .ToListAsync();
            return Json(tipos);
        }

        // ---- MODIFICACIÓN: trae los datos de un vehículo para cargarlos en el formulario ----
        [HttpGet]
        public async Task<IActionResult> ObtenerJson(int id)
        {
            var v = await _context.Vehiculos
                .Where(x => x.Id == id && x.Activo)
                .Select(x => new
                {
                    x.Id, x.Patente, x.Marca, x.Modelo, x.Color, x.IdTipoVehiculo, x.IdCliente,
                    ClienteNombre = x.Cliente != null
                        ? x.Cliente.Apellido + ", " + x.Cliente.Nombre + " (DNI " + x.Cliente.Dni + ")"
                        : null
                })
                .FirstOrDefaultAsync();

            if (v == null) return NotFound(new { mensaje = "El vehículo no existe." });
            return Json(v);
        }

        // ---- ALTA y MODIFICACIÓN (según venga Id = 0 o un Id existente) ----
        [HttpPost]
        public async Task<IActionResult> GuardarJson([FromBody] VehiculoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errores = ErroresDeModelState() });

            if (!await _context.TipoVehiculos.AnyAsync(t => t.Id == dto.IdTipoVehiculo && t.Activo))
                return BadRequest(new { errores = new Dictionary<string, string[]> { ["idTipoVehiculo"] = new[] { "El tipo de vehículo no existe." } } });

            if (dto.IdCliente != null && !await _context.Clientes.AnyAsync(c => c.Id == dto.IdCliente && c.Activo))
                return BadRequest(new { errores = new Dictionary<string, string[]> { ["idCliente"] = new[] { "El cliente no existe." } } });

            Vehiculo vehiculo;
            if (dto.Id == 0)
            {
                vehiculo = new Vehiculo { Activo = true };
                _context.Vehiculos.Add(vehiculo);
            }
            else
            {
                var existente = await _context.Vehiculos.FindAsync(dto.Id);
                if (existente == null || !existente.Activo)
                    return NotFound(new { mensaje = "El vehículo no existe." });
                vehiculo = existente;
            }

            vehiculo.Patente = dto.Patente.Trim().ToUpper();
            vehiculo.Marca = dto.Marca.Trim();
            vehiculo.Modelo = dto.Modelo.Trim();
            vehiculo.Color = string.IsNullOrWhiteSpace(dto.Color) ? null : dto.Color.Trim();
            vehiculo.IdTipoVehiculo = dto.IdTipoVehiculo;
            vehiculo.IdCliente = dto.IdCliente;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                return BadRequest(new { errores = new Dictionary<string, string[]> { ["patente"] = new[] { "Ya existe un vehículo con esa patente." } } });
            }

            return Json(new
            {
                ok = true,
                id = vehiculo.Id,
                mensaje = dto.Id == 0 ? "Vehículo registrado correctamente." : "Vehículo actualizado correctamente."
            });
        }

        // ---- BAJA (lógica: el vehículo puede tener estadías históricas). Solo administradores. ----
        [HttpPost]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> EliminarJson(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return NotFound(new { mensaje = "El vehículo no existe." });

            vehiculo.Activo = false;
            await _context.SaveChangesAsync();
            return Json(new { ok = true, mensaje = "Vehículo eliminado." });
        }

        // ---- Detalle (página común, con las últimas estadías) ----
        public async Task<IActionResult> Details(int id)
        {
            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.TipoVehiculo)
                .Include(v => v.Estadias)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return NotFound();
            return View(vehiculo);
        }

        // Búsqueda Ajax por patente (la usa Ingreso). Máximo 10 resultados.
        [HttpGet]
        public async Task<IActionResult> BuscarPorPatente(string? term)
        {
            term ??= string.Empty;
            var vehiculos = await _context.Vehiculos
                .Where(v => v.Activo && v.Patente.Contains(term))
                .OrderBy(v => v.Patente)
                .Take(10)
                .Select(v => new
                {
                    v.Id, v.Patente, v.Marca, v.Modelo, v.IdTipoVehiculo,
                    Tipo = v.TipoVehiculo!.Descripcion
                })
                .ToListAsync();
            return Json(vehiculos);
        }

        private Dictionary<string, string[]> ErroresDeModelState() =>
            ModelState
                .Where(e => e.Value!.Errors.Count > 0)
                .ToDictionary(
                    e => char.ToLowerInvariant(e.Key[0]) + e.Key.Substring(1),
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
    }
}