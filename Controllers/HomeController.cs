using Estacionamiento.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Controllers
{
    public class HomeController : Controller
    {
        private readonly EstacionamientoContext _context;

        public HomeController(EstacionamientoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CocherasLibres = await _context.Cocheras.CountAsync(c => c.Activo && c.Estado == "Libre");
            ViewBag.CocherasOcupadas = await _context.Cocheras.CountAsync(c => c.Activo && c.Estado == "Ocupada");
            ViewBag.EstadiasActivas = await _context.Estadias.CountAsync(e => e.Estado);
            ViewBag.VehiculosRegistrados = await _context.Vehiculos.CountAsync(v => v.Activo);
            return View();
        }
    }
}