using DapperDataAccess.Models;
using Dapper;
using Microsoft.Data.SqlClient;

var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
var connectionString =
    $"Server=127.0.0.1,1433;Database=curso;User ID=sa;Password={password};TrustServerCertificate=True";

using (var connection = new SqlConnection(connectionString))
{
    // Deixe só UMA linha sem // e rode com: dotnet run
    ListCategories(connection);
    // GetCategory(connection);
    // CreateCategory(connection);
    // UpdateCategory(connection);
    // DeleteCategory(connection);
}

static void ListCategories(SqlConnection connection)
{
    var categories = connection.Query<Category>("SELECT [Id], [Title] FROM [Category]");
    foreach (var item in categories)
    {
        Console.WriteLine($"{item.Id} - {item.Title}");
    }
}

static void GetCategory(SqlConnection connection)
{
    var category = connection.QueryFirstOrDefault<Category>(
        "SELECT TOP 1 [Id], [Title] FROM [Category] WHERE [Id]=@id",
        new { id = new Guid("af3407aa-11ae-4621-a2ef-2028b85507c4") });

    if (category == null)
    {
        Console.WriteLine("Categoria não encontrada");
        return;
    }

    Console.WriteLine($"{category.Id} - {category.Title}");
}

static void CreateCategory(SqlConnection connection)
{
    var category = new Category();
    category.Id = Guid.NewGuid();
    category.Title = "Amazon AWS";
    category.Url = "amazon";
    category.Description = "Categoria destinada a serviços do AWS";
    category.Order = 8;
    category.Summary = "AWS Cloud";
    category.Featured = false;

    var insertSql = @"INSERT INTO
            [Category]
        VALUES(
            @Id,
            @Title,
            @Url,
            @Summary,
            @Order,
            @Description,
            @Featured)";

    var rows = connection.Execute(insertSql, new
    {
        category.Id,
        category.Title,
        category.Url,
        category.Summary,
        category.Order,
        category.Description,
        category.Featured
    });

    Console.WriteLine($"{rows} linhas inseridas");
    Console.WriteLine($"Id da nova categoria: {category.Id}"); // copie este Id para usar no DeleteCategory
}

static void UpdateCategory(SqlConnection connection)
{
    var updateQuery = "UPDATE [Category] SET [Title]=@title WHERE [Id]=@id";
    var rows = connection.Execute(updateQuery, new
    {
        id = new Guid("af3407aa-11ae-4621-a2ef-2028b85507c4"),
        title = "Frontend 2021"
    });

    Console.WriteLine($"{rows} registros atualizados");
}

static void DeleteCategory(SqlConnection connection)
{
    // Troque o texto abaixo pelo Id que apareceu quando você rodou o CreateCategory
    var id = new Guid("76ab270e-6bf6-4b63-9bcb-438797d80ff6");

    var deleteQuery = "DELETE [Category] WHERE [Id]=@id";
    var rows = connection.Execute(deleteQuery, new { id });

    Console.WriteLine($"{rows} registros excluídos");
}
