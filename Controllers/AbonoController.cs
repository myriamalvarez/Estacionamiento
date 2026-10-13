using Microsoft.AspNetCore.Authorization;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Controllers
{
    public class AbonoController : Controller
    {
        private readonly EstacionamientoContext _context;

        public AbonoController(EstacionamientoContext context)
        {
            _context = context;
        }

        // La vista es un cascarón Vue; los datos llegan paginados desde ListarJson.
        public IActionResult Index() => View();

        // Listado paginado en el servidor. Filtra por patente o apellido del cliente.
        [HttpGet]
        public async Task<IActionResult> ListarJson(int pagina = 1, int tamanioPagina = 10, string? term = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1) tamanioPagina = 10;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var consulta = _context.Abonos.Where(a => a.Activo);

            if (!string.IsNullOrWhiteSpace(term))
                consulta = consulta.Where(a => a.Vehiculo!.Patente.Contains(term)
                                            || a.Cliente!.Apellido.Contains(term));

            var total = await consulta.CountAsync();
            var hoy = DateTime.Today;

            var items = await consulta
                .OrderByDescending(a => a.FechaInicio)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(a => new
                {
                    a.Id,
                    Cliente = a.Cliente!.Apellido + ", " + a.Cliente.Nombre,
                    Patente = a.Vehiculo!.Patente,
                    FechaInicio = a.FechaInicio.ToString("dd/MM/yyyy"),
                    FechaFin = a.FechaFin.ToString("dd/MM/yyyy"),
                    a.Precio,
                    // Vigente = no cancelado y hoy dentro del período
                    Vigente = a.Estado && a.FechaInicio <= hoy && a.FechaFin >= hoy
                })
                .ToListAsync();

            return Json(new
            {
                items,
                total,
                pagina,
                tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var abono = await _context.Abonos
                .Include(a => a.Cliente)
                .Include(a => a.Vehiculo)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (abono == null) return NotFound();
            return View(abono);
        }

        public async Task<IActionResult> Create()
        {
            return View(new Abono
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddMonths(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Abono abono)
        {
            if (abono.IdCliente <= 0)
                ModelState.AddModelError(nameof(Abono.IdCliente), "Seleccioná un cliente de la lista.");
            if (abono.IdVehiculo <= 0)
                ModelState.AddModelError(nameof(Abono.IdVehiculo), "Seleccioná un vehículo de la lista.");
            if (abono.FechaFin <= abono.FechaInicio)
                ModelState.AddModelError(nameof(Abono.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");

            if (!ModelState.IsValid)
            {
                await CargarSeleccionadosAsync(abono.IdCliente, abono.IdVehiculo);
                return View(abono);
            }

            _context.Abonos.Add(abono);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Abono creado correctamente.";
            return RedirectToAction(nameof(Details), new { id = abono.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono == null) return NotFound();
            await CargarSeleccionadosAsync(abono.IdCliente, abono.IdVehiculo);
            return View(abono);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Abono abono)
        {
            if (id != abono.Id) return BadRequest();
            if (abono.IdCliente <= 0)
                ModelState.AddModelError(nameof(Abono.IdCliente), "Seleccioná un cliente de la lista.");
            if (abono.IdVehiculo <= 0)
                ModelState.AddModelError(nameof(Abono.IdVehiculo), "Seleccioná un vehículo de la lista.");
            if (abono.FechaFin <= abono.FechaInicio)
                ModelState.AddModelError(nameof(Abono.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");

            if (!ModelState.IsValid)
            {
                await CargarSeleccionadosAsync(abono.IdCliente, abono.IdVehiculo);
                return View(abono);
            }

            _context.Update(abono);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Abono actualizado correctamente.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Cancelación manual del abono (distinto de que venza por fecha).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono == null) return NotFound();

            abono.Estado = false;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Abono cancelado.";
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var abono = await _context.Abonos.FindAsync(id);
            if (abono == null) return NotFound();

            abono.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Abono eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // Los selectores son búsquedas Ajax: solo hace falta el texto del cliente/vehículo ya elegido
        // (al editar o al volver con errores de validación), no listas completas.
        private async Task CargarSeleccionadosAsync(int? idCliente, int? idVehiculo)
        {
            if (idCliente is > 0)
            {
                var c = await _context.Clientes.FindAsync(idCliente.Value);
                if (c != null) ViewBag.ClienteNombre = $"{c.Apellido}, {c.Nombre} (DNI {c.Dni})";
            }
            if (idVehiculo is > 0)
            {
                var v = await _context.Vehiculos.FindAsync(idVehiculo.Value);
                if (v != null) ViewBag.VehiculoNombre = $"{v.Patente} — {v.Marca} {v.Modelo}";
            }
        }
    }
}