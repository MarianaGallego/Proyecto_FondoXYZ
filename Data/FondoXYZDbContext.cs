using FondoXYZ.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FondoXYZ.Data;

public class FondoXYZDbContext : DbContext
{
    public FondoXYZDbContext(DbContextOptions<FondoXYZDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sitio> Sitios => Set<Sitio>();
    public DbSet<Region> Regiones => Set<Region>();
    public DbSet<ServicioSitio> ServiciosSitio => Set<ServicioSitio>();
    public DbSet<BloqueAlojamiento> BloquesAlojamiento => Set<BloqueAlojamiento>();
    public DbSet<UnidadAlojamiento> UnidadesAlojamiento => Set<UnidadAlojamiento>();
    public DbSet<Asociado> Asociados => Set<Asociado>();
    public DbSet<TipoServicio> TiposServicio => Set<TipoServicio>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaUnidad> ReservaUnidades => Set<ReservaUnidad>();
    public DbSet<ReservaAcompanante> ReservaAcompanantes => Set<ReservaAcompanante>();
    public DbSet<ReservaServicio> ReservaServicios => Set<ReservaServicio>();
    public DbSet<AuditoriaTarifa> AuditoriaTarifas => Set<AuditoriaTarifa>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Region>(entity =>
        {
            entity.ToTable("Region");
            entity.HasKey(e => e.RegionId);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Sitio>(entity =>
        {
            entity.ToTable("Sitio");
            entity.HasKey(e => e.SitioId);
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.TipoSitio).HasMaxLength(20);
            entity.Property(e => e.Ciudad).HasMaxLength(100);
            entity.Property(e => e.Ubicacion).HasMaxLength(255);

            entity.HasOne(e => e.Region)
                .WithMany(r => r.Sitios)
                .HasForeignKey(e => e.RegionId);
        });

        modelBuilder.Entity<ServicioSitio>(entity =>
        {
            entity.ToTable("ServicioSitio");
            entity.HasKey(e => e.ServicioSitioId);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Categoria).HasMaxLength(50);

            entity.HasOne(e => e.Sitio)
                .WithMany(s => s.Servicios)
                .HasForeignKey(e => e.SitioId);
        });

        modelBuilder.Entity<BloqueAlojamiento>(entity =>
        {
            entity.ToTable("BloqueAlojamiento");
            entity.HasKey(e => e.BloqueAlojamientoId);
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(e => e.Sitio)
                .WithMany(s => s.BloquesAlojamiento)
                .HasForeignKey(e => e.SitioId);
        });

        modelBuilder.Entity<UnidadAlojamiento>(entity =>
        {
            entity.ToTable("UnidadAlojamiento");
            entity.HasKey(e => e.UnidadAlojamientoId);
            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(e => e.Sitio)
                .WithMany(s => s.UnidadesAlojamiento)
                .HasForeignKey(e => e.SitioId);

            entity.HasOne(e => e.Bloque)
                .WithMany(b => b.Unidades)
                .HasForeignKey(e => e.BloqueAlojamientoId);
        });

        modelBuilder.Entity<Asociado>(entity =>
        {
            entity.ToTable("Asociado");
            entity.HasKey(e => e.AsociadoId);
            entity.Property(e => e.NumeroAsociado).HasMaxLength(20);
            entity.Property(e => e.TipoDocumento).HasMaxLength(10);
            entity.Property(e => e.NumeroDocumento).HasMaxLength(20);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Clave).HasMaxLength(255);
        });

        modelBuilder.Entity<TipoServicio>(entity =>
        {
            entity.ToTable("TipoServicio");
            entity.HasKey(e => e.TipoServicioId);
            entity.Property(e => e.Codigo).HasMaxLength(30);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
        });

        modelBuilder.Entity<Tarifa>(entity =>
        {
            entity.ToTable("Tarifa");
            entity.HasKey(e => e.TarifaId);
            entity.Property(e => e.TipoConcepto).HasMaxLength(30);
            entity.Property(e => e.Precio).HasPrecision(12, 2);
            entity.Property(e => e.PrecioPersonaAdicional).HasPrecision(12, 2);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("Reserva");
            entity.HasKey(e => e.ReservaId);
            entity.Property(e => e.CodigoReserva).HasMaxLength(20);
            entity.Property(e => e.TipoReserva).HasMaxLength(20);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Subtotal).HasPrecision(12, 2);
            entity.Property(e => e.TotalServicios).HasPrecision(12, 2);
            entity.Property(e => e.Total).HasPrecision(12, 2);

            entity.HasOne(e => e.Asociado)
                .WithMany(a => a.Reservas)
                .HasForeignKey(e => e.AsociadoId);

            entity.HasOne(e => e.Sitio)
                .WithMany()
                .HasForeignKey(e => e.SitioId);
        });

        modelBuilder.Entity<ReservaUnidad>(entity =>
        {
            entity.ToTable("ReservaUnidad");
            entity.HasKey(e => e.ReservaUnidadId);
            entity.Property(e => e.PrecioNoche).HasPrecision(12, 2);
            entity.Property(e => e.Subtotal).HasPrecision(12, 2);

            entity.HasOne(e => e.Reserva)
                .WithMany(r => r.Unidades)
                .HasForeignKey(e => e.ReservaId);

            entity.HasOne(e => e.UnidadAlojamiento)
                .WithMany()
                .HasForeignKey(e => e.UnidadAlojamientoId);
        });

        modelBuilder.Entity<ReservaAcompanante>(entity =>
        {
            entity.ToTable("ReservaAcompanante");
            entity.HasKey(e => e.ReservaAcompananteId);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.TipoDocumento).HasMaxLength(10);
            entity.Property(e => e.NumeroDocumento).HasMaxLength(20);
            entity.Property(e => e.TarifaAplicada).HasPrecision(12, 2);

            entity.HasOne(e => e.Reserva)
                .WithMany(r => r.Acompanantes)
                .HasForeignKey(e => e.ReservaId);
        });

        modelBuilder.Entity<ReservaServicio>(entity =>
        {
            entity.ToTable("ReservaServicio");
            entity.HasKey(e => e.ReservaServicioId);
            entity.Property(e => e.PrecioUnitario).HasPrecision(12, 2);
            entity.Property(e => e.Subtotal).HasPrecision(12, 2);

            entity.HasOne(e => e.Reserva)
                .WithMany(r => r.Servicios)
                .HasForeignKey(e => e.ReservaId);

            entity.HasOne(e => e.TipoServicio)
                .WithMany()
                .HasForeignKey(e => e.TipoServicioId);
        });

        modelBuilder.Entity<AuditoriaTarifa>(entity =>
        {
            entity.ToTable("AuditoriaTarifa");
            entity.HasKey(e => e.AuditoriaTarifaId);
            entity.Property(e => e.Concepto).HasMaxLength(100);
            entity.Property(e => e.ValorUnitario).HasPrecision(12, 2);
            entity.Property(e => e.Subtotal).HasPrecision(12, 2);

            entity.HasOne(e => e.Reserva)
                .WithMany(r => r.AuditoriaTarifas)
                .HasForeignKey(e => e.ReservaId);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("Pago");
            entity.HasKey(e => e.PagoId);
            entity.Property(e => e.Monto).HasPrecision(12, 2);
            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.Estado).HasMaxLength(20);

            entity.HasOne(e => e.Reserva)
                .WithMany(r => r.Pagos)
                .HasForeignKey(e => e.ReservaId);
        });
    }
}
