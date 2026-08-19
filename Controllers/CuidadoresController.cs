using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
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
        return BadRequest("El ID debe ser mayor que cero.");

    var cuidador = await _db.Cuidadores.FindAsync(id);
    if (cuidador == null)
        return NotFound($"No se encontró un cuidador con ID {id}.");

    return Ok(cuidador);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
{
    // Normalizar texto
    cuidador.Nombre = NormalizarTexto(cuidador.Nombre);
    cuidador.Turno = cuidador.Turno?.Trim();

    // Validar formato de Nombre
    if (!EsNombreValido(cuidador.Nombre))
        return BadRequest("El nombre debe tener entre 2 y 100 caracteres y solo contener letras, espacios o guiones.");

    // Validar Turno (lista cerrada)
    var turnosPermitidos = new[] { "Mañana", "Tarde", "Noche" };
    if (string.IsNullOrWhiteSpace(cuidador.Turno) || !turnosPermitidos.Contains(cuidador.Turno))
        return BadRequest("El turno debe ser exactamente: 'Mañana', 'Tarde' o 'Noche'.");

    // Validar duplicado (Ticket 3)
    
    _db.Cuidadores.Add(cuidador);
    await _db.SaveChangesAsync();
    return CreatedAtAction(nameof(GetAll), new { id = cuidador.Id }, cuidador);

    var existeDuplicado = await _db.Cuidadores
    .AnyAsync(c => c.Nombre == cuidador.Nombre && c.Turno == cuidador.Turno);
    
    if (existeDuplicado)
    return Conflict($"Ya existe un cuidador con el nombre '{cuidador.Nombre}' y turno '{cuidador.Turno}'.");
}

    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)
    

    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas asignadas

    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe
}
