//settable
var tableStructure = new Dictionary<string, string>{};
string data = ""; tableName = "";

string schemaName = project.Variables["DBmode"].Value == "PostgreSQL" ? "accounts." : "";
if (!string.IsNullOrEmpty(schemaName)) W3Query(project, "CREATE SCHEMA IF NOT EXISTS accounts;", true);

//Profile
data = "profile";
tableName = schemaName + data;
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
W3MakeTable(project, tableStructure,tableName,true);

//Twitter
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
W3MakeTable(project, tableStructure, tableName, true);
//Discord
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
W3MakeTable(project, tableStructure, tableName, true);
//Google
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
W3MakeTable(project, tableStructure, tableName, true);


//Blockchain
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
W3MakeTable(project, tableStructure, tableName, true);



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
W3MakeTable(project, tableStructure, tableName, true);