using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly RefugioDbContext _db;

    public MascotasController(RefugioDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mascotas = await _db.Mascotas
            .Include(m => m.Cuidador)
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return Ok(mascotas);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe

    [HttpPost]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var mascota = await _db.Mascotas
            .Include(m => m.Cuidador)
            .FirstOrDefaultAsync(m => m.Id == id);

        // Un voluntario reportó que esta validación de edad se comporta raro (Ticket 0)
        if (mascota is null)
            return NotFound();

        var cuidadorExiste = await _db.Cuidadores.AnyAsync(c => c.Id == mascota.CuidadorId);
        if (!cuidadorExiste)
            return BadRequest("El cuidador especificado no existe.");

        // TODO (Ticket 2): normalizar texto y validar formato de Nombre/Especie

        // TODO (Ticket 3): validar duplicado (Nombre + CuidadorId) -> 409 Conflict

        _db.Mascotas.Add(mascota);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = mascota.Id }, mascota);
    }

    // TODO (Ticket 4): Update(int id, Mascota mascotaActualizada)

    // TODO (Ticket 5): Delete(int id) -> 409 si EnTratamiento es true
}