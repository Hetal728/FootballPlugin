using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Commands;

namespace FootballPlugin.Commands
{
    public static class CommandHandler
    {
        public static void AddAll()
        {
            Command.all.Remove(Command.all.Find("top"));
            Command.all.Remove(Command.all.Find("whois"));
            Command.all.Add(new CmdSkills());
            Command.all.Add(new CmdSpec());
            Command.all.Add(new CmdSpinball());
            Command.all.Add(new CmdTeamSet());
            Command.all.Add(new CmdTop());
            Command.all.Add(new CmdWhois());
            CommandPerms.Load();
        }
    }
}
