using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;


#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Addresses",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Addresses",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "States",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "AB", "Abia" },
                    { 2, "AD", "Adamawa" },
                    { 3, "AK", "Akwa Ibom" },
                    { 4, "AN", "Anambra" },
                    { 5, "BA", "Bauchi" },
                    { 6, "BY", "Bayelsa" },
                    { 7, "BN", "Benue" },
                    { 8, "BO", "Borno" },
                    { 9, "CR", "Cross River" },
                    { 10, "DE", "Delta" },
                    { 11, "EB", "Ebonyi" },
                    { 12, "ED", "Edo" },
                    { 13, "EK", "Ekiti" },
                    { 14, "EN", "Enugu" },
                    { 15, "GO", "Gombe" },
                    { 16, "IM", "Imo" },
                    { 17, "JI", "Jigawa" },
                    { 18, "KD", "Kaduna" },
                    { 19, "KN", "Kano" },
                    { 20, "KT", "Katsina" },
                    { 21, "KE", "Kebbi" },
                    { 22, "KO", "Kogi" },
                    { 23, "KW", "Kwara" },
                    { 24, "LA", "Lagos" },
                    { 25, "NA", "Nasarawa" },
                    { 26, "NI", "Niger" },
                    { 27, "OG", "Ogun" },
                    { 28, "ON", "Ondo" },
                    { 29, "OS", "Osun" },
                    { 30, "OY", "Oyo" },
                    { 31, "PL", "Plateau" },
                    { 32, "RI", "Rivers" },
                    { 33, "SO", "Sokoto" },
                    { 34, "TA", "Taraba" },
                    { 35, "YO", "Yobe" },
                    { 36, "ZA", "Zamfara" },
                    { 37, "FCT", "Federal Capital Territory" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Addresses",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Addresses",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Addresses",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Addresses",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);
        }
    }
}
