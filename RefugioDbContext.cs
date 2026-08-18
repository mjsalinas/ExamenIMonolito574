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
    }
}
