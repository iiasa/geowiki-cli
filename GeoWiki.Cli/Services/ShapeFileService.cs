using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;

namespace GeoWiki.Cli.Services
{
    public class ShapeFileService
    {
        public async Task AddShapeFileAsync(string filePath)
        {
            // Add the shape file to the database.
            Console.WriteLine($"Reading shape file {filePath}.");
            if (!File.Exists(filePath))
                throw new Exception("File not found");

            // Read the shape file.
            // var sfr = new ShapefileReader(filePath, geometryFactory: GeometryFactory.Default);
            // var gc = sfr.ReadAll();
            // for (int i = 0; i < gc.NumGeometries; i++)
            //     Console.WriteLine(i + " " + gc.Geometries[i].SRID);
            // using (var reader = new ShapefileDataReader(filePath, GeometryFactory.Default))
            // {
            //     int length = reader.DbaseHeader.NumFields;
            //     while (reader.Read())
            //     {
            //         for (int i = 1; i < length; i++)
            //         {
            //             Console.WriteLine(reader.get(i));
            //         }
            //     }
            // }

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
                    var envelope = geometry.Envelope;
                    feature.BoundingBox = new Envelope(envelope.Coordinates[0], envelope.Coordinates[2]);
                    feature.Attributes = attributesTable;
                    features.Add(feature);
                }
            }
            Console.WriteLine($"Added {features.Count()} features.");

            foreach (var feature in features.First().Attributes.GetNames())
            {
                Console.WriteLine(feature);
            }

            var connString = "Host=localhost;Username=postgres;Password=postgres;Database=gis_data";

            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            bool dbExists;

            string cmdText = "SELECT 1 FROM pg_database WHERE datname='gis_data'";
            using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
            {
                dbExists = cmd.ExecuteScalar() != null;
            }

            Console.WriteLine($"Database exists: {dbExists}");

            // if (!dbExists)
            // {
            //     cmdText = "CREATE DATABASE gis_data";
            //     using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
            //     {
            //         await cmd.ExecuteNonQueryAsync();
            //     }
            // }

            // Check if the table exists.
            var tableExists = false;
            string tableName = "iba_shape_file";
            cmdText = $"select case when exists((select * from information_schema.tables where table_name = '" + tableName + "')) then 1 else 0 end";
            using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
            {
                tableExists = (int)cmd.ExecuteScalar() == 1;
            }

            Console.WriteLine($"Table exists: {tableExists}");


            // Create the table if it doesn't exist.
            if (!tableExists)
            {
                cmdText = $"CREATE TABLE {tableName} (id serial PRIMARY KEY, geom geometry(Geometry, 4326))";
                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // // Add columns if they don't exist.

            // foreach (var feature in features.First().Attributes.GetNames())
            // {
            //     Console.WriteLine(feature);
            //     cmdText = $"ALTER TABLE {tableName} ADD COLUMN {feature} text";
            //     using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
            //     {
            //         await cmd.ExecuteNonQueryAsync();
            //     }
            // }

            // Insert the features.
            foreach (var feature in features)
            {
                cmdText = $"INSERT INTO {tableName} (geom) VALUES (ST_GeomFromText('{feature.Geometry.AsText()}', 4326)) ";
                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}