using NetTopologySuite.Features;
using Npgsql;

namespace GeoWiki.Cli.Services
{
    public class DatabaseService
    {
        public void InsertFeature(NpgsqlConnection conn, string tableName, IFeature feature)
        {
            var sql = $"INSERT INTO {tableName} (geom, {string.Join(", ", feature.Attributes.GetNames())}) VALUES (ST_GeomFromText('{feature.Geometry.AsText()}', 4326), @{string.Join(", @", feature.Attributes.GetNames())})";
            using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var attribute in feature.Attributes.GetNames())
            {
                cmd.Parameters.AddWithValue(attribute, feature.Attributes[attribute] ?? DBNull.Value);
            }
            cmd.ExecuteNonQuery();
        }

        public void AddColumn(NpgsqlConnection conn, string tableName, string feature)
        {
            var cmdText = $"ALTER TABLE {tableName} ADD COLUMN {feature} text";
            ExecuteNonQuery(conn, cmdText);
        }

        public bool CheckColumnExists(NpgsqlConnection conn, string tableName, string feature)
        {
            string cmdText = $"SELECT column_name FROM information_schema.columns WHERE table_name = '{tableName}' AND column_name = '{feature}'";
            return ExecuteScalar(conn, cmdText);
        }

        public void CreateTableWithGeometry(NpgsqlConnection conn, string tableName)
        {
            string cmdText = $"CREATE TABLE {tableName} (id serial PRIMARY KEY, geom geometry(Geometry, 4326))";
            ExecuteNonQuery(conn, cmdText);
        }

        public void CreateDatabase(NpgsqlConnection conn, string database)
        {
            string cmdText = $"CREATE DATABASE {database}";
            ExecuteNonQuery(conn, cmdText);
        }

        public void CreateSchema(NpgsqlConnection conn, string schema)
        {
            string cmdText = $"CREATE SCHEMA {schema}";
            ExecuteNonQuery(conn, cmdText);
        }

        public bool DbExist(NpgsqlConnection conn, string database)
        {
            string cmdText = $"SELECT 1 FROM pg_database WHERE datname='{database}'";
            return ExecuteScalar(conn, cmdText);
        }

        public bool SchemaExist(NpgsqlConnection conn, string schema)
        {
            string cmdText = $"SELECT 1 FROM pg_namespace WHERE nspname='{schema}'";
            return ExecuteScalar(conn, cmdText);
        }

        public bool TableExist(NpgsqlConnection conn, string tableName)
        {
            string cmdText = $"SELECT 1 FROM pg_tables WHERE tablename='{tableName}'";
            return ExecuteScalar(conn, cmdText);
        }

        public void DropDatabase(NpgsqlConnection conn, string database)
        {
            string cmdText = $"DROP DATABASE {database}";
            ExecuteNonQuery(conn, cmdText);
        }

        public void DropSchema(NpgsqlConnection conn, string schema)
        {
            string cmdText = $"DROP SCHEMA {schema}";
            ExecuteNonQuery(conn, cmdText);
        }

        public void DropTable(NpgsqlConnection conn, string tableName)
        {
            string cmdText = $"DROP TABLE {tableName}";
            ExecuteNonQuery(conn, cmdText);
        }

        public void ExecuteNonQuery(NpgsqlConnection conn, string cmdText)
        {
            Console.WriteLine($"Executing SQL: {cmdText}");
            using var cmd = new NpgsqlCommand(cmdText, conn);
            cmd.ExecuteNonQuery();
        }

        private bool ExecuteScalar(NpgsqlConnection conn, string cmdText)
        {
            Console.WriteLine($"Executing: {cmdText}");
            using NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn);
            return cmd.ExecuteScalar() != null;
        }
    }
}