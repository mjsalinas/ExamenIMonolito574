using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
{
    private readonly RefugioDbContext _db;
    private object refugio_context;

    public CuidadoresController(RefugioDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cuidadores = await _db.Cuidadores
            .OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToListAsync();

        return Ok(cuidadores);
    }

    // TODO (Ticket 1): GetById(int id) -> 400 si id <= 0, 404 si no existe
    [HttpGet("{id}")]
public IActionResult GetById(int id)
{
    if (id <= 0)
        return BadRequest();

    var cuidador = _db.Cuidadores.Find(id);

    if (cuidador == null)
        return NotFound();

    return Ok(cuidador);
}

    [HttpPost]
    public async Task<IActionResult> Create(Cuidador cuidador)
    {
        // TODO (Ticket 2): normalizar texto (espacios, capitalización) y validar
        // formato de Nombre y que Turno sea exactamente "Mañana", "Tarde" o "Noche"
        cuidador.Nombre = NormalizarTexto(cuidador.Nombre);
        cuidador.Turno = NormalizarTexto(cuidador.Turno);

        //validacion de nombre 
    if (cuidador.Nombre.Length < 2 || cuidador.Nombre.Length > 100)
    {
        return BadRequest("Nombre inválido.");
    }

    if (!TextoValido(cuidador.Nombre))
    {
        return BadRequest("El nombre solo puede tener letras, espacios y guiones.");
    }

     // Validar Turno
    if (cuidador.Turno != "Mañana" &&
        cuidador.Turno != "Tarde" &&
        cuidador.Turno != "Noche")
    {
        return BadRequest("Turno inválido.");
    }
       // TODO (Ticket 3): validar duplicado (Nombre + Turno) -> 409 Conflict

        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");

var existe = await _db.Cuidadores.AnyAsync(c =>
    c.Nombre == cuidador.Nombre &&
    c.Turno == cuidador.Turno);

if (existe)
{
    return Conflict("El cuidador ya existe.");
}
        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cuidador.Id }, cuidador);
    }

    private string NormalizarTexto(string nombre)
    {
        throw new NotImplementedException();
    }

    private bool TextoValido(string nombre)
{
    foreach (char letra in nombre)
    {
        if (!char.IsLetter(letra) && letra != ' ' && letra != '-')
        {
            return false;
        }
    }

    return true;
}
}
    // TODO (Ticket 4): Update(int id, Cuidador cuidadorActualizado)

    // TODO (Ticket 5): Delete(int id) -> 409 si el cuidador tiene mascotas asignadas

    // TODO (Ticket 6): GetMascotasPorCuidador(int id)
    // Ruta esperada: GET api/cuidadores/{id}/mascotas -> 404 si el cuidador no existe
   
