using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
{
    private readonly RefugioDbContext _db;

    public CuidadoresController(RefugioDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cuidadores = await _db.Cuidadores.ToListAsync();

        var catalogo = cuidadores
            .OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return Ok(catalogo);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cuidadores = await _db.Cuidadores.FindAsync(id);
        if (cuidadores == null)
            return NotFound();

        return Ok(cuidadores);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
    {
        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"
        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();   

        // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict
        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);
    }

    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)}
      public async Task<IActionResult> Update(int id, Cuidador cuidador)
    {
        if (id != cuidador.Id)
            return BadRequest("El id del cuidador no coincide con el id del cuerpo.");

        var existente = await _db.Cuidadores.FindAsync(id);
        if (existente == null)
            return NotFound();

        existente.Nombre = cuidador.Nombre;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas asignadas
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cuidador = await _db.Cuidadores.FindAsync(id);
        if (cuidador == null)
            return NotFound();

        _db.Cuidadores.Remove(cuidador);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe

