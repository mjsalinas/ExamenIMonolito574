using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly RefugioDbContext _db;
    private string NormalizarTexto(string texto)
{
    if (string.IsNullOrWhiteSpace(texto))
        return string.Empty;

    // Recortar espacios
    texto = texto.Trim();
    
    // Colapsar espacios múltiples a uno solo
    texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\s+", " ");
    
    // Capitalizar (primera letra mayúscula, resto minúscula)
    var palabras = texto.Split(' ');
    for (int i = 0; i < palabras.Length; i++)
    {
        if (palabras[i].Length > 0)
        {
            palabras[i] = char.ToUpper(palabras[i][0]) + palabras[i].Substring(1).ToLower();
        }
    }
    
    return string.Join(" ", palabras);
}

private bool EsNombreValido(string nombre)
{
    // 2-100 caracteres, solo letras, espacios y guiones
    return !string.IsNullOrWhiteSpace(nombre) &&
           nombre.Length >= 2 &&
           nombre.Length <= 100 &&
           System.Text.RegularExpressions.Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-]+$");
}

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
    public async Task<IActionResult> GetByid(int id)
    {
    if (id <= 0)
        return BadRequest("El ID debe ser mayor que cero.");

    var mascota = await _db.Mascotas
        .Include(m => m.Cuidador)
        .FirstOrDefaultAsync(m => m.Id == id);
    
    if (mascota == null)
        return NotFound($"No se encontró una mascota con ID {id}.");

    return Ok(mascota);
}
    [HttpPost]
    [HttpPost]
public async Task<IActionResult> Create(Mascota mascota)
{
    // Normalizar texto
    mascota.Nombre = NormalizarTexto(mascota.Nombre);
    mascota.Especie = NormalizarTexto(mascota.Especie);

    // Validar formato de Nombre
    if (string.IsNullOrWhiteSpace(mascota.Nombre) || 
        mascota.Nombre.Length < 2 || 
        mascota.Nombre.Length > 60 ||
        !System.Text.RegularExpressions.Regex.IsMatch(mascota.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-]+$"))
        return BadRequest("El nombre debe tener entre 2 y 60 caracteres y solo contener letras, espacios o guiones.");

    // Validar formato de Especie
    if (string.IsNullOrWhiteSpace(mascota.Especie) || 
        mascota.Especie.Length < 2 || 
        mascota.Especie.Length > 40 ||
        !System.Text.RegularExpressions.Regex.IsMatch(mascota.Especie, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-]+$"))
        return BadRequest("La especie debe tener entre 2 y 40 caracteres y solo contener letras, espacios o guiones.");

    // Validar edad (Ticket 0)
    if (mascota.Edad < 0 || mascota.Edad > 30)
        return BadRequest("La edad debe estar entre 0 y 30 años.");

    var cuidadorExiste = await _db.Cuidadores.AnyAsync(c => c.Id == mascota.CuidadorId);
    if (!cuidadorExiste)
        return BadRequest("El cuidador especificado no existe.");

    // Validar duplicado (Ticket 3)

    _db.Mascotas.Add(mascota);
    await _db.SaveChangesAsync();
    return CreatedAtAction(nameof(GetAll), new { id = mascota.Id }, mascota);
}

    // TODO (Ticket 4): Update(int id, Mascota mascotaActualizada)

    // TODO (Ticket 5): Delete(int id) -> 409 si EnTratamiento es true
}
