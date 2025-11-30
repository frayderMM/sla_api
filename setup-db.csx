using Npgsql;

var connString = "Host=tcsxesan.cdupy54qjkk5.us-east-1.rds.amazonaws.com;Port=5432;Database=postgres;Username=postgres;Password=123456789;";

await using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();

// Crear base de datos TCS-XSA si no existe
try
{
    await using var cmd = new NpgsqlCommand("CREATE DATABASE \"TCS-XSA\";", conn);
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("✅ Base de datos 'TCS-XSA' creada");
}
catch
{
    Console.WriteLine("⚠️ Base de datos 'TCS-XSA' ya existe");
}

await conn.CloseAsync();

// Conectar a TCS-XSA y crear tablas
var connStringTcs = "Host=tcsxesan.cdupy54qjkk5.us-east-1.rds.amazonaws.com;Port=5432;Database=TCS-XSA;Username=postgres;Password=123456789;";
await using var connTcs = new NpgsqlConnection(connStringTcs);
await connTcs.OpenAsync();

var sqlScript = await File.ReadAllTextAsync("Data/setup-database.sql");

// Ejecutar script SQL
await using var cmdScript = new NpgsqlCommand(sqlScript, connTcs);
await cmdScript.ExecuteNonQueryAsync();

Console.WriteLine("✅ Todas las tablas creadas exitosamente en Aurora PostgreSQL");
Console.WriteLine("📋 Tablas: roles, usuarios, tipos_sla, solicitudes, log_acceso, alertas");
