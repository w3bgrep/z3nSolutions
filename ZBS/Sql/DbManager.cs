using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{



    public class DbManager : Sql
    {
        protected bool _logShow = false;
        protected readonly int _rangeEnd;

        public DbManager(IZennoPosterProjectModel project, bool log = false)
            : base(project, log: log)
        {
            _logShow = log;
            

        }
        



    }
}
