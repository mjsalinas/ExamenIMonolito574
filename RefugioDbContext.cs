using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;

namespace RefugioMascotas;

public class RefugioDbContext : DbContext
{
    public RefugioDbContext(DbContextOptions<RefugioDbContext> options) : base(options) { }

    public DbSet<Cuidador> Cuidadores => Set<Cuidador>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mascota>()
            .HasOne(m => m.Cuidador)
            .WithMany(c => c.Mascotas)
            .HasForeignKey(m => m.CuidadorId);

        modelBuilder.Entity<Cuidador>().HasData(
            new Cuidador { Id = 1, Nombre = "Maria Lopez", Turno = "Mañana" },
            new Cuidador { Id = 2, Nombre = "Carlos Perez", Turno = "Tarde" }
        );

        modelBuilder.Entity<Mascota>().HasData(
            new Mascota { Id = 1, Nombre = "Toby", Especie = "Perro", Edad = 5, EnTratamiento = false, CuidadorId = 1 },
            new Mascota { Id = 2, Nombre = "Michi", Especie = "Gato", Edad = 3, EnTratamiento = true, CuidadorId = 2 }
        );
    }
}
