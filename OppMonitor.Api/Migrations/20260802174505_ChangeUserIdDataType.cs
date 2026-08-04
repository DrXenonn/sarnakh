using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OppMonitor.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIdDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "identity",
                table: "RefreshTokens",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
