using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RefugioMascotas.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cuidadores",
                columns: new[] { "Id", "Nombre", "Turno" },
                values: new object[,]
                {
                    { 1, "Maria Lopez", "Manana" },
                    { 2, "Carlos Perez", "Tarde" }
                });

            migrationBuilder.InsertData(
                table: "Mascotas",
                columns: new[] { "Id", "CuidadorId", "Edad", "EnTratamiento", "Especie", "Nombre" },
                values: new object[,]
                {
                    { 1, 1, 5, false, "Perro", "Toby" },
                    { 2, 2, 3, true, "Gato", "Michi" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Mascotas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Mascotas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cuidadores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cuidadores",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
