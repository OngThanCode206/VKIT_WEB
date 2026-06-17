using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VKITActivityManager.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangUuDiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UuDiems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenUuDiem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MauNen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UuDiems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnhUuDiems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoTaNgan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DuongDanAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UuDiemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhUuDiems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnhUuDiems_UuDiems_UuDiemId",
                        column: x => x.UuDiemId,
                        principalTable: "UuDiems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhUuDiems_UuDiemId",
                table: "AnhUuDiems",
                column: "UuDiemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhUuDiems");

            migrationBuilder.DropTable(
                name: "UuDiems");
        }
    }
}
