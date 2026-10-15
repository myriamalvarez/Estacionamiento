using Microsoft.AspNetCore.Authorization;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Estacionamiento.Controllers
{
    public class ClienteController : Controller
    {
        private readonly EstacionamientoContext _context;

        public ClienteController(EstacionamientoContext context)
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

            var consulta = _context.Clientes.Where(c => c.Activo);
            if (!string.IsNullOrWhiteSpace(term))
                consulta = consulta.Where(c => c.Dni.Contains(term) || c.Apellido.Contains(term) || c.Nombre.Contains(term));

            var total = await consulta.CountAsync();
            var items = await consulta
                .OrderBy(c => c.Apellido).ThenBy(c => c.Nombre)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(c => new { c.Id, c.Nombre, c.Apellido, c.Dni, c.Telefono, c.Email })
                .ToListAsync();

            return Json(new
            {
                items, total, pagina, tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Vehiculos)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        public IActionResult Create() => View(new Cliente());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (!ModelState.IsValid) return View(cliente);

            try
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cliente registrado correctamente.";
                return RedirectToAction(nameof(Details), new { id = cliente.Id });
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(Cliente.Dni), "Ya existe un cliente con ese DNI.");
                return View(cliente);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.Id) return BadRequest();
            if (!ModelState.IsValid) return View(cliente);

            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cliente actualizado correctamente.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                ModelState.AddModelError(nameof(Cliente.Dni), "Ya existe otro cliente con ese DNI.");
                return View(cliente);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            cliente.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Cliente eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // Búsqueda vía Ajax (requisito de la consigna): buscar cliente por DNI o apellido
        // sin traer todos los clientes — útil al asignar cliente en Vehiculo/Abono.
        [HttpGet]
        public async Task<IActionResult> Buscar(string term)
        {
            term ??= string.Empty;
            var clientes = await _context.Clientes
                .Where(c => c.Activo && (c.Dni.Contains(term) || c.Apellido.Contains(term)))
                .OrderBy(c => c.Apellido)
                .Take(10)
                .Select(c => new { c.Id, c.Nombre, c.Apellido, c.Dni })
                .ToListAsync();
            return Json(clientes);
        }
    }
}