using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class YourMigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("16ea936c-7a28-4c30-86a2-9a9704b6115e"),
                column: "ConcurrencyStamp",
                value: "a6571fdb-cd1e-4735-a618-ca082e5e0c3a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7cb750cf-3612-4fb4-9f7d-a38ba8f16bf4"),
                column: "ConcurrencyStamp",
                value: "5f7878ed-0f02-4594-b6d6-8a98b6522b54");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("edf6c246-41d8-475f-8d92-41dddac3aefb"),
                column: "ConcurrencyStamp",
                value: "5b842c02-bc17-4e81-a4d2-568837581a7e");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("3aa42229-1c0f-4630-8c1a-db879ecd0427"),
                columns: new[] { "ConcurrencyStamp", "EmailConfirmationSentAt", "EmailConfirmationToken", "PasswordHash", "SecurityStamp" },
                values: new object[] { "eecb852f-a850-4f20-9227-56e7d279085b", new DateTime(2024, 8, 16, 10, 24, 54, 150, DateTimeKind.Utc).AddTicks(5079), "1c924c70-062a-49d0-b2df-a500479bce77", "AQAAAAIAAYagAAAAEHy8f2UJ6UfMr1oYXY4rN9gFYINM/bm84CepLBa8BywTlenSloYb0BrT4RO3Bo5XYQ==", "5f6c1179-0815-4580-a508-a6c6f3280435" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("cb94223b-ccb8-4f2f-93d7-0df96a7f065c"),
                columns: new[] { "ConcurrencyStamp", "EmailConfirmationSentAt", "EmailConfirmationToken", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6b782973-4ce7-4413-ad5a-e4f759d4d52a", new DateTime(2024, 8, 16, 10, 24, 54, 49, DateTimeKind.Utc).AddTicks(6793), "2ae83cc9-25ae-4662-88a3-64af261cfdec", "AQAAAAIAAYagAAAAEBqAQ6i8hybj2zDMAD3Dm5VqseQN/2ObvsWKlNqWkJ7azCKQXRbtjoKVtO5TJryBPg==", "59e51882-4915-49c8-9731-742d12293cab" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("16ea936c-7a28-4c30-86a2-9a9704b6115e"),
                column: "ConcurrencyStamp",
                value: "fb87a7a5-adb9-4625-ac83-1041f1b09f32");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7cb750cf-3612-4fb4-9f7d-a38ba8f16bf4"),
                column: "ConcurrencyStamp",
                value: "b242f89b-33b5-4ab9-a865-7be7553b9388");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("edf6c246-41d8-475f-8d92-41dddac3aefb"),
                column: "ConcurrencyStamp",
                value: "df46c7f4-ddc8-49ff-87d8-4ac809f91418");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("3aa42229-1c0f-4630-8c1a-db879ecd0427"),
                columns: new[] { "ConcurrencyStamp", "EmailConfirmationSentAt", "EmailConfirmationToken", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d1c16bec-4847-4dff-aaef-f27c10fc3b8e", new DateTime(2024, 8, 16, 9, 44, 4, 322, DateTimeKind.Utc).AddTicks(3943), "bf65cd9d-44dc-4d65-bed6-69dd69f7a8b7", "AQAAAAIAAYagAAAAEHUa9380lu7+I+PNtGAK95N4SvDqlIPcmJIsTh03ZrWC4W3OWgqCSP/C/ns8wpINxg==", "8722e468-96d3-4fc1-be82-67ebef84d418" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("cb94223b-ccb8-4f2f-93d7-0df96a7f065c"),
                columns: new[] { "ConcurrencyStamp", "EmailConfirmationSentAt", "EmailConfirmationToken", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5a769c4e-9cd6-4cb4-b850-5d077e913c5b", new DateTime(2024, 8, 16, 9, 44, 4, 237, DateTimeKind.Utc).AddTicks(6937), "ad454fd1-13a5-40c9-9223-6f0a4baf8740", "AQAAAAIAAYagAAAAEOShpAw5NtH1OqVOIOMIlO9n8oI/+SU3XRv/SW3jhf1R82SaMbyQBmtFlCPFBbM/YQ==", "2ab29ab4-15c1-4b21-8a88-34e7b0b8b706" });
        }
    }
}
