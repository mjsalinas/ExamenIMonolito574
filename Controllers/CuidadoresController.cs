using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefugioMascotas.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RefugioMascotas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
{
    private const int NombreMinLength = 2;
    private const int NombreMaxLength = 100;
    private static readonly string[] TurnosValidos =
    {
        "Mañana",
        "Tarde",
        "Noche"
    };
    private readonly RefugioDbContext _db;

    public CuidadoresController(RefugioDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cuidadores = await _db.Cuidadores
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(cuidadores);
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
        cuidador.Nombre = NormalizarTexto(cuidador.Nombre);
        cuidador.Turno = NormalizarTexto(cuidador.Turno);
    
        if (string.IsNullOrWhiteSpace(cuidador.Nombre))
            return BadRequest("El nombre del cuidador es obligatorio.");
        
        if (cuidador.Nombre.Length < NombreMinLength ||
            cuidador.Nombre.Length > NombreMaxLength)
        {
            return BadRequest(
                $"El nombre del cuidador debe tener entre {NombreMinLength} y {NombreMaxLength} caracteres.");
    }
        if(!Regex.IsMatch(cuidador.Nombre, @"^[\p{L}\s-]+$"))
        {
            return BadRequest(
                "El nombre del cuidador solo puede contener letras, espacios y guiones.");
    }

        if(!TurnosValidos.Contains(cuidador.Turno))
        {
            return BadRequest(
                $"El turno del cuidador debe ser uno de los siguientes: {string.Join(", ", TurnosValidos)}.");
    }


        _db.Cuidadores.Add(cuidador);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById), 
            new { id = cuidador.Id }, 
            cuidador);
    }

    private static string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

    var colapsado = Regex.Replace
    (texto.Trim(),
    @"\s+",
    " ");

    var cultura = CultureInfo.GetCultureInfo("es-HN");

    return cultura.TextInfo.ToTitleCase
    (colapsado.ToLower(cultura));

    }

}
