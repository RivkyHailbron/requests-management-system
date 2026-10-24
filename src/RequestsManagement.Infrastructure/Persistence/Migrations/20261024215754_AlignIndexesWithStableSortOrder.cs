using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequestsManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignIndexesWithStableSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_AssignedTo_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_OrganizationName_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Priority_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Status_CreatedAt",
                table: "Requests");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AssignedTo_CreatedAt",
                table: "Requests",
                columns: new[] { "AssignedTo", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests",
                columns: new[] { "CreatedAt", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_OrganizationName_CreatedAt",
                table: "Requests",
                columns: new[] { "OrganizationName", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Priority_CreatedAt",
                table: "Requests",
                columns: new[] { "Priority", "CreatedAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_CreatedAt",
                table: "Requests",
                columns: new[] { "Status", "CreatedAt", "Id" },
                descending: new[] { false, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_AssignedTo_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_OrganizationName_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Priority_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Status_CreatedAt",
                table: "Requests");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AssignedTo_CreatedAt",
                table: "Requests",
                columns: new[] { "AssignedTo", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_OrganizationName_CreatedAt",
                table: "Requests",
                columns: new[] { "OrganizationName", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Priority_CreatedAt",
                table: "Requests",
                columns: new[] { "Priority", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_CreatedAt",
                table: "Requests",
                columns: new[] { "Status", "CreatedAt" },
                descending: new[] { false, true });
        }
    }
}
