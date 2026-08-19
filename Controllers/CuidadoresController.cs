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
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El id debe ser mayor que cero.");

        var mascota = await _db.Mascotas.Include(m => m.Cuidador)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mascota == null)
            return NotFound($"No existe una mascota con id {id}.");

        return Ok(mascota);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
    {
        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"
        cuidador.Nombre = NormalizarTexto(cuidador.Nombre);
        cuidador.Turno = NormalizarTexto(cuidador.Turno);

        if (cuidador.Nombre.Length < 2 || cuidador.Nombre.Length > 100)
            return BadRequest("El nombre debe tener entre 2 y 100 caracteres.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(cuidador.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s-]+$"))
            return BadRequest("El nombre solo puede contener letras, espacios y guiones.");

        var turnosValidos = new[] { "Mañana", "Tarde", "Noche" };
        if (!turnosValidos.Contains(cuidador.Turno))
            return BadRequest("Turno inválido. Debe ser 'Mañana', 'Tarde' o 'Noche'.");

        // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict
        bool existeDuplicado = await _db.Cuidadores.AnyAsync(c =>
    c.Nombre == cuidador.Nombre && c.Turno == cuidador.Turno);

        if (existeDuplicado)
            return Conflict("Ya existe un cuidador con ese nombre y turno.");

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
    private string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return texto;
        texto = System.Text.RegularExpressions.Regex.Replace(texto.Trim(), @"\s+", " ");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
    }
}
