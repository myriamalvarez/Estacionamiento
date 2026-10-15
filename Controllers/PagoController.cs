using Microsoft.AspNetCore.Authorization;
using Estacionamiento.Data;
using Estacionamiento.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento.Controllers
{
    public class PagoController : ControladorBase
    {
        private readonly EstacionamientoContext _context;

        private readonly IWebHostEnvironment _env;

        // Extensiones permitidas para el comprobante digital (PDF o imagen) y tamaño máximo.
        private static readonly string[] ExtensionesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long TamanioMaximo = 5 * 1024 * 1024;

        public PagoController(EstacionamientoContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Se llega acá normalmente desde EstadiaController.Egreso, con el importe ya calculado.
        public async Task<IActionResult> Create(int idEstadia, decimal importe)
        {
            var estadia = await _context.Estadias
                .Include(e => e.Vehiculo)
                .Include(e => e.Pago)
                .FirstOrDefaultAsync(e => e.Id == idEstadia);
            if (estadia == null) return NotFound();

            // Estadia.Pago es una relación 1 a 1: no se puede cargar un segundo pago mientras el actual
            // siga vigente. Si fue anulado, se permite volver a registrarlo (reemplaza al anulado).
            if (estadia.Pago != null && estadia.Pago.Estado)
            {
                TempData["Error"] = "Esta estadía ya tiene un pago registrado.";
                return RedirectToAction("Details", "Estadia", new { id = idEstadia });
            }

            ViewBag.Estadia = estadia;
            return View(new Pago { IdEstadia = idEstadia, Monto = importe });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pago pago, IFormFile? archivoComprobante)
        {
            var existente = await _context.Pagos.FirstOrDefaultAsync(p => p.IdEstadia == pago.IdEstadia);
            if (existente != null && existente.Estado)
            {
                TempData["Error"] = "Esta estadía ya tiene un pago registrado.";
                return RedirectToAction("Details", "Estadia", new { id = pago.IdEstadia });
            }

            // Quien cobra es el usuario logueado (no viaja desde el formulario).
            pago.IdUsuarioRegistro = UsuarioActualId!.Value;
            ModelState.Remove(nameof(Pago.IdUsuarioRegistro));

            if (archivoComprobante != null && archivoComprobante.Length > 0)
            {
                var ext = Path.GetExtension(archivoComprobante.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(ext))
                    ModelState.AddModelError("archivoComprobante", "El comprobante debe ser PDF, JPG o PNG.");
                else if (archivoComprobante.Length > TamanioMaximo)
                    ModelState.AddModelError("archivoComprobante", "El comprobante no puede superar los 5 MB.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Estadia = await _context.Estadias.Include(e => e.Vehiculo).FirstOrDefaultAsync(e => e.Id == pago.IdEstadia);
                return View(pago);
            }

            if (existente != null)
            {
                // El pago anterior estaba anulado: se reutiliza el mismo registro con los datos nuevos
                // (la relación sigue siendo 1 a 1) y se descarta el comprobante del pago anulado.
                existente.Monto = pago.Monto;
                existente.MetodoPago = pago.MetodoPago;
                existente.IdUsuarioRegistro = pago.IdUsuarioRegistro;
                existente.FechaHoraPago = DateTime.Now;
                existente.Estado = true;
                existente.Comprobante = null;
                BorrarComprobantes(existente.Id);
                pago = existente;
            }
            else
            {
                pago.FechaHoraPago = DateTime.Now;
                _context.Pagos.Add(pago);
            }
            await _context.SaveChangesAsync(); // primero se guarda para obtener el Id del pago

            // El archivo se guarda en wwwroot/uploads/comprobantes/comprobante_{id}.ext y en la
            // base queda solo la ruta (mismo criterio que el avatar y las fotos en Reservas).
            if (archivoComprobante != null && archivoComprobante.Length > 0)
            {
                var carpeta = Path.Combine(_env.WebRootPath, "uploads", "comprobantes");
                Directory.CreateDirectory(carpeta);
                var ext = Path.GetExtension(archivoComprobante.FileName).ToLowerInvariant();
                var nombre = $"comprobante_{pago.Id}{ext}";
                using (var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
                    await archivoComprobante.CopyToAsync(stream);

                pago.Comprobante = $"/uploads/comprobantes/{nombre}";
                await _context.SaveChangesAsync();
            }

            TempData["Mensaje"] = "Pago registrado correctamente.";
            return RedirectToAction("Details", "Estadia", new { id = pago.IdEstadia });
        }

        // Borra del disco los comprobantes de un pago (comprobante_{id}.pdf/.jpg/...).
        private void BorrarComprobantes(int idPago)
        {
            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "comprobantes");
            if (!Directory.Exists(carpeta)) return;
            foreach (var archivo in Directory.GetFiles(carpeta, $"comprobante_{idPago}.*"))
                System.IO.File.Delete(archivo);
        }

        // Anular: cambio de estado, el pago sigue visible (mismo criterio que en Reservas Temporales).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Anular(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null) return NotFound();

            pago.Estado = false;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pago anulado.";
            return RedirectToAction("Details", "Estadia", new { id = pago.IdEstadia });
        }
    }
}