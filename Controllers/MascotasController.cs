using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly RefugioDbContext _context;

    public MascotasController(RefugioDbContext db) => _context = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mascotas = await _context.Mascotas
            .Include(m => m.Cuidador)
            .OrderBy(m => m.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToListAsync();

        return Ok(mascotas);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    if (id <= 0)
        return BadRequest("El id debe ser mayor que 0.");

    var mascota = await _context.Mascotas
        .FirstOrDefaultAsync(m => m.Id == id);

    if (mascota is null)
        return NotFound();

    return Ok(mascota);
}

    [HttpPost]
    public async Task<IActionResult> Create(Mascota mascota)
    {
        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            return BadRequest("El nombre de la mascota es obligatorio.");

        if (string.IsNullOrWhiteSpace(mascota.Especie))
            return BadRequest("La especie es obligatoria.");

        // Un voluntario reportó que esta validación de edad se comporta raro (Ticket 0)
        if (mascota.Edad < 0 || mascota.Edad > 30) 
      {
        return BadRequest("La edad debe estar entre 0 y 30 años.");
      }

        var cuidadorExiste = await _context.Cuidadores.AnyAsync(c => c.Id == mascota.CuidadorId);
        if (!cuidadorExiste)
            return BadRequest("El cuidador especificado no existe.");

        // TODO (Ticket 2): normalizar texto y validar formato de Nombre/Especie

        // TODO (Ticket 3): validar duplicado (Nombre + CuidadorId) -> 409 Conflict

        _context.Mascotas.Add(mascota);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = mascota.Id }, mascota);
    }

    // TODO (Ticket 4): Update(int id, Mascota mascotaActualizada)

    // TODO (Ticket 5): Delete(int id) -> 409 si EnTratamiento es true
}
