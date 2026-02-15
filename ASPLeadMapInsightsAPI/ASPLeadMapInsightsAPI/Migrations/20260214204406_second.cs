using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPLeadMapInsightsAPI.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Divisions_DivisionId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Families_FamilyId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Genera_GenusId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Species_SpeciesId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_TaxonomyClasses_TaxonomyClassId",
                table: "Trees");

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Divisions_DivisionId",
                table: "Trees",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Families_FamilyId",
                table: "Trees",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Genera_GenusId",
                table: "Trees",
                column: "GenusId",
                principalTable: "Genera",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Species_SpeciesId",
                table: "Trees",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_TaxonomyClasses_TaxonomyClassId",
                table: "Trees",
                column: "TaxonomyClassId",
                principalTable: "TaxonomyClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Divisions_DivisionId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Families_FamilyId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Genera_GenusId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_Species_SpeciesId",
                table: "Trees");

            migrationBuilder.DropForeignKey(
                name: "FK_Trees_TaxonomyClasses_TaxonomyClassId",
                table: "Trees");

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Divisions_DivisionId",
                table: "Trees",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Families_FamilyId",
                table: "Trees",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Genera_GenusId",
                table: "Trees",
                column: "GenusId",
                principalTable: "Genera",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_Species_SpeciesId",
                table: "Trees",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trees_TaxonomyClasses_TaxonomyClassId",
                table: "Trees",
                column: "TaxonomyClassId",
                principalTable: "TaxonomyClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
