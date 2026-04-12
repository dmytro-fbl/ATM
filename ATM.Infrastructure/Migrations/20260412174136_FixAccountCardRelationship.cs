using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAccountCardRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Cards_CardId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Cards",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_UserId",
                table: "Cards",
                newName: "IX_Cards_AccountId");

            migrationBuilder.RenameColumn(
                name: "CardId",
                table: "Accounts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_CardId",
                table: "Accounts",
                newName: "IX_Accounts_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_UserId",
                table: "Accounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Accounts_AccountId",
                table: "Cards",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_UserId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Accounts_AccountId",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Cards",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_AccountId",
                table: "Cards",
                newName: "IX_Cards_UserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Accounts",
                newName: "CardId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                newName: "IX_Accounts_CardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Cards_CardId",
                table: "Accounts",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
