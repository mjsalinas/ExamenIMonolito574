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

    private static string Normalize(string texto)
    {
        texto = texto.Trim();
        texto = Regex.Replace(texto, @"\s+", " ");
        texto = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
        return texto;
    }

    private static string NormalizeTurno(string turno)
    {
        if (string.IsNullOrWhiteSpace(turno))
            return string.Empty;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(turno.Trim().ToLower());
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cuidadores = await _db.Cuidadores.ToListAsync();

        var catalogo = cuidadores
            .OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return Ok(catalogo);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var cuidador = await _db.Cuidadores.FindAsync(id);

        if (cuidador is null)
            return NotFound();

        return Ok(cuidador);
    }
    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
    {
        cuidador.Nombre = Normalize(cuidador.Nombre);
        cuidador.Turno = NormalizeTurno(cuidador.Turno);

        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        if (cuidador.Nombre.Length > 100)
            return BadRequest("El nombre no puede exceder 100 caracteres.");

        var turnosValidos = new[] { "Mañana", "Tarde", "Noche" };
        if (!turnosValidos.Contains(cuidador.Turno, StringComparer.CurrentCultureIgnoreCase))
            return BadRequest("El turno debe ser 'Mañana', 'Tarde' o 'Noche'.");

        var duplicado = await _db.Cuidadores.AnyAsync(c =>
            c.Nombre.ToLower() == cuidador.Nombre.ToLower() &&
            c.Turno.ToLower() == cuidador.Turno.ToLower());
        if (duplicado)
            return Conflict("Ya existe un cuidador con ese nombre en ese turno.");

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Cuidador cuidadorActualizado)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var cuidador = await _db.Cuidadores.FindAsync(id);
        if (cuidador is null)
            return NotFound();

        cuidadorActualizado.Nombre = Normalize(cuidadorActualizado.Nombre);
        cuidadorActualizado.Turno = NormalizeTurno(cuidadorActualizado.Turno);

        if (string.IsNullOrWhiteSpace(cuidadorActualizado.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        if (cuidadorActualizado.Nombre.Length > 100)
            return BadRequest("El nombre no puede exceder 100 caracteres.");

        var turnosValidos = new[] { "Mañana", "Tarde", "Noche" };
        if (!turnosValidos.Contains(cuidadorActualizado.Turno, StringComparer.CurrentCultureIgnoreCase))
            return BadRequest("El turno debe ser 'Mañana', 'Tarde' o 'Noche'.");

        var duplicado = await _db.Cuidadores.AnyAsync(c =>
            c.Nombre.ToLower() == cuidadorActualizado.Nombre.ToLower() &&
            c.Turno.ToLower() == cuidadorActualizado.Turno.ToLower() &&
            c.Id != id);
        if (duplicado)
            return Conflict("Ya existe un cuidador con ese nombre en ese turno.");

        cuidador.Nombre = cuidadorActualizado.Nombre;
        cuidador.Turno = cuidadorActualizado.Turno;

        await _db.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var cuidador = await _db.Cuidadores.FindAsync(id);
        if (cuidador is null)
            return NotFound();
    //un Cuidador NO se puede eliminar si tiene mascotas asignadas (409 Conflict).
        var tieneMascotas = await _db.Mascotas.AnyAsync(m => m.CuidadorId == id);
      // Una Mascota NO se puede eliminar si EnTratamiento es true
        if (tieneMascotas)
            return Conflict("No se puede eliminar un cuidador que tiene mascotas asignadas.");

        _db.Cuidadores.Remove(cuidador);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/mascotas")]
    public async Task<IActionResult> GetMascotasPorCuidador(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var cuidador = await _db.Cuidadores.FindAsync(id);
        if (cuidador is null)
            return NotFound();

        var mascotas = await _db.Mascotas
            .Include(m => m.Cuidador)
            .Where(m => m.CuidadorId == id)
            .ToListAsync();

        return Ok(mascotas);
    }

}