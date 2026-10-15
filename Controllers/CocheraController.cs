using Microsoft.AspNetCore.Authorization;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Estacionamiento.Controllers
{
    public class CocheraController : Controller
    {
        private readonly EstacionamientoContext _context;

        public CocheraController(EstacionamientoContext context)
        {
            _context = context;
        }

        // Cascarón Vue; los datos llegan paginados desde ListarJson.
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> ListarJson(int pagina = 1, int tamanioPagina = 10, string? term = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1) tamanioPagina = 10;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var consulta = _context.Cocheras.Where(c => c.Activo);
            if (!string.IsNullOrWhiteSpace(term))
                consulta = consulta.Where(c => c.Numero.Contains(term));

            var total = await consulta.CountAsync();
            var items = await consulta
                .OrderBy(c => c.Numero)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(c => new { c.Id, c.Numero, Tipo = c.TipoVehiculo!.Descripcion, c.Estado })
                .ToListAsync();

            return Json(new
            {
                items, total, pagina, tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }

        // La narrativa: "los empleados pueden consultar qué cocheras están ocupadas y cuáles libres".
        public async Task<IActionResult> Disponibilidad()
        {
            var cocheras = await _context.Cocheras
                .Include(c => c.TipoVehiculo)
                .Where(c => c.Activo)
                .OrderBy(c => c.Numero)
                .ToListAsync();
            ViewBag.Libres = cocheras.Count(c => c.Estado == "Libre");
            ViewBag.Ocupadas = cocheras.Count(c => c.Estado == "Ocupada");
            return View(cocheras);
        }

        public async Task<IActionResult> Details(int id)
        {
            var cochera = await _context.Cocheras
                .Include(c => c.TipoVehiculo)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cochera == null) return NotFound();
            return View(cochera);
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Create()
        {
            await CargarTiposAsync();
            return View(new Cochera());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Create(Cochera cochera)
        {
            if (!ModelState.IsValid)
            {
                await CargarTiposAsync(cochera.IdTipoVehiculo);
                return View(cochera);
            }

            try
            {
                _context.Cocheras.Add(cochera);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cochera creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(Cochera.Numero), "Ya existe una cochera con ese número.");
                await CargarTiposAsync(cochera.IdTipoVehiculo);
                return View(cochera);
            }
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Edit(int id)
        {
            var cochera = await _context.Cocheras.FindAsync(id);
            if (cochera == null) return NotFound();
            await CargarTiposAsync(cochera.IdTipoVehiculo);
            return View(cochera);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Edit(int id, Cochera cochera)
        {
            if (id != cochera.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await CargarTiposAsync(cochera.IdTipoVehiculo);
                return View(cochera);
            }

            try
            {
                _context.Update(cochera);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cochera actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(Cochera.Numero), "Ya existe otra cochera con ese número.");
                await CargarTiposAsync(cochera.IdTipoVehiculo);
                return View(cochera);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cochera = await _context.Cocheras.FindAsync(id);
            if (cochera == null) return NotFound();

            cochera.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Cochera eliminada.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarTiposAsync(int? idTipoVehiculo = null)
        {
            ViewBag.TiposVehiculo = new SelectList(
                await _context.TipoVehiculos.Where(t => t.Activo).OrderBy(t => t.Descripcion).ToListAsync(),
                "Id", "Descripcion", idTipoVehiculo);
        }
    }
}