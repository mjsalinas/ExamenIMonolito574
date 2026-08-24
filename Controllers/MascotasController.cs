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
    private const int NombreMinLength = 2;
    private const int NombreMaxLength = 60;

    private const int EspecieMinLength = 2;
    private const int EspecieMaxLength = 40;

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

        if (mascota.Nombre.Length < NombreMinLength ||
            mascota.Nombre.Length > NombreMaxLength)
        {
            return BadRequest(
                $"El nombre debe tener entre {NombreMinLength} y {NombreMaxLength} caracteres.");
        }

        if (!Regex.IsMatch(mascota.Nombre, @"^[\p{L}\s-]+$"))
        {
            return BadRequest(
                "El nombre de la mascota solo puede contener letras, espacios y guiones.");
        }

        if (string.IsNullOrWhiteSpace(mascota.Especie))
            return BadRequest("La especie es obligatoria.");

        if (mascota.Especie.Length < EspecieMinLength ||
            mascota.Especie.Length > EspecieMaxLength)
        {
            return BadRequest(
                $"La especie debe tener entre {EspecieMinLength} y {EspecieMaxLength} caracteres.");
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

    private static string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var colapsado = Regex.Replace(
        texto.Trim(),
        @"\s+",
        " ");

        var cultura = CultureInfo.GetCultureInfo("es-HN");

        return cultura.TextInfo.ToTitleCase(
            colapsado.ToLower(cultura));
    }
}