using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurrfectMatch.Migrations
{
    /// <inheritdoc />
    public partial class AddReasonToReject2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "AdoptionRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "AdoptionRequests");
        }
    }
}
