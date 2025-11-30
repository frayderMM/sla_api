using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

namespace DamslaApi.Setup
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Iniciando configuración de base de datos Aurora PostgreSQL...\n");

            var connString = "Host=tcsxesan.cdupy54qjkk5.us-east-1.rds.amazonaws.com;Port=5432;Database=postgres;Username=postgres;Password=123456789;";

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                Console.WriteLine("✅ Conexión exitosa a Aurora PostgreSQL\n");

                // Crear base de datos TCS-XSA si no existe
                try
                {
                    await using var cmd = new NpgsqlCommand("CREATE DATABASE \"TCS-XSA\";", conn);
                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine("✅ Base de datos 'TCS-XSA' creada\n");
                }
                catch (PostgresException ex) when (ex.SqlState == "42P04")
                {
                    Console.WriteLine("⚠️  Base de datos 'TCS-XSA' ya existe\n");
                }

                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al conectar a PostgreSQL: {ex.Message}");
                return;
            }

            // Conectar a TCS-XSA y crear tablas
            var connStringTcs = "Host=tcsxesan.cdupy54qjkk5.us-east-1.rds.amazonaws.com;Port=5432;Database=TCS-XSA;Username=postgres;Password=123456789;";
            
            try
            {
                await using var connTcs = new NpgsqlConnection(connStringTcs);
                await connTcs.OpenAsync();
                Console.WriteLine("✅ Conectado a base de datos 'TCS-XSA'\n");

                var sqlScript = await File.ReadAllTextAsync("Data/setup-database.sql");

                // Ejecutar script SQL
                await using var cmdScript = new NpgsqlCommand(sqlScript, connTcs);
                await cmdScript.ExecuteNonQueryAsync();

                Console.WriteLine("✅ Todas las tablas creadas exitosamente en Aurora PostgreSQL\n");
                Console.WriteLine("📋 Tablas creadas:");
                Console.WriteLine("   - roles");
                Console.WriteLine("   - usuarios");
                Console.WriteLine("   - tipos_sla");
                Console.WriteLine("   - solicitudes");
                Console.WriteLine("   - log_acceso");
                Console.WriteLine("   - alertas\n");
                Console.WriteLine("🎉 Configuración completada. Ahora puedes ejecutar: dotnet run");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al ejecutar script SQL: {ex.Message}");
                Console.WriteLine($"   Detalle: {ex}");
            }
        }
    }
}
