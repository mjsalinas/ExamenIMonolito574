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

// Ticket 1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var cuidador = await _db.Cuidadores
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cuidador is null)
            return NotFound($"No se encontró el cuidador con id {id}.");

        return Ok(cuidador);
    }

    //

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
    {
        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"

    

        // Ticket 2 finalizado


        
        // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict
        // Ticket 3
        var duplicado = await _db.Cuidadores.AnyAsync(c =>
            c.Nombre == cuidador.Nombre &&
            c.Turno == cuidador.Turno);

        if (duplicado)
        {
            return Conflict(
                "Ya existe un cuidador con el mismo nombre y turno.");
        }

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = cuidador.Id },
            cuidador);
    
        //

        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);
    }

    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)



    

    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas asignadas
    // 
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var cuidador = await _db.Cuidadores
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cuidador is null)
            return NotFound($"No se encontró el cuidador con id {id}.");

        var tieneMascotasAsignadas = await _db.Mascotas
            .AnyAsync(m => m.CuidadorId == id);

        if (tieneMascotasAsignadas)
        {
            return Conflict(
                "No se puede eliminar el cuidador porque tiene mascotas asignadas.");
        }

        _db.Cuidadores.Remove(cuidador);
        await _db.SaveChangesAsync();

        return NoContent();
    }


    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe
}
