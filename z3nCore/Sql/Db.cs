using Nethereum.Contracts.QueryHandlers.MultiCall;
using Nethereum.Contracts.Standards.ENS.ETHRegistrarController.ContractDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3nCore
{
    public static class Db
    {
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
    }
}
