using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;

namespace GeoWiki.Cli.Services
{
    public class ShapeFileService
    {
        private readonly DatabaseService databaseService;

        public ShapeFileService(DatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        public async Task AddShapeFileAsync(string filePath, string? tableName)
        {
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
            var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
            var database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE");
            
            // check if all required environment variables are set
            checkEnvironment(host, port, user, password, database);

            // create connection string
            var connectionString = $"Host={host};Port={port};Username={user};Password={password};Database={database}";

            // check file exist.
            Console.WriteLine($"Reading shape file {filePath}.");
            if (!File.Exists(filePath))
                throw new Exception("File not found");

            // read shape file.
            FeatureCollection features = readShapeFile(filePath);

            Console.WriteLine($"Found {features.Count()} features.");

            Console.WriteLine("Found following features:");
            var firstAttribute = features.First().Attributes.GetNames().First();
            foreach (var feature in features.First().Attributes.GetNames())
            {
                Console.WriteLine(feature);
            }

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            bool dbExists = databaseService.DbExist(conn, database!);

            if (tableName is null)
            {
                tableName = Path.GetFileNameWithoutExtension(filePath);
            }

            // Check if the table exists.
            Console.WriteLine($"Checking if table {tableName} exists.");
            var tableExists = databaseService.TableExist(conn, tableName);
            Console.WriteLine($"Table exists: {tableExists}");


            // Create the table if it doesn't exist.
            if (!tableExists)
            {
                Console.WriteLine($"Creating table {tableName}.");
                databaseService.CreateTableWithGeometry(conn, tableName);
            }

            // Add columns if they don't exist.
            foreach (var feature in features.First().Attributes.GetNames())
            {
                Console.WriteLine($"Checking if column {feature} exists.");
                var columnExists = databaseService.CheckColumnExists(conn, tableName, feature.ToLowerInvariant());
                if (!columnExists)
                {
                    Console.WriteLine($"Adding column {feature}.");
                    databaseService.AddColumn(conn, tableName, feature);
                }
            }

            // Insert the features.
            foreach (var feature in features)
            {
                Console.WriteLine($"Inserting feature {feature.Attributes[firstAttribute]}.");
                insertFeature(conn, tableName, feature);
            }
        }

        private static void checkEnvironment(string? host, string? port, string? user, string? password, string? database)
        {
            if (host == null)
            {
                throw new InvalidOperationException("POSTGRES_HOST environment variable is not set.");
            }

            if (port == null)
            {
                throw new InvalidOperationException("POSTGRES_PORT environment variable is not set.");
            }

            if (user == null)
            {
                throw new InvalidOperationException("POSTGRES_USER environment variable is not set.");
            }

            if (password == null)
            {
                throw new InvalidOperationException("POSTGRES_PASSWORD environment variable is not set.");
            }

            if (database == null)
            {
                throw new InvalidOperationException("POSTGRES_DATABASE environment variable is not set.");
            }
        }

        private FeatureCollection readShapeFile(string filePath)
        {
            FeatureCollection features = new FeatureCollection();
            var geometryFactory = new GeometryFactory();
            using (var shapeFileDataReader = new ShapefileDataReader(filePath, geometryFactory))
            {
                DbaseFileHeader header = shapeFileDataReader.DbaseHeader;

                while (shapeFileDataReader.Read())
                {
                    Feature feature = new Feature();
                    AttributesTable attributesTable = new AttributesTable();

                    string[] keys = new string[header.NumFields];

                    var geometry = shapeFileDataReader.Geometry;

                    for (int i = 0; i < header.NumFields; i++)
                    {
                        DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                        keys[i] = fldDescriptor.Name;
                        attributesTable.Add(fldDescriptor.Name, shapeFileDataReader.GetValue(i + 1));
                    }

                    feature.Geometry = geometry;
                    feature.Geometry.SRID = 4326;
                    var envelope = geometry.Envelope;
                    feature.BoundingBox = new Envelope(envelope.Coordinates[0], envelope.Coordinates[2]);
                    feature.Attributes = attributesTable;
                    features.Add(feature);
                }
            }
            return features;
        }

        private void insertFeature(NpgsqlConnection conn, string tableName, IFeature feature)
        {
            var sql = $"INSERT INTO {tableName} (geom, {string.Join(", ", feature.Attributes.GetNames())}) VALUES (ST_GeomFromText('{feature.Geometry.AsText()}', 4326), @{string.Join(", @", feature.Attributes.GetNames())})";
            using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var attribute in feature.Attributes.GetNames())
            {
                cmd.Parameters.AddWithValue(attribute, feature.Attributes[attribute] ?? DBNull.Value);
            }
            // Console.WriteLine($"Executing SQL: {cmd.CommandText}");
            cmd.ExecuteNonQuery();
        }
    }
}