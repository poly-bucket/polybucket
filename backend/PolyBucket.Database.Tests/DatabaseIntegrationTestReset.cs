namespace PolyBucket.Database.Tests;

internal static class DatabaseIntegrationTestReset
{
    internal const string TruncateMutableDataSql = """
        DO $truncate$
        DECLARE
            tables text;
        BEGIN
            SELECT string_agg(format('%I.%I', schemaname, tablename), ', ')
            INTO tables
            FROM pg_tables
            WHERE schemaname = 'public'
              AND tablename NOT IN ('__EFMigrationsHistory', 'Roles', 'Permissions', 'RolePermissions');
            IF tables IS NOT NULL THEN
                EXECUTE 'TRUNCATE TABLE ' || tables || ' RESTART IDENTITY CASCADE';
            END IF;
        END
        $truncate$;
        """;
}
