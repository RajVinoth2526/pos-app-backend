using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientAppPOSWebAPI.Migrations
{
    public partial class AddIsDraftToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Orders");
        }
    }
}

