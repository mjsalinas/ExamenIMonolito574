using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private const int LongitudMinima = 2;
    private const int LongitudMaxima = 50;

    private readonly RefugioDbContext _db;

    public MascotasController(RefugioDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var mascotas = await _db.Mascotas
            .Include(m => m.Cuidador)
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return Ok(mascotas);
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
        mascota.Nombre = NormalizarTexto(mascota.Nombre);
        mascota.Especie = NormalizarTexto(mascota.Especie);

        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            return BadRequest("El nombre de la mascota es obligatorio.");

        if (mascota.Nombre.Length < LongitudMinima ||
            mascota.Nombre.Length > LongitudMaxima)
        {
            return BadRequest(
                $"El nombre debe tener entre {LongitudMinima} y {LongitudMaxima} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(mascota.Especie))
            return BadRequest("La especie es obligatoria.");

        if (mascota.Especie.Length < LongitudMinima ||
            mascota.Especie.Length > LongitudMaxima)
        {
            return BadRequest(
                $"La especie debe tener entre {LongitudMinima} y {LongitudMaxima} caracteres.");
        }

        if (mascota.Edad < 0 || mascota.Edad > 30)
            return BadRequest("La edad debe estar entre 0 y 30 años.");

        var cuidadorExiste = await _db.Cuidadores
            .AnyAsync(c => c.Id == mascota.CuidadorId);

        if (!cuidadorExiste)
            return BadRequest("El cuidador especificado no existe.");

        _db.Mascotas.Add(mascota);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = mascota.Id },
            mascota);
    }

    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        texto = Regex.Replace(texto.Trim(), @"\s+", " ");

        return CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(texto.ToLower());
    }
}