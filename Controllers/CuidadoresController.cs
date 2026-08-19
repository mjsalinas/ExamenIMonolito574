using System.Globalization;
using System.Text.RegularExpressions;
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
    public async Task<IActionResult> GetAll() =>
          Ok(await _db.Cuidadores.OrderBy(c => c.Nombre).ToListAsync());

    // Ticket 1: GetById
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0) return BadRequest("El ID debe ser mayor a cero.");

        var cuidador = await _db.Cuidadores.FindAsync(id);
        return cuidador == null ? NotFound("Cuidador no encontrado.") : Ok(cuidador);
    }
    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe


    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)


    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas 
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0) return BadRequest("El ID debe ser mayor a cero.");

        var c = await _db.Cuidadores.FindAsync(id);
        if (c == null) return NotFound("Cuidador no encontrado.");

        // Regla de negocio: No eliminar si tiene mascotas a cargo
        bool tieneMascotas = await _db.Mascotas.AnyAsync(m => m.CuidadorId == id);
        if (tieneMascotas)
            return Conflict("No se puede eliminar el cuidador porque tiene mascotas asignadas a su cargo.");

        _db.Cuidadores.Remove(c);
        await _db.SaveChangesAsync();

        return NoContent();
    }


    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe

}
