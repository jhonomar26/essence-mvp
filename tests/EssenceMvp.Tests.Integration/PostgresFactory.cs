namespace EssenceMvp.Tests.Integration;

// Arranca la API contra PostgreSQL real. La cadena de conexión viene de ESSENCE_TEST_PG
// (apuntar a una base descartable creada con db/essence_mvp_schema.sql).
public class PostgresWebFactory : EssenceWebFactory
{
    public const string EnvVar = "ESSENCE_TEST_PG";

    protected override bool UseInMemoryDatabase => false;

    public PostgresWebFactory()
    {
        var cs = Environment.GetEnvironmentVariable(EnvVar);
        if (!string.IsNullOrEmpty(cs))
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);
    }
}

// Se omite sin ESSENCE_TEST_PG, para que la suite normal no dependa de una base local.
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(PostgresWebFactory.EnvVar)))
            Skip = $"Define {PostgresWebFactory.EnvVar} con la cadena de una base PostgreSQL descartable.";
    }
}
