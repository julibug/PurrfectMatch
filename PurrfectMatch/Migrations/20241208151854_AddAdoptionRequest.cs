using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurrfectMatch.Migrations
{
    /// <inheritdoc />
    public partial class AddAdoptionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdoptionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CatId = table.Column<int>(type: "int", nullable: false),
                    HasOtherAnimals = table.Column<bool>(type: "bit", nullable: false),
                    HasChildren = table.Column<bool>(type: "bit", nullable: false),
                    Housing = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdoptionRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdoptionRequests");
        }
    }
}
