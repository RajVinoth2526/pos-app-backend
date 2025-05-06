using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientAppPOSWebAPI.Migrations
{
    public partial class YourMigrationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VolumeInLiters",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VolumeUnit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "WeightUnit",
                table: "Products",
                newName: "UnitType");

            migrationBuilder.AddColumn<float>(
                name: "DiscountRate",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitialQuantity",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UnitValue",
                table: "Products",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "InitialQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitValue",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "UnitType",
                table: "Products",
                newName: "WeightUnit");

            migrationBuilder.AddColumn<decimal>(
                name: "VolumeInLiters",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VolumeUnit",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
