using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VKITActivityManager.Migrations
{
    /// <inheritdoc />
    public partial class AddBangHocPhi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhSachHocPhi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NganhDaoTao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGian = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViApDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MucHocPhi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HocPhiGiam25 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HocPhiGiam50 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaDongPhu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhSachHocPhi", x => x.Id);
                });

          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhSachHocPhi");

            migrationBuilder.DropTable(
                name: "HoatDong");
        }
    }
}
