using Estacionamiento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Api
{
    // Web API REST protegida con JWT (Authorization: Bearer <token>).
    [ApiController]
    [Route("api/estacionamiento")]
    [Authorize]
    public class EstacionamientoController : ControllerBase
    {
        private readonly EstacionamientoContext _context;

        public EstacionamientoController(EstacionamientoContext context)
        {
            _context = context;
        }

        // Cantidad de cocheras libres y ocupadas, total y por tipo de vehículo.
        [HttpGet("disponibilidad")]
        public async Task<IActionResult> Disponibilidad()
        {
            var porTipo = await _context.Cocheras
                .Where(c => c.Activo)
                .GroupBy(c => c.TipoVehiculo!.Descripcion)
                .Select(g => new
                {
                    tipo = g.Key,
                    libres = g.Count(c => c.Estado == "Libre"),
                    ocupadas = g.Count(c => c.Estado == "Ocupada")
                })
                .ToListAsync();

            return Ok(new
            {
                libres = porTipo.Sum(t => t.libres),
                ocupadas = porTipo.Sum(t => t.ocupadas),
                porTipo
            });
        }

        // Vehículos que están adentro ahora, paginado.
        [HttpGet("estadias-activas")]
        public async Task<IActionResult> EstadiasActivas([FromQuery] int pagina = 1, [FromQuery] int tamanioPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamanioPagina < 1 || tamanioPagina > 50) tamanioPagina = 10;

            var consulta = _context.Estadias.Where(e => e.Estado);
            var total = await consulta.CountAsync();

            var items = await consulta
                .OrderBy(e => e.FechaHoraIngreso)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(e => new
                {
                    e.Id,
                    patente = e.Vehiculo!.Patente,
                    cochera = e.Cochera!.Numero,
                    ingreso = e.FechaHoraIngreso,
                    cubiertaPorAbono = e.IdAbono != null
                })
                .ToListAsync();

            return Ok(new { items, total, pagina, tamanioPagina });
        }

        // Dada una patente que está adentro, devuelve el tiempo transcurrido y el importe acumulado hasta ahora.
        [HttpGet("estadia-activa/{patente}")]
        public async Task<IActionResult> EstadiaActiva(string patente)
        {
            patente = patente.Trim().ToUpper();

            var estadia = await _context.Estadias
                .Include(e => e.Vehiculo)
                .Include(e => e.Cochera)
                .FirstOrDefaultAsync(e => e.Estado && e.Vehiculo!.Patente == patente);

            if (estadia == null)
                return NotFound(new { mensaje = $"El vehículo {patente} no tiene una estadía activa." });

            var permanencia = DateTime.Now - estadia.FechaHoraIngreso;
            decimal importe = 0m;
            string cobertura;

            if (estadia.IdAbono != null)
            {
                cobertura = "Abono";
            }
            else
            {
                // Misma regla que el egreso: hasta 24 h por hora (mínimo 1), más de 24 h por día.
                var unidad = permanencia.TotalHours <= 24 ? "Hora" : "Dia";
                var cantidad = unidad == "Hora"
                    ? Math.Max(1, (int)Math.Ceiling(permanencia.TotalHours))
                    : (int)Math.Ceiling(permanencia.TotalDays);

                var tarifa = await _context.Tarifas.FirstOrDefaultAsync(t =>
                    t.Activa && t.IdTipoVehiculo == estadia.Vehiculo!.IdTipoVehiculo && t.UnidadTiempo == unidad);

                importe = tarifa != null ? tarifa.Precio * cantidad : 0m;
                cobertura = tarifa != null ? $"Tarifa por {unidad.ToLower()}" : "Sin tarifa configurada";
            }

            return Ok(new
            {
                patente = estadia.Vehiculo!.Patente,
                cochera = estadia.Cochera!.Numero,
                ingreso = estadia.FechaHoraIngreso,
                horasTranscurridas = Math.Round(permanencia.TotalHours, 2),
                cobertura,
                importeAcumulado = importe
            });
        }

        // Solo administradores: total cobrado en una fecha (por defecto, hoy).
        [Authorize(Roles = "Administrador")]
        [HttpGet("recaudacion")]
        public async Task<IActionResult> Recaudacion([FromQuery] DateTime? fecha = null)
        {
            var dia = (fecha ?? DateTime.Today).Date;
            var siguiente = dia.AddDays(1);

            var pagos = await _context.Pagos
                .Where(p => p.Estado && p.FechaHoraPago >= dia && p.FechaHoraPago < siguiente)
                .GroupBy(p => p.MetodoPago)
                .Select(g => new { metodo = g.Key, cantidad = g.Count(), total = g.Sum(p => p.Monto) })
                .ToListAsync();

            return Ok(new
            {
                fecha = dia.ToString("yyyy-MM-dd"),
                totalGeneral = pagos.Sum(p => p.total),
                porMetodoPago = pagos
            });
        }
    }
}