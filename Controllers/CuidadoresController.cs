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
        var cuidadores = await _db.Cuidadores
            .OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToListAsync();

        return Ok(cuidadores);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe
    [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
        {
            
            return BadRequest();
        }

        var cuidador = await _db.Cuidadores.FindAsync(id);
        if (cuidador == null)
        {
            return NotFound();
        }

        return Ok(cuidador);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
    {
        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"
        cuidador.Nombre = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cuidador.Nombre.Trim());
        cuidador.Turno = cuidador.Turno.Trim();

        if (string.IsNullOrWhiteSpace(cuidador.Turno) || 
            (cuidador.Turno != "Mañana" && cuidador.Turno != "Tarde" && cuidador.Turno != "Noche"))
        {
            return BadRequest("El turno debe ser 'Mañana', 'Tarde' o 'Noche'.");
        }
        
        // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict
        var existeDuplicado = await _db.Cuidadores.AnyAsync(c => c.Nombre.ToLower() == cuidador.Nombre.ToLower() && c.Turno == cuidador.Turno);
        if (existeDuplicado)
        {
            return Conflict("Ya existe un cuidador con el mismo nombre y turno.");
        }


        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);
    }

    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Cuidador cuidadorActualizado)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser un número positivo.");

        var cuidadorExistente = await _db.Cuidadores.FindAsync(id);
        if (cuidadorExistente == null)
            return NotFound();

        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"
        cuidadorActualizado.Nombre = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cuidadorActualizado.Nombre.Trim());
        cuidadorActualizado.Turno = cuidadorActualizado.Turno.Trim();

        if (string.IsNullOrWhiteSpace(cuidadorActualizado.Turno) || 
            (cuidadorActualizado.Turno != "Mañana" && cuidadorActualizado.Turno != "Tarde" && cuidadorActualizado.Turno != "Noche"))
        {
            return BadRequest("El turno debe ser 'Mañana', 'Tarde' o 'Noche'.");
        }

        // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict
        var existeDuplicado = await _db.Cuidadores.AnyAsync(c => c.Nombre.ToLower() == cuidadorActualizado.Nombre.ToLower() && c.Turno == cuidadorActualizado.Turno && c.Id != id);
        if (existeDuplicado)
        {
            return Conflict("Ya existe un cuidador con el mismo nombre y turno.");
        }

        if (string.IsNullOrWhiteSpace(cuidadorActualizado.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        cuidadorExistente.Nombre = cuidadorActualizado.Nombre;
        cuidadorExistente.Turno = cuidadorActualizado.Turno;

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

        var tieneMascotas = await _db.Mascotas.AnyAsync(m => m.CuidadorId == id);
        if (tieneMascotas)
            return Conflict("No se puede eliminar un cuidador que tiene mascotas asignadas.");

        _db.Cuidadores.Remove(cuidador);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe
    [HttpGet("{id}/mascotas")]
    public async Task<IActionResult> GetMascotasPorCuidador(int id)
    {
        var cuidador = await _db.Cuidadores.FindAsync(id);
        if (cuidador == null)
            return NotFound();

        var mascotas = await _db.Mascotas
            .Where(m => m.CuidadorId == id)
            .OrderBy(m => m.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToListAsync();

        return Ok(mascotas);
    }
}
