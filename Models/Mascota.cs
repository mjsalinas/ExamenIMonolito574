namespace RefugioMascotas.Models;

public class Mascota
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;

    // Rango esperado: 0-30 (la validación actual tiene un error a propósito, ver Ticket 0)
    public int Edad { get; set; }

    public bool EnTratamiento { get; set; }

    public int CuidadorId { get; set; }
    public Cuidador? Cuidador { get; set; }
}
