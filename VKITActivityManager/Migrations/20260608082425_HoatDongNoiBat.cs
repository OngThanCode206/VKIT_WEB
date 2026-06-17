using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VKITActivityManager.Migrations
{
    /// <inheritdoc />
    public partial class HoatDongNoiBat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoatDongNoiBats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DuongDanAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoatDongNoiBats", x => x.Id);
                });

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoatDongNoiBats");

         
        }
    }
}
