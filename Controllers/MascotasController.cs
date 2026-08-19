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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mascotas = await _db.Mascotas.Include(m => m.Cuidador).ToListAsync();

        var catalogo = mascotas
            .OrderBy(m => m.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return Ok(catalogo);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var cuidador = await _db.Cuidadores.FindAsync(id);

        if (cuidador == null)
            return NotFound($"No existe un cuidador con id {id}.");

        return Ok(cuidador);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Mascota mascota)
    {
        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            return BadRequest("El nombre de la mascota es obligatorio.");

        if (string.IsNullOrWhiteSpace(mascota.Especie))
            return BadRequest("La especie es obligatoria.");

        // Un voluntario reportó que esta validación de edad se comporta raro (Ticket 0)
        if (mascota.Edad <= 0 || mascota.Edad > 30)
            return BadRequest("La edad debe estar entre 0 y 30 años.");



        var cuidadorExiste = await _db.Cuidadores.AnyAsync(c => c.Id == mascota.CuidadorId);
        if (!cuidadorExiste)
            return BadRequest("El cuidador especificado no existe.");

        // TODO (Ticket 2): normalizar texto y validar formato de Nombre/Especiemascota.Nombre = NormalizarTexto(mascota.Nombre);
        mascota.Especie = NormalizarTexto(mascota.Especie);

        if (mascota.Nombre.Length < 2 || mascota.Nombre.Length > 60)
            return BadRequest("El nombre debe tener entre 2 y 60 caracteres.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(mascota.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s-]+$"))
            return BadRequest("El nombre solo puede contener letras, espacios y guiones.");

        if (mascota.Especie.Length < 2 || mascota.Especie.Length > 40)
            return BadRequest("La especie debe tener entre 2 y 40 caracteres.");


        // TODO (Ticket 3): validar duplicado (Nombre + CuidadorId) -> 409 Conflict
        bool existeDuplicado = await _db.Mascotas.AnyAsync(m =>
    m.Nombre == mascota.Nombre && m.CuidadorId == mascota.CuidadorId);

        if (existeDuplicado)
            return Conflict("Ya existe una mascota con ese nombre bajo ese cuidador.");

        _db.Mascotas.Add(mascota);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = mascota.Id }, mascota);
    }

    // TODO (Ticket 4): Update(int id, Mascota mascotaActualizada)

    // TODO (Ticket 5): Delete(int id) -> 409 si EnTratamiento es true
    private string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return texto;
        texto = System.Text.RegularExpressions.Regex.Replace(texto.Trim(), @"\s+", " ");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
    }
}
