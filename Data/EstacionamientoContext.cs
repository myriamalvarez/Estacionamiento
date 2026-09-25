using Microsoft.EntityFrameworkCore;
using Estacionamiento.Models;

namespace Estacionamiento.Data
{
    public class EstacionamientoContext : DbContext
    {
        public EstacionamientoContext(DbContextOptions<EstacionamientoContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<TipoVehiculo> TipoVehiculos { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Cochera> Cocheras { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<Abono> Abonos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Estadia> Estadias { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Forzar los nombres exactos de las tablas en minúsculas como estan en MySQL/XAMPP
            modelBuilder.Entity<Cliente>().ToTable("cliente");
            modelBuilder.Entity<TipoVehiculo>().ToTable("tipo_vehiculo");
            modelBuilder.Entity<Vehiculo>().ToTable("vehiculo");
            modelBuilder.Entity<Cochera>().ToTable("cochera");
            modelBuilder.Entity<Tarifa>().ToTable("tarifa");
            modelBuilder.Entity<Abono>().ToTable("abono");
            modelBuilder.Entity<Usuario>().ToTable("usuario");
            modelBuilder.Entity<Estadia>().ToTable("estadia");
            modelBuilder.Entity<Pago>().ToTable("pago");
        }
    }
}