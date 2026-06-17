using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VKITActivityManager.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangVideoMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CHỈ TẠO BẢNG PHÂN LOẠI VIDEO
            migrationBuilder.CreateTable(
                name: "PhanLoaiVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenPhanLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanLoaiVideos", x => x.Id);
                });

            // CHỈ TẠO BẢNG VIDEOS
            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoTaNgan = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DuongDanVideo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhanLoaiVideoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_PhanLoaiVideos_PhanLoaiVideoId",
                        column: x => x.PhanLoaiVideoId,
                        principalTable: "PhanLoaiVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // THÊM SẴN 2 DỮ LIỆU MẶC ĐỊNH
            migrationBuilder.InsertData(
                table: "PhanLoaiVideos",
                columns: new[] { "Id", "TenPhanLoai" },
                values: new object[,]
                {
                    { 1, "Video Giới thiệu (Toàn màn hình)" },
                    { 2, "Video Danh sách (Ngang ở dưới)" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Videos_PhanLoaiVideoId",
                table: "Videos",
                column: "PhanLoaiVideoId");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoatDongChuyenNganh");

            migrationBuilder.DropTable(
                name: "SinhVienHocBong");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "ChuyenNganh");

            migrationBuilder.DropTable(
                name: "LoaiHocBong");

            migrationBuilder.DropTable(
                name: "PhanLoaiVideos");

            migrationBuilder.DropColumn(
                name: "NganhId",
                table: "HoatDong");

            migrationBuilder.DropColumn(
                name: "HeDaoTao",
                table: "DanhSachHocPhi");
        }
    }
}
