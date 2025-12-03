using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateProductsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS ProductsView AS
                SELECT 
                    Id,
                    json_extract(Data, '$.name') as Name,
                    json_extract(Data, '$.description') as Description,
                    CAST(json_extract(Data, '$.price') AS REAL) as Price,
                    Data,
                    CreatedAt,
                    UpdatedAt
                FROM Products;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS ProductsView;");
        }
    }
}
