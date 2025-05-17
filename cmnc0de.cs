

#region using
using System;
using System.Collections.Generic;
using System.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary;
using ZBSolutions;
using NBitcoin;

#endregion

namespace w3tools //by @w3bgrep
{

    public static class TestStatic
{
        public static void Deadline(this IZennoPosterProjectModel project, int sec = 0)
        {
            if (sec != 0)
            {
                var start = project.Variables[$"t0"].Value;
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long startTime = long.Parse(start);
                int difference = (int)(currentTime - startTime);
                if (difference > sec) throw new Exception("Timeout");

            }
            else 
            {
                project.Variables["t0"].Value = (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString();
            }
        }



}


}
