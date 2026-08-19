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

    [HttpGet("id")]
    public async Task<IActionResult> GetAll(int id)
    {
        if (id <= 0) return BadRequest("El ID debe ser mayor a cero.");
        var mascotas = await _db.Mascotas
            .Include(m => m.Cuidador)
            .FirstOrDefaultAsync(m => m.Id == id);
            //.OrderBy(m => m.Nombre, StringComparer.CurrentCultureIgnoreCase)
            //.ToListAsync();
        if (mascotas == null) return NotFound($"No se encontró la mascota con ID {id}.");
        return Ok(mascotas);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe

    [HttpPost]
    public async Task<IActionResult> Create(Mascota mascota)
    {
        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            return BadRequest("El nombre de la mascota es obligatorio.");

        if (string.IsNullOrWhiteSpace(mascota.Especie))
            return BadRequest("La especie es obligatoria.");

        // Un voluntario reportó que esta validación de edad se comporta raro (Ticket 0)
        if (mascota.Edad < 0 || mascota.Edad > 30)//se corrige operador logico
            return BadRequest("La edad debe estar entre 0 y 30 años.");

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
