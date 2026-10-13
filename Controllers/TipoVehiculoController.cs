using Microsoft.AspNetCore.Authorization;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Estacionamiento.Controllers
{
    [Authorize(Policy = "Administrador")]
    public class TipoVehiculoController : Controller
    {
        private readonly EstacionamientoContext _context;

        public TipoVehiculoController(EstacionamientoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tipos = await _context.TipoVehiculos
                .OrderBy(t => t.Descripcion)
                .ToListAsync();
            return View(tipos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tipo = await _context.TipoVehiculos.FindAsync(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        public IActionResult Create() => View(new TipoVehiculo());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TipoVehiculo tipo)
        {
            if (!ModelState.IsValid) return View(tipo);

            try
            {
                _context.TipoVehiculos.Add(tipo);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Tipo de vehículo creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(TipoVehiculo.Descripcion), "Ya existe un tipo de vehículo con esa descripción.");
                return View(tipo);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tipo = await _context.TipoVehiculos.FindAsync(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TipoVehiculo tipo)
        {
            if (id != tipo.Id) return BadRequest();
            if (!ModelState.IsValid) return View(tipo);

            try
            {
                _context.Update(tipo);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Tipo de vehículo actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(TipoVehiculo.Descripcion), "Ya existe otro tipo de vehículo con esa descripción.");
                return View(tipo);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var tipo = await _context.TipoVehiculos.FindAsync(id);
            if (tipo == null) return NotFound();

            tipo.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Tipo de vehículo eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}