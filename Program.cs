using System.Data;
using DapperDataAccess.Models;
using Dapper;
using Microsoft.Data.SqlClient;

var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
var connectionString =
    $"Server=127.0.0.1,1433;Database=curso;User ID=sa;Password={password};TrustServerCertificate=True";

using (var connection = new SqlConnection(connectionString))
{
    //Execute cada grupo de forma separada, em cada grupo somente uma linha deve estar ligada e rode com dotnet run 
    ListCategories(connection);

    //Adiciona inserção múltipla e retorno de Id
    //CreateManyCategory(connection);
    //ExecuteScalar(connection);

    //Adiciona procedures e view
    //ExecuteProcedure(connection);
    //ExecuteReadProcedure(connection);
    //ReadView(connection);

    //Adiciona relacionamentos entre tabelas
    //OneToOne(connection);
    //OneToMany(connection);
    //QueryMultiple(connection);

    //Adiciona IN, LIKE e transação
    //SelectIn(connection);
    //Like(connection, "backend");
    //Transaction(connection);
}

static void ListCategories(SqlConnection connection)
{
    var categories = connection.Query<Category>("SELECT [Id], [Title] FROM [Category]");
    foreach (var item in categories)
    {
        Console.WriteLine($"{item.Id} - {item.Title}");
    }
}

// Insere 2 categorias de uma vez só
static void CreateManyCategory(SqlConnection connection)
{
    var category = new Category();
    category.Id = Guid.NewGuid();
    category.Title = "Amazon AWS";
    category.Url = "amazon";
    category.Description = "Categoria destinada a serviços do AWS";
    category.Order = 8;
    category.Summary = "AWS Cloud";
    category.Featured = false;

    var category2 = new Category();
    category2.Id = Guid.NewGuid();
    category2.Title = "Categoria Nova";
    category2.Url = "categoria-nova";
    category2.Description = "Categoria nova";
    category2.Order = 9;
    category2.Summary = "Categoria";
    category2.Featured = true;

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

    var rows = connection.Execute(insertSql, new[]
    {
        new
        {
            category.Id,
            category.Title,
            category.Url,
            category.Summary,
            category.Order,
            category.Description,
            category.Featured
        },
        new
        {
            category2.Id,
            category2.Title,
            category2.Url,
            category2.Summary,
            category2.Order,
            category2.Description,
            category2.Featured
        }
    });

    Console.WriteLine($"{rows} linhas inseridas");
}

// Insere e já devolve o Id que o próprio banco gerou
static void ExecuteScalar(SqlConnection connection)
{
    var category = new Category();
    category.Title = "Amazon AWS";
    category.Url = "amazon";
    category.Description = "Categoria destinada a serviços do AWS";
    category.Order = 8;
    category.Summary = "AWS Cloud";
    category.Featured = false;

    var insertSql = @"
        INSERT INTO
            [Category]
        OUTPUT inserted.[Id]
        VALUES(
            NEWID(),
            @Title,
            @Url,
            @Summary,
            @Order,
            @Description,
            @Featured)";

    var id = connection.ExecuteScalar<Guid>(insertSql, new
    {
        category.Title,
        category.Url,
        category.Summary,
        category.Order,
        category.Description,
        category.Featured
    });

    Console.WriteLine($"A categoria inserida foi: {id}");
}

// Procedure que EXECUTA uma ação (apaga um aluno)
static void ExecuteProcedure(SqlConnection connection)
{
    var procedure = "[spDeleteStudent]";
    var pars = new { StudentId = new Guid("6bd552ea-7187-4bae-abb6-54e8f8b9f530") };
    var affectedRows = connection.Execute(
        procedure,
        pars,
        commandType: CommandType.StoredProcedure);

    Console.WriteLine($"{affectedRows} linhas afetadas");
}

// Procedure que LÊ dados (cursos de uma categoria)
static void ExecuteReadProcedure(SqlConnection connection)
{
    var procedure = "[spGetCoursesByCategory]";
    var pars = new { CategoryId = new Guid("09ce0b7b-cfca-497b-92c0-3290ad9d5142") };
    var courses = connection.Query(
        procedure,
        pars,
        commandType: CommandType.StoredProcedure);

    foreach (var item in courses)
    {
        Console.WriteLine(item.Title);
    }
}

static void ReadView(SqlConnection connection)
{
    var sql = "SELECT * FROM [vwCourses]";
    var courses = connection.Query(sql);

    foreach (var item in courses)
    {
        Console.WriteLine($"{item.Id} - {item.Title}");
    }
}

// 1 para 1: cada item da carreira tem UM curso
static void OneToOne(SqlConnection connection)
{
    var sql = @"
        SELECT
            *
        FROM
            [CareerItem]
        INNER JOIN
            [Course] ON [CareerItem].[CourseId] = [Course].[Id]";

    var items = connection.Query<CareerItem, Course, CareerItem>(
        sql,
        (careerItem, course) =>
        {
            careerItem.Course = course;
            return careerItem;
        },
        splitOn: "Id");

    foreach (var item in items)
    {
        Console.WriteLine($"{item.Title} - Curso: {item.Course.Title}");
    }
}

// 1 para muitos: cada carreira tem VÁRIOS itens
static void OneToMany(SqlConnection connection)
{
    var sql = @"
        SELECT
            [Career].[Id],
            [Career].[Title],
            [CareerItem].[CareerId],
            [CareerItem].[Title]
        FROM
            [Career]
        INNER JOIN
            [CareerItem] ON [CareerItem].[CareerId] = [Career].[Id]
        ORDER BY
            [Career].[Title]";

    var careers = new List<Career>();
    connection.Query<Career, CareerItem, Career>(
        sql,
        (career, item) =>
        {
            var car = careers.FirstOrDefault(x => x.Id == career.Id);
            if (car == null)
            {
                car = career;
                car.Items.Add(item);
                careers.Add(car);
            }
            else
            {
                car.Items.Add(item);
            }

            return career;
        },
        splitOn: "CareerId");

    foreach (var career in careers)
    {
        Console.WriteLine($"{career.Title}");
        foreach (var item in career.Items)
        {
            Console.WriteLine($" - {item.Title}");
        }
    }
}

// Duas consultas numa viagem só ao banco
static void QueryMultiple(SqlConnection connection)
{
    var query = "SELECT * FROM [Category]; SELECT * FROM [Course]";

    using (var multi = connection.QueryMultiple(query))
    {
        var categories = multi.Read<Category>();
        var courses = multi.Read<Course>();

        foreach (var item in categories)
        {
            Console.WriteLine(item.Title);
        }

        foreach (var item in courses)
        {
            Console.WriteLine(item.Title);
        }
    }
}

// WHERE Id IN (...)
static void SelectIn(SqlConnection connection)
{
    var query = "SELECT * FROM [Career] WHERE [Id] IN @Id";

    var items = connection.Query<Career>(query, new
    {
        Id = new[]
        {
            "4327ac7e-963b-4893-9f31-9a3b28a4e72b",
            "e6730d1c-6870-4df3-ae68-438624e04c72"
        }
    });

    foreach (var item in items)
    {
        Console.WriteLine(item.Title);
    }
}

// Busca por parte do texto
static void Like(SqlConnection connection, string term)
{
    var query = "SELECT * FROM [Course] WHERE [Title] LIKE @exp";

    var items = connection.Query<Course>(query, new
    {
        exp = $"%{term}%"
    });

    foreach (var item in items)
    {
        Console.WriteLine(item.Title);
    }
}

// Transação: ou grava tudo, ou não grava nada
static void Transaction(SqlConnection connection)
{
    var category = new Category();
    category.Id = Guid.NewGuid();
    category.Title = "Minha categoria que não";
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

    connection.Open();
    using (var transaction = connection.BeginTransaction())
    {
        var rows = connection.Execute(insertSql, new
        {
            category.Id,
            category.Title,
            category.Url,
            category.Summary,
            category.Order,
            category.Description,
            category.Featured
        }, transaction);

        // Rode uma vez com Commit (grava). Depois troque por Rollback (desfaz) e veja a diferença.
        transaction.Commit();
        // transaction.Rollback();

        Console.WriteLine($"{rows} linhas inseridas");
    }
}