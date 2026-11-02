using Microsoft.AspNetCore.Hosting;

namespace EssenceMvp.Tests.Integration;

// Arranca la API contra PostgreSQL real. La cadena de conexión viene de ESSENCE_TEST_PG y
// debe apuntar a una base descartable (creada con db/essence_mvp_schema.sql) cuyo nombre
// contenga "test": la prueba escribe usuarios, proyectos y snapshots.
public class PostgresWebFactory : EssenceWebFactory
{
    public const string EnvVar = "ESSENCE_TEST_PG";

    protected override bool UseInMemoryDatabase => false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var cs = PostgresFactAttribute.ConnectionString
                 ?? throw new InvalidOperationException($"{EnvVar} no está definida.");
        // UseSetting limita la cadena a este host; no toca el entorno del proceso de pruebas.
        builder.UseSetting("ConnectionStrings:DefaultConnection", cs);
        base.ConfigureWebHost(builder);
    }
}

// Se omite sin ESSENCE_TEST_PG, o si la base no parece descartable, para que la suite normal
// no dependa de una base local ni pueda escribir en una compartida.
public sealed class PostgresFactAttribute : FactAttribute
{
    public static string? ConnectionString => Environment.GetEnvironmentVariable(PostgresWebFactory.EnvVar);

    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            Skip = $"Define {PostgresWebFactory.EnvVar} con la cadena de una base PostgreSQL descartable.";
        else if (!LooksDisposable(ConnectionString))
            Skip = $"{PostgresWebFactory.EnvVar} debe apuntar a una base cuyo nombre contenga \"test\".";
    }

    private static bool LooksDisposable(string cs) =>
        cs.Split(';').Select(p => p.Split('=', 2))
            .Any(p => p.Length == 2
                      && p[0].Trim().Equals("Database", StringComparison.OrdinalIgnoreCase)
                      && p[1].Contains("test", StringComparison.OrdinalIgnoreCase));
}
