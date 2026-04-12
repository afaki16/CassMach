using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CassMach.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMachinesAndErrorSolutionUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ErrorSolutions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ErrorDesc",
                table: "ErrorSolutions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MachineId",
                table: "ErrorSolutions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machines_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "token_multiplier",
                column: "Value",
                value: "1");

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[] { "no_machine_penalty", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "2" });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorSolutions_MachineId",
                table: "ErrorSolutions",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Brand_Model",
                table: "Machines",
                columns: new[] { "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_Machines_UserId",
                table: "Machines",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorSolutions_Machines_MachineId",
                table: "ErrorSolutions",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ErrorSolutions_Machines_MachineId",
                table: "ErrorSolutions");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_ErrorSolutions_MachineId",
                table: "ErrorSolutions");

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "no_machine_penalty");

            migrationBuilder.DropColumn(
                name: "ErrorDesc",
                table: "ErrorSolutions");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "ErrorSolutions");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ErrorSolutions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "token_multiplier",
                column: "Value",
                value: "0.1");
        }
    }
}
