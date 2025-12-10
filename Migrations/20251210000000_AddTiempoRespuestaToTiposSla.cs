using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DamslaApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTiempoRespuestaToTiposSla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TiempoRespuesta",
                table: "TiposSla",
                type: "integer",
                nullable: false,
                defaultValue: 35);

            // Actualizar datos existentes
            migrationBuilder.Sql(@"
                UPDATE ""TiposSla"" 
                SET ""TiempoRespuesta"" = 35 
                WHERE ""Codigo"" = 'SLA1';
                
                UPDATE ""TiposSla"" 
                SET ""TiempoRespuesta"" = 20 
                WHERE ""Codigo"" = 'SLA2';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TiempoRespuesta",
                table: "TiposSla");
        }
    }
}
