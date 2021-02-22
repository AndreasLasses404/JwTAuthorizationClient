using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IndividuellUppgiftKlient.Migrations
{
    public partial class tokenexpire : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "JwtExpires",
                table: "users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshExpires",
                table: "users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtExpires",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshExpires",
                table: "users");
        }
    }
}
