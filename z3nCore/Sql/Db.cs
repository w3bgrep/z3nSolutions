using Dapper;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using Nethereum.Contracts.Standards.ENS.ETHRegistrarController.ContractDefinition;
using OtpNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZXing.QrCode.Internal;

namespace z3nCore
{
    public static class Db
    {
        private static string Quote(string name, bool isColumnList = false)
        {
            if (isColumnList)
            {
                name = name.Trim().TrimEnd(',');
                var parts = name.Split(',').Select(p => p.Trim()).ToList();
                var result = new List<string>();

                foreach (var part in parts)
                {
                    int equalsIndex = part.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string columnName = part.Substring(0, equalsIndex).Trim();
                        string valuePart = part.Substring(equalsIndex).Trim();
                        result.Add($"\"{columnName}\" {valuePart}");
                    }
                    else
                    {
                        result.Add($"\"{part}\"");
                    }
                }
                return string.Join(", ", result);
            }
            return $"\"{name.Replace("\"", "\"\"")}\"";
        }

        public static string DbGet(this IZennoPosterProjectModel project, string toGet, string tableName = null, bool log = false, bool throwOnEx = false, string key = "acc0", string acc = null, string where = "")
        {
            return new Sql(project, log).Get(toGet, tableName, log, throwOnEx, key, acc, where);
        }
        public static void DbUpd(this IZennoPosterProjectModel project, string toUpd, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, string key = "acc0", object acc = null, string where = "")
        {
            if (toUpd.Contains("relax")) log = true;
            try { project.Var("lastQuery", toUpd); } catch (Exception Ex){ project.SendWarningToLog(Ex.Message, true); }
            new Sql(project, log).Upd(toUpd, tableName, log, throwOnEx, last, key, acc, where);
        }
        public static void MakeAccList(this IZennoPosterProjectModel project, List<string> dbQueries, bool log = false)
        {

            var _project = project;
            var _sql = new Sql(project, log);
            var _logger = new Logger(project, log);
            var result = new List<string>();


            if (!string.IsNullOrEmpty(_project.Variables["acc0Forced"].Value))
            {
                _project.Lists["accs"].Clear();
                _project.Lists["accs"].Add(_project.Variables["acc0Forced"].Value);
                _logger.Send($@"manual mode on with {_project.Variables["acc0Forced"].Value}");
                return;
            }

            var allAccounts = new HashSet<string>();
            foreach (var query in dbQueries)
            {
                try
                {
                    var accsByQuery = _sql.DbQ(query).Trim();
                    if (!string.IsNullOrWhiteSpace(accsByQuery))
                    {
                        var accounts = accsByQuery.Split('\n').Select(x => x.Trim().TrimStart(','));
                        allAccounts.UnionWith(accounts);
                    }
                }
                catch
                {
                    _logger.Send($"{query}");
                }
            }

            if (allAccounts.Count == 0)
            {
                _project.Variables["noAccsToDo"].Value = "True";
                _logger.Send($"♻ noAccountsAvailable by queries [{string.Join(" | ", dbQueries)}]");
                return;
            }
            _logger.Send($"Initial availableAccounts: [{string.Join(", ", allAccounts)}]");
            FilterAccList(project, allAccounts, log);
        }
        private static void FilterAccList(IZennoPosterProjectModel project, HashSet<string> allAccounts, bool log = false)
        {
            var _project = project;
            var _sql = new Sql(project, log);
            var _logger = new Logger(project, log);

            if (!string.IsNullOrEmpty(_project.Variables["requiredSocial"].Value))
            {
                string[] demanded = _project.Variables["requiredSocial"].Value.Split(',');
                _logger.Send($"Filtering by socials: [{string.Join(", ", demanded)}]");

                foreach (string social in demanded)
                {
                    string tableName = $"projects_{social.Trim().ToLower()}";
                    var notOK = _sql.Get($"acc0", tableName, where: "status LIKE '%suspended%' OR status LIKE '%restricted%' OR status LIKE '%ban%' OR status LIKE '%CAPTCHA%' OR status LIKE '%applyed%' OR status LIKE '%Verify%'", log: log)
                        .Split('\n')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrEmpty(x));
                    allAccounts.ExceptWith(notOK);
                    _logger.Send($"After {social} filter: [{string.Join("|", allAccounts)}]");
                }
            }
            _project.Lists["accs"].Clear();
            _project.Lists["accs"].AddRange(allAccounts);
            _logger.Send($"final list [{string.Join("|", _project.Lists["accs"])}]");

        }
        public static string Ref(this IZennoPosterProjectModel project, string refCode = null, bool log = false)
        {
            if (string.IsNullOrEmpty(refCode)) 
                refCode = project.Variables["cfgRefCode"].Value;
            if (string.IsNullOrEmpty(refCode))
                refCode = new Sql(project, log).Get("refcode", where: "TRIM(refcode) != '' ORDER BY RANDOM() LIMIT 1;");
            return refCode;
        }
        public static void MigrateTable(this IZennoPosterProjectModel project, string source, string dest)
        {
            project.SendInfoToLog($"{source} -> {dest}", true);
            project.SqlTableCopy(source, dest);
            try { project.SqlW($"ALTER TABLE {dest} RENAME COLUMN acc0 to id;"); } catch { }
            try { project.SqlW($"ALTER TABLE {dest} RENAME COLUMN key to id;"); } catch { }
        }


        public static string SqlGet(this IZennoPosterProjectModel project, string toGet, string tableName = null, bool log = false, bool throwOnEx = false, string key = "id", object id = null, string where = "")
        {

            if (string.IsNullOrWhiteSpace(toGet))
                throw new ArgumentException("Column names cannot be null or empty", nameof(toGet));

            toGet = Quote(toGet.Trim().TrimEnd(','));
            if (string.IsNullOrEmpty(tableName)) 
                tableName = project.Variables["projectTable"].Value;
            
            if (id is null) id = project.Variables["acc0"].Value;

            string query;
            if (string.IsNullOrEmpty(where))
            {
                query = $"SELECT {toGet} from {tableName} WHERE {key} = {id}";
            }
            else
            {
                query = $@"SELECT {toGet} from {tableName} WHERE {where};";
            }

            return project.SqlR(query, log: log, throwOnEx: throwOnEx);
        }
        public static string SqlUpd(this IZennoPosterProjectModel project, string toUpd, string tableName = null, bool log = false, bool throwOnEx = false, bool last = true, string key = "id", object id = null, string where = "")
        {          
            var parameters = new DynamicParameters();
            if (string.IsNullOrEmpty(tableName)) tableName = project.Var("projectTable");
            if (string.IsNullOrEmpty(tableName)) throw new Exception("TableName is null");

            toUpd = Quote(toUpd, true);
            tableName = Quote(tableName);

            if (last)
            {
                toUpd += ", last = @lastTime";
                parameters.Add("lastTime", DateTime.UtcNow.ToString("MM-ddTHH:mm"));
            }
            if (id is null) id = project.Variables["acc0"].Value;
            
            string query;
            if (string.IsNullOrEmpty(where))
            {
                query = $"UPDATE {tableName} SET {toUpd} WHERE {key} = {id}";
            }
            else
            {
                query = $"UPDATE {tableName} SET {toUpd} WHERE {where}";
            }
            return project.SqlW(query).ToString();
        }

    }

    public static class DbCore
    {
        public static string SqlR(this IZennoPosterProjectModel project, string query, bool log = false, string sqLitePath = null, string pgHost = null, string pgPort = null, string pgDbName = null, string pgUser = null, string pgPass = null, bool throwOnEx = false)
        {
            var _logger = new Logger(project);
            
            if (string.IsNullOrEmpty(sqLitePath)) sqLitePath = project.Var("DBsqltPath");
            if (string.IsNullOrEmpty(pgHost)) pgHost = "localhost";
            if (string.IsNullOrEmpty(pgPort)) pgPort = "5432";
            if (string.IsNullOrEmpty(pgDbName)) pgDbName = "postgres";
            if (string.IsNullOrEmpty(pgUser)) pgUser = "postgres";
            if (string.IsNullOrEmpty(pgPass)) pgPass = project.Var("DBpstgrPass");
            string dbMode = project.Var("DBmode");

            try
            {
                using (var db = dbMode == "PostgreSQL"
                    ? new dSql($"Host={pgHost};Port={pgPort};Database={pgDbName};Username={pgUser};Password={pgPass};Pooling=true;Connection Idle Lifetime=10;")
                    : new dSql(sqLitePath, null))
                {
                    return db.DbReadAsync(query).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                project.SendWarningToLog(ex.Message, true);
                if (throwOnEx) throw ex;
                return string.Empty;
            }
        }
        public static int SqlW(this IZennoPosterProjectModel project, string query, string sqLitePath = null, string pgHost = null, string pgPort = null, string pgDbName = null, string pgUser = null, string pgPass = null, bool throwOnEx = false)
        {
            if (string.IsNullOrEmpty(sqLitePath)) sqLitePath = project.Var("DBsqltPath");
            if (string.IsNullOrEmpty(pgHost)) pgHost = "localhost";
            if (string.IsNullOrEmpty(pgPort)) pgPort = "5432";
            if (string.IsNullOrEmpty(pgDbName)) pgDbName = "postgres";
            if (string.IsNullOrEmpty(pgUser)) pgUser = "postgres";
            if (string.IsNullOrEmpty(pgPass)) pgPass = project.Var("DBpstgrPass");
            string dbMode = project.Var("DBmode");

            try
            {
                using (var db = dbMode == "PostgreSQL"
                    ? new dSql($"Host={pgHost};Port={pgPort};Database={pgDbName};Username={pgUser};Password={pgPass};Pooling=true;Connection Idle Lifetime=10;")
                    : new dSql(sqLitePath, null))
                {
                    return db.DbWriteAsync(query).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                project.SendWarningToLog(ex.Message, true);
                if (throwOnEx) throw ex;
                return 0;
            }
        }
        public static int SqlTableCopy(this IZennoPosterProjectModel project, string sourceTable, string destinationTable, string sqLitePath = null, string pgHost = null, string pgPort = null, string pgDbName = null, string pgUser = null, string pgPass = null, bool throwOnEx = false)
        {
            if (string.IsNullOrEmpty(sourceTable)) throw new ArgumentNullException(nameof(sourceTable));
            if (string.IsNullOrEmpty(destinationTable)) throw new ArgumentNullException(nameof(destinationTable));
            if (string.IsNullOrEmpty(sqLitePath)) sqLitePath = project.Var("DBsqltPath");
            if (string.IsNullOrEmpty(pgHost)) pgHost = "localhost";
            if (string.IsNullOrEmpty(pgPort)) pgPort = "5432";
            if (string.IsNullOrEmpty(pgDbName)) pgDbName = "postgres";
            if (string.IsNullOrEmpty(pgUser)) pgUser = "postgres";
            if (string.IsNullOrEmpty(pgPass)) pgPass = project.Var("DBpstgrPass");
            string dbMode = project.Var("DBmode");

            using (var db = dbMode == "PostgreSQL"
                ? new dSql($"Host={pgHost};Port={pgPort};Database={pgDbName};Username={pgUser};Password={pgPass};Pooling=true;Connection Idle Lifetime=10;")
                : new dSql(sqLitePath, null))
            {
                try
                {
                    return db.CopyTableAsync(sourceTable, destinationTable).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    project.SendWarningToLog(ex.Message, true);
                    if (throwOnEx) throw;
                    return 0;
                }
            }
        }

    }
}
