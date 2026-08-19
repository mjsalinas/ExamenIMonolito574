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
        var mascota = await _db.Mascotas
            .FirstOrDefaultAsync(c => c.Id == id);

        if (id <= 0) return BadRequest();

        if (mascota is null) return NotFound();


        return Ok(mascota);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
    {
        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"
        var NombreLimpio = cuidador.Nombre.Trim().ToUpper();
        var TurnoLimpio = cuidador.Turno.Trim().ToUpper();

        if (NombreLimpio.Length > 100 || NombreLimpio.Length < 2) return BadRequest();

        if ((TurnoLimpio != "Mañana") && (TurnoLimpio != "Tarde") && (TurnoLimpio != "Noche")) return BadRequest();


        // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict

        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();   
        return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);
    }

    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)

    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas asignadas

    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe
}
