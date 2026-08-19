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

        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

        if (cuidador.Nombre.Length > 100)
            return BadRequest("El nombre no puede exceder 100 caracteres.");

        var turnosValidos = new[] { "Mañana", "Tarde", "Noche" };
        if (!turnosValidos.Contains(cuidador.Turno))
            return BadRequest("El turno debe ser 'Mañana', 'Tarde' o 'Noche'.");

        var duplicado = await _db.Cuidadores.AnyAsync(c =>
            c.Nombre == cuidador.Nombre && c.Turno == cuidador.Turno);
        if (duplicado)
            return Conflict("Ya existe un cuidador con ese nombre en ese turno.");

        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);
    }

    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)

    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas asignadas

    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe
}
