using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CassMach.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMachinesAndRefactorCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Users_UserId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_Brand_Model",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_UserId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Machines");

            migrationBuilder.CreateTable(
                name: "UserMachines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MachineId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMachines_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMachines_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Brand_Model",
                table: "Machines",
                columns: new[] { "Brand", "Model" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMachines_MachineId",
                table: "UserMachines",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMachines_UserId",
                table: "UserMachines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMachines_UserId_MachineId",
                table: "UserMachines",
                columns: new[] { "UserId", "MachineId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMachines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_Brand_Model",
                table: "Machines");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Machines",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Machines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Brand_Model",
                table: "Machines",
                columns: new[] { "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_Machines_UserId",
                table: "Machines",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Users_UserId",
                table: "Machines",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
