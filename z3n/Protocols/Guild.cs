using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.CommandCenter;

namespace z3n
{
    public class Guild
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        private readonly Logger _logger;

        protected readonly bool _logShow;
        protected readonly Sql _sql;

        protected string _status;
        protected string _login;
        protected string _pass;
        protected string _2fa;

        public Guild(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {

            _project = project;
            _instance = instance;
            _sql = new Sql(_project, log);
            _logger = new Logger(project, log: log, classEmoji: "GUILD");

        }

        public void ParseRoles(string tablename)
        {
            
            var done = new List<string>();
            var undone = new List<string>();



            var roles = _instance.ActiveTab.FindElementsByAttribute("div", "id", "role-", "regexp").ToList();
            _sql.ClmnAdd(tablename, "guild_done");
            _sql.ClmnAdd(tablename, "guild_undone");


            foreach (HtmlElement role in roles)
            {
                string name = role.FindChildByAttribute("div", "class", "flex\\ items-center\\ gap-3\\ p-5", "regexp", 0).InnerText;
                string state = role.FindChildByAttribute("div", "class", "mb-4\\ flex\\ items-center\\ justify-between\\ p-5\\ pb-0\\ transition-transform\\ md:mb-6", "regexp", 0).InnerText;
                string total = name.Split('\n')[1];
                name = name.Split('\n')[0];
                state = state.Split('\n')[1];

                if (state.Contains("No access") || state.Contains("Join Guild"))
                {
                    string undoneT = "";
                    var tasksHe = role.FindChildByAttribute("div", "class", "flex\\ flex-col\\ p-5\\ pt-0", "regexp", 0);
                    var tasks = tasksHe.FindChildrenByAttribute("div", "class", "w-full\\ transition-all\\ duration-200\\ translate-y-0\\ opacity-100", "regexp").ToList();
                    foreach (HtmlElement task in tasks)
                    {
                        string taskText = task.InnerText.Split('\n')[0];
                        if (task.InnerHtml.Contains("M208.49,191.51a12,12,0,0,1-17,17L128,145,64.49,208.49a12,12,0,0,1-17-17L111,128,47.51,64.49a12,12,0,0,1,17-17L128,111l63.51-63.52a12,12,0,0,1,17,17L145,128Z"))
                        {
                            _logger.Send($"[!Undone]: {taskText}");
                            undoneT += taskText + ", ";
                        }
                    }
                    undoneT = undoneT.Trim().Trim(',');
                    undone.Add($"[{name}]: {undoneT}");
                }
                else if (state.Contains("You have access"))
                {
                    done.Add($"[{name}] ({total})");
                }
                else if (state.Contains("Reconnect"))
                {
                    undone.Add($"[{name}]: reconnect");
                }
                string wDone = string.Join("; ", done);
                string wUndone = string.Join("; ", undone);
                _project.Var("guildUndone", wUndone);

                _sql.Upd($"guild_done = '{wDone}', guild_undone = '{wUndone}'", tablename);
                _logger.Send($"{name}({total}) :  {state}");
            }

        }

    }
}
