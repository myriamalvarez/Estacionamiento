using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Controllers
{
    public class EstadiaController : ControladorBase
    {
        private readonly EstacionamientoContext _context;

        public EstadiaController(EstacionamientoContext context)
        {
            _context = context;
        }

        // "Qué vehículos se encuentran actualmente dentro del establecimiento" (narrativa).
        // Cascarón Vue; los datos llegan paginados desde ActivasJson.
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> ActivasJson(int pagina = 1, int tamanioPagina = 10, string? term = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1) tamanioPagina = 10;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var consulta = _context.Estadias.Where(e => e.Estado);
            if (!string.IsNullOrWhiteSpace(term))
                consulta = consulta.Where(e => e.Vehiculo!.Patente.Contains(term));

            var total = await consulta.CountAsync();
            var datos = await consulta
                .OrderBy(e => e.FechaHoraIngreso)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(e => new
                {
                    e.Id,
                    Patente = e.Vehiculo!.Patente,
                    Vehiculo = e.Vehiculo.Marca + " " + e.Vehiculo.Modelo,
                    Cochera = e.Cochera!.Numero,
                    e.FechaHoraIngreso
                })
                .ToListAsync();

            var ahora = DateTime.Now;
            var items = datos.Select(e =>
            {
                var perm = ahora - e.FechaHoraIngreso;
                return new
                {
                    e.Id, e.Patente, e.Vehiculo, e.Cochera,
                    Ingreso = e.FechaHoraIngreso.ToString("dd/MM/yyyy HH:mm"),
                    Permanencia = $"{(int)perm.TotalHours} h {perm.Minutes} min"
                };
            }).ToList();

            return Json(new
            {
                items, total, pagina, tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }

        // Cascarón Vue; los datos llegan paginados desde HistorialJson.
        public IActionResult Historial() => View();

        [HttpGet]
        public async Task<IActionResult> HistorialJson(int pagina = 1, int tamanioPagina = 10, string? term = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1) tamanioPagina = 10;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var consulta = _context.Estadias.AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
                consulta = consulta.Where(e => e.Vehiculo!.Patente.Contains(term));

            var total = await consulta.CountAsync();

            var items = await consulta
                .OrderByDescending(e => e.FechaHoraIngreso)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(e => new
                {
                    e.Id,
                    Patente = e.Vehiculo!.Patente,
                    Cochera = e.Cochera!.Numero,
                    Ingreso = e.FechaHoraIngreso.ToString("dd/MM/yyyy HH:mm"),
                    Egreso = e.FechaHoraEgreso != null ? e.FechaHoraEgreso.Value.ToString("dd/MM/yyyy HH:mm") : null,
                    Activa = e.Estado,
                    ConAbono = e.IdAbono != null,
                    Monto = e.Pago != null ? (decimal?)e.Pago.Monto : null
                })
                .ToListAsync();

            return Json(new
            {
                items, total, pagina, tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var estadia = await _context.Estadias
                .Include(e => e.Vehiculo).ThenInclude(v => v!.Cliente)
                .Include(e => e.Cochera)
                .Include(e => e.Tarifa)
                .Include(e => e.Abono)
                .Include(e => e.Usuario)
                .Include(e => e.Pago)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (estadia == null) return NotFound();

            // Si la estadía terminó y todavía no se cobró, se calcula el importe para ofrecer "Registrar pago".
            if (!estadia.Estado && (estadia.Pago == null || !estadia.Pago.Estado) && estadia.FechaHoraEgreso != null)
            {
                if (estadia.IdAbono != null)
                    ViewBag.ImportePendiente = 0m;
                else if (estadia.Tarifa != null)
                {
                    var perm = estadia.FechaHoraEgreso.Value - estadia.FechaHoraIngreso;
                    var unidades = estadia.Tarifa.UnidadTiempo == "Hora"
                        ? Math.Max(1, (int)Math.Ceiling(perm.TotalHours))
                        : (int)Math.Ceiling(perm.TotalDays);
                    ViewBag.ImportePendiente = estadia.Tarifa.Precio * unidades;
                }
            }
            return View(estadia);
        }

        // ------------------------------------------------------------------
        // Ingreso
        // ------------------------------------------------------------------

        public IActionResult Ingreso() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ingreso(int idVehiculo)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(idVehiculo);
            if (vehiculo == null || !vehiculo.Activo)
            {
                ModelState.AddModelError(string.Empty, "El vehículo no existe o no está activo.");
                return View();
            }

            // Regla de la narrativa: no se permite registrar el ingreso si no hay una
            // cochera disponible compatible con el tipo de vehículo.
            var cochera = await _context.Cocheras
                .Where(c => c.Activo && c.Estado == "Libre" && c.IdTipoVehiculo == vehiculo.IdTipoVehiculo)
                .FirstOrDefaultAsync();

            if (cochera == null)
            {
                ModelState.AddModelError(string.Empty, "No hay cocheras disponibles para este tipo de vehículo.");
                return View();
            }

            // Si el vehículo tiene un abono vigente (activo, no cancelado, y dentro del
            // período contratado), la estadía queda cubierta por ese abono desde el ingreso.
            var hoy = DateTime.Today;
            var abonoVigente = await _context.Abonos
                .Where(a => a.IdVehiculo == idVehiculo && a.Activo && a.Estado
                         && a.FechaInicio <= hoy && a.FechaFin >= hoy)
                .FirstOrDefaultAsync();

            var estadia = new Estadia
            {
                IdVehiculo = idVehiculo,
                IdCochera = cochera.Id,
                IdAbono = abonoVigente?.Id,
                IdUsuario = UsuarioActualId!.Value, // quien registra es el usuario logueado
                FechaHoraIngreso = DateTime.Now,
                Estado = true
            };

            cochera.Estado = "Ocupada";

            _context.Estadias.Add(estadia);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Ingreso registrado. Cochera asignada: {cochera.Numero}"
                + (abonoVigente != null ? " (cubierta por abono vigente)." : ".");
            return RedirectToAction(nameof(Details), new { id = estadia.Id });
        }

        // ------------------------------------------------------------------
        // Egreso
        // ------------------------------------------------------------------

        public async Task<IActionResult> Egreso(int id)
        {
            var estadia = await _context.Estadias
                .Include(e => e.Vehiculo)
                .Include(e => e.Cochera)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (estadia == null) return NotFound();

            // Regla de la narrativa: no se permite registrar el egreso de un vehículo
            // que no tenga una estadía activa.
            if (!estadia.Estado)
            {
                TempData["Error"] = "Esta estadía ya fue finalizada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(estadia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Egreso(int id, bool confirmar)
        {
            var estadia = await _context.Estadias
                .Include(e => e.Vehiculo)
                .Include(e => e.Cochera)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (estadia == null) return NotFound();

            if (!estadia.Estado)
            {
                TempData["Error"] = "Esta estadía ya fue finalizada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            estadia.FechaHoraEgreso = DateTime.Now;
            decimal importe;

            if (estadia.IdAbono.HasValue)
            {
                // Cubierta por abono: no se aplica ninguna tarifa adicional (narrativa).
                importe = 0m;
            }
            else
            {
                var permanencia = estadia.FechaHoraEgreso.Value - estadia.FechaHoraIngreso;
                var vehiculo = estadia.Vehiculo!;

                // Regla de cálculo: hasta 24 horas se cobra por hora (redondeando hacia
                // arriba, mínimo 1 hora); más de 24 horas se cobra por día (ídem).
                var unidadElegida = permanencia.TotalHours <= 24 ? "Hora" : "Dia";
                var cantidadUnidades = unidadElegida == "Hora"
                    ? Math.Max(1, (int)Math.Ceiling(permanencia.TotalHours))
                    : (int)Math.Ceiling(permanencia.TotalDays);

                var tarifa = await _context.Tarifas
                    .FirstOrDefaultAsync(t => t.Activa
                                            && t.IdTipoVehiculo == vehiculo.IdTipoVehiculo
                                            && t.UnidadTiempo == unidadElegida);

                if (tarifa == null)
                {
                    TempData["Error"] = $"No hay una tarifa activa configurada para este tipo de vehículo ({unidadElegida}).";
                    return RedirectToAction(nameof(Egreso), new { id });
                }

                estadia.IdTarifa = tarifa.Id;
                importe = tarifa.Precio * cantidadUnidades;
            }

            estadia.Estado = false; // finalizada
            estadia.Cochera!.Estado = "Libre";

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Egreso registrado. Importe a cobrar: {importe:C}.";
            return RedirectToAction("Create", "Pago", new { idEstadia = id, importe });
        }
    }
}