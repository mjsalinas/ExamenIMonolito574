using System.Globalization;
using System.Text.RegularExpressions;
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
        var mascotas = await _db.Mascotas.Include(m => m.Cuidador).ToListAsync();

        var catalogo = mascotas
            .OrderBy(m => m.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return Ok(catalogo);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var mascota = await _db.Mascotas
            .Include(m => m.Cuidador)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mascota is null)
            return NotFound();

        return Ok(mascota);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Mascota mascota)
    {
        mascota.Nombre = Normalize(mascota.Nombre);
        mascota.Especie = Normalize(mascota.Especie);

        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            return BadRequest("El nombre de la mascota es obligatorio.");

        if (mascota.Nombre.Length > 100)
            return BadRequest("El nombre no puede exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(mascota.Especie))
            return BadRequest("La especie es obligatoria.");

        if (mascota.Especie.Length > 50)
            return BadRequest("La especie no puede exceder 50 caracteres.");

        if (mascota.Edad < 0 || mascota.Edad > 30)
            return BadRequest("La edad debe estar entre 0 y 30 años.");

        var cuidadorExiste = await _db.Cuidadores.AnyAsync(c => c.Id == mascota.CuidadorId);
        if (!cuidadorExiste)
            return BadRequest("El cuidador especificado no existe.");

        _db.Mascotas.Add(mascota);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = mascota.Id }, mascota);
    }

    // TODO (Ticket 4): Update(int id, Mascota mascotaActualizada)

    // TODO (Ticket 5): Delete(int id) -> 409 si EnTratamiento es true
}
