using Microsoft.AspNetCore.Authorization;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Controllers
{
    [Authorize(Policy = "Administrador")]
    public class TarifaController : Controller
    {
        private readonly EstacionamientoContext _context;

        // "Hora"/"Dia" son los dos valores que espera EstadiaController.Egreso al elegir tarifa;
        // si agregás una tercera unidad, recordá actualizar también esa lógica de cálculo.
        private static readonly string[] UnidadesTiempo = { "Hora", "Dia" };

        public TarifaController(EstacionamientoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tarifas = await _context.Tarifas
                .Include(t => t.TipoVehiculo)
                .OrderBy(t => t.TipoVehiculo!.Descripcion)
                .ThenBy(t => t.UnidadTiempo)
                .ToListAsync();
            return View(tarifas);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tarifa = await _context.Tarifas
                .Include(t => t.TipoVehiculo)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tarifa == null) return NotFound();
            return View(tarifa);
        }

        public async Task<IActionResult> Create()
        {
            await CargarListasAsync();
            return View(new Tarifa());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tarifa tarifa)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync(tarifa.IdTipoVehiculo, tarifa.UnidadTiempo);
                return View(tarifa);
            }

            _context.Tarifas.Add(tarifa);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Tarifa creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tarifa = await _context.Tarifas.FindAsync(id);
            if (tarifa == null) return NotFound();
            await CargarListasAsync(tarifa.IdTipoVehiculo, tarifa.UnidadTiempo);
            return View(tarifa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tarifa tarifa)
        {
            if (id != tarifa.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await CargarListasAsync(tarifa.IdTipoVehiculo, tarifa.UnidadTiempo);
                return View(tarifa);
            }

            _context.Update(tarifa);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Tarifa actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Baja lógica (usa el campo Activa, propio de esta entidad).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var tarifa = await _context.Tarifas.FindAsync(id);
            if (tarifa == null) return NotFound();

            tarifa.Activa = false;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Tarifa eliminada.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasAsync(int? idTipoVehiculo = null, string? unidadSeleccionada = null)
        {
            ViewBag.TiposVehiculo = new SelectList(
                await _context.TipoVehiculos.Where(t => t.Activo).OrderBy(t => t.Descripcion).ToListAsync(),
                "Id", "Descripcion", idTipoVehiculo);

            ViewBag.UnidadesTiempo = new SelectList(UnidadesTiempo, unidadSeleccionada);
        }
    }
}