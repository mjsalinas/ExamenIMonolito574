namespace RefugioMascotas.Models;

public class Cuidador
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    // Valor esperado (aún no validado): "Mañana", "Tarde" o "Noche" (Ticket 2)
    public string Turno { get; set; } = string.Empty;

    public List<Mascota> Mascotas { get; set; } = new();
    
}
