using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public enum schema
    {
        private_blockchain,
        private_twitter,
        private_discord,
        private_google,
        private_github,
        private_api,
        private_settings,
        private_profile,

        public_blockchain,
        public_deposits,
        public_native,
        public_mail,
        public_twitter,
        public_profile,
        public_github,
        public_discord,
        public_google,
        public_browser,
        public_rpc,

        project
    }


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
