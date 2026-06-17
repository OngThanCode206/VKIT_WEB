using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VKITActivityManager.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangChatbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHoiThuongGaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CauHoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TraLoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiThuongGaps", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHoiThuongGaps");
        }
    }
}
