using Estacionamiento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Controllers
{
    // Informe de recaudación: solo administradores. Es la contraparte web del endpoint
    // GET /api/estacionamiento/recaudacion (misma regla: solo cuentan los pagos no anulados).
    [Authorize(Policy = "Administrador")]
    public class RecaudacionController : ControladorBase
    {
        private readonly EstacionamientoContext _context;

        public RecaudacionController(EstacionamientoContext context)
        {
            _context = context;
        }

        // Cascarón Vue: elige la fecha y trae los datos por Ajax.
        public IActionResult Index() => View();

        // Total del día y desglose por método de pago.
        [HttpGet]
        public async Task<IActionResult> ResumenJson(DateTime? fecha = null)
        {
            var dia = (fecha ?? DateTime.Today).Date;
            var siguiente = dia.AddDays(1);

            var porMetodo = await _context.Pagos
                .Where(p => p.Estado && p.FechaHoraPago >= dia && p.FechaHoraPago < siguiente)
                .GroupBy(p => p.MetodoPago)
                .Select(g => new { metodo = g.Key, cantidad = g.Count(), total = g.Sum(p => p.Monto) })
                .OrderByDescending(x => x.total)
                .ToListAsync();

            var anulados = await _context.Pagos
                .CountAsync(p => !p.Estado && p.FechaHoraPago >= dia && p.FechaHoraPago < siguiente);

            return Json(new
            {
                fecha = dia.ToString("yyyy-MM-dd"),
                totalGeneral = porMetodo.Sum(x => x.total),
                cantidadPagos = porMetodo.Sum(x => x.cantidad),
                anulados,
                porMetodo
            });
        }

        // Detalle de los pagos del día, paginado en el servidor.
        [HttpGet]
        public async Task<IActionResult> PagosJson(DateTime? fecha = null, int pagina = 1, int tamanioPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1) tamanioPagina = 10;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var dia = (fecha ?? DateTime.Today).Date;
            var siguiente = dia.AddDays(1);

            var consulta = _context.Pagos.Where(p => p.FechaHoraPago >= dia && p.FechaHoraPago < siguiente);
            var total = await consulta.CountAsync();

            var datos = await consulta
                .OrderByDescending(p => p.FechaHoraPago)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(p => new
                {
                    p.Id,
                    p.IdEstadia,
                    Patente = p.Estadia!.Vehiculo!.Patente,
                    p.FechaHoraPago,
                    p.Monto,
                    p.MetodoPago,
                    p.Estado,
                    CobradoPor = p.UsuarioRegistro!.Nombre + " " + p.UsuarioRegistro.Apellido
                })
                .ToListAsync();

            var items = datos.Select(p => new
            {
                p.Id, p.IdEstadia, p.Patente,
                Hora = p.FechaHoraPago.ToString("HH:mm"),
                p.Monto, p.MetodoPago, p.Estado, p.CobradoPor
            });

            return Json(new
            {
                items, total, pagina, tamanioPagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamanioPagina)
            });
        }
    }
}