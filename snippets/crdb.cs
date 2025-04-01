// Определяем перечисление для режимов базы данных
public enum DatabaseMode
{
    SQLite,
    PostgreSQL,
    MySQL
}

// Определяем режим базы данных
DatabaseMode dbMode;
switch (project.Variables["DBmode"].Value.ToLower())
{
    case "sqlite":
        dbMode = DatabaseMode.SQLite;
        break;
    case "postgresql":
        dbMode = DatabaseMode.PostgreSQL;
        break;
    case "mysql":
        dbMode = DatabaseMode.MySQL;
        break;
    default:
        throw new Exception($"Unsupported DBmode: {project.Variables["DBmode"].Value}");
}

// Устанавливаем schemaName и выполняем предварительные настройки
string schemaName = dbMode == DatabaseMode.PostgreSQL ? "accounts." : "";
if (dbMode == DatabaseMode.PostgreSQL)
{
    W3Query(project, "CREATE SCHEMA IF NOT EXISTS accounts;", true);
}
else if (dbMode == DatabaseMode.MySQL)
{
    W3Query(project, "CREATE DATABASE IF NOT EXISTS mydatabase;", true);
    W3Query(project, "USE mydatabase;", true);
}

// Определяем defaultColumn (пример, замените на ваше значение)
string defaultColumn = "TEXT DEFAULT ''";

// Создаем таблицы
var tableStructure = new Dictionary<string, string>{};
string data;

// Profile
data = "profile";
string tableName = schemaName + data;
tableStructure = new Dictionary<string, string>
{
    {"acc0", "INTEGER PRIMARY KEY"},
    {"nickname", "TEXT DEFAULT ''"},
    {"bio", "TEXT DEFAULT ''"},
    {"cookies", "TEXT DEFAULT ''"},
    {"webGL", "TEXT DEFAULT ''"},
    {"timezone", "TEXT DEFAULT ''"},
    {"proxy", "TEXT DEFAULT ''"},  
};
project.SendInfoToLog($"Creating table: {tableName}", true);
W3MakeTable(project, tableStructure, tableName, true);

// Twitter
data = "twitter";
tableName = schemaName + data;
tableStructure = new Dictionary<string, string>
{
    {"acc0", "INTEGER PRIMARY KEY"},
    {"cooldown", "INTEGER DEFAULT 0"},  
    {"status", defaultColumn},
    {"last", defaultColumn},
    {"token", defaultColumn},
    {"login", defaultColumn},
    {"password", defaultColumn},
    {"code2FA", defaultColumn},
    {"emailLogin", defaultColumn},
    {"emailPass", defaultColumn},
    {"recovery2FA", defaultColumn},
};
project.SendInfoToLog($"Creating table: {tableName}", true);
W3MakeTable(project, tableStructure, tableName, true);

// Discord
data = "discord";
tableName = schemaName + data;
tableStructure = new Dictionary<string, string>
{
    {"acc0", "INTEGER PRIMARY KEY"},
    {"cooldown", "INTEGER DEFAULT 0"},
    {"status", defaultColumn},
    {"last", defaultColumn},
    {"servers", defaultColumn},
    {"roles", defaultColumn},
    {"token", defaultColumn},
    {"login", defaultColumn},
    {"password", defaultColumn},
    {"code2FA", defaultColumn},
};
project.SendInfoToLog($"Creating table: {tableName}", true);
W3MakeTable(project, tableStructure, tableName, true);

// Google
data = "google";
tableName = schemaName + data;
tableStructure = new Dictionary<string, string>
{
    {"acc0", "INTEGER PRIMARY KEY"},
    {"cooldown", "INTEGER DEFAULT 0"},
    {"status", defaultColumn},
    {"last", defaultColumn},
    {"login", defaultColumn},
    {"password", defaultColumn},
    {"recoveryEmail", defaultColumn},
    {"code2FA", defaultColumn},
    {"recovery2FA", defaultColumn},
    {"icloud", defaultColumn},
};
project.SendInfoToLog($"Creating table: {tableName}", true);
W3MakeTable(project, tableStructure, tableName, true);

// Blockchain_private
data = "blockchain_private";
tableName = schemaName + data;
tableStructure = new Dictionary<string, string>
{
    {"acc0", "INTEGER PRIMARY KEY"},
    {"publicEVM", "TEXT DEFAULT ''"},
    {"publicSOL", "TEXT DEFAULT ''"},
    {"publicAPT", "TEXT DEFAULT ''"},
    {"private256K1", "TEXT DEFAULT ''"},
    {"privateBASE58", "TEXT DEFAULT ''"},
    {"seedBIP39", "TEXT DEFAULT ''"},
};
project.SendInfoToLog($"Creating table: {tableName}", true);
W3MakeTable(project, tableStructure, tableName, true);

// Blockchain_public
data = "blockchain_public";
tableName = schemaName + data;
tableStructure = new Dictionary<string, string>
{
    {"acc0", "INTEGER PRIMARY KEY"},
    {"evm", "TEXT DEFAULT ''"},
    {"sol", "TEXT DEFAULT ''"},
    {"apt", "TEXT DEFAULT ''"},
    {"sui", "TEXT DEFAULT ''"},
    {"osmo", "TEXT DEFAULT ''"},
    {"xion", "TEXT DEFAULT ''"},
    {"ton", "TEXT DEFAULT ''"},
    {"taproot", "TEXT DEFAULT ''"},
};
project.SendInfoToLog($"Creating table: {tableName}", true);
W3MakeTable(project, tableStructure, tableName, true);