using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Games;
using MCGalaxy.Commands;
using MCGalaxy.DB;

namespace FootballPlugin.Commands
{
    public sealed class CmdWhois : Command
    {
        public override string name { get { return "whois"; } }
        public override string shortcut { get { return "whowas"; } }
        public override string type { get { return CommandTypes.Information; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override CommandPerm[] ExtraPerms
        {
            get { return new[] { new CommandPerm(LevelPermission.AdvBuilder, "+ can see IPs and if a player is whitelisted") }; }
        }
        public override CommandAlias[] Aliases
        {
            get { return new[] { new CommandAlias("info"), new CommandAlias("i") }; }
        }

        public override void Use(Player p, string message)
        {
            if (message == "") message = p.name;
            int matches;
            Player pl = PlayerInfo.FindMatches(p, message, out matches);
            if (matches > 1) return;

            WhoInfo info;
            if (matches == 1)
            {
                info = FromOnline(pl);
            }
            else
            {
                if (!Formatter.ValidName(p, message, "player")) return;
                Player.Message(p, "Searching database for the player..");
                PlayerData target = PlayerInfo.FindOfflineMatches(p, message);
                if (target == null) return;
                info = FromOffline(target, message);
            }
            WhoInfo.Output(p, info, CheckExtraPerm(p));
        }

        WhoInfo FromOnline(Player who)
        {
            WhoInfo info = new WhoInfo();
            string prefix = who.title == "" ? "" : who.color + "[" + who.titlecolor + who.title + who.color + "] ";
            info.FullName = prefix + who.ColoredName;
            info.Name = who.name;
            info.Group = who.group;
            info.Money = who.money; info.Deaths = who.overallDeath;

            info.TotalBlocks = who.overallBlocks; info.TotalDrawn = who.TotalDrawn;
            info.TotalPlaced = who.TotalPlaced; info.TotalDeleted = who.TotalDeleted;
            info.LoginBlocks = who.loginBlocks;

            info.TimeSpent = who.time; info.TimeOnline = DateTime.Now - who.timeLogged;
            info.First = who.firstLogin;
            info.Logins = who.totalLogins; info.Kicks = who.totalKicked;
            info.IP = who.ip; info.AfkMessage = who.afkMessage;
            info.IdleTime = DateTime.UtcNow - who.LastAction;

            info.RoundsTotal = who.Game.TotalRoundsSurvived;
            info.RoundsMax = who.Game.MaxRoundsSurvived;
            info.InfectedTotal = who.Game.TotalInfected;
            info.InfectedMax = who.Game.MaxInfected;
            info.TotalRounds = (int)who.ExtraData["TotalRounds"]; info.Wins = (int)who.ExtraData["Wins"]; info.Losses = (int)who.ExtraData["Losses"]; info.Draws = (int)who.ExtraData["Draws"];
            info.Goals = (int)who.ExtraData["Goals"]; info.Assists = (int)who.ExtraData["Assists"]; info.OwnGoals = (int)who.ExtraData["OwnGoals"]; info.Saves = (int)who.ExtraData["Saves"]; info.Fouls = (int)who.ExtraData["Fouls"];
            return info;
        }

        WhoInfo FromOffline(PlayerData data, string message)
        {
            Group group = Group.GroupIn(data.Name);
            string color = data.Color == "" ? group.Color : data.Color;
            string prefix = data.Title == "" ? "" : color + "[" + data.TitleColor + data.Title + color + "] ";

            WhoInfo info = new WhoInfo();
            info.FullName = prefix + color + data.Name.TrimEnd('+');
            info.Name = data.Name;
            info.Group = group;
            info.Money = data.Money; info.Deaths = data.Deaths;

            info.TotalBlocks = data.TotalModified; info.TotalDrawn = data.TotalDrawn;
            info.TotalPlaced = data.TotalPlaced; info.TotalDeleted = data.TotalDeleted;
            info.LoginBlocks = -1;

            info.TimeSpent = data.TotalTime.ToString().ParseDBTime();
            info.First = data.FirstLogin;
            info.Last = data.LastLogin;
            info.Logins = data.Logins; info.Kicks = data.Kicks;
            info.IP = data.IP;

            if (Server.zombie.Running)
            {
                ZombieStats stats = Server.zombie.LoadZombieStats(data.Name);
                info.RoundsTotal = stats.TotalRounds; info.InfectedTotal = stats.TotalInfected;
                info.RoundsMax = stats.MaxRounds; info.InfectedMax = stats.MaxInfected;
            }

            if (Core.Match != null)
            {
                FootballStats stats = Stats.LoadFootballStats(data.Name);
                info.TotalRounds = stats.TotalRounds; info.Wins = stats.Wins; info.Losses = stats.Losses; info.Draws = stats.Draws;
                info.Goals = stats.Goals; info.Assists = stats.Assists; info.OwnGoals = stats.OwnGoals; info.Saves = stats.Saves;
            }
            return info;
        }

        public override void Help(Player p)
        {
            Player.Message(p, "%T/whois [name]");
            Player.Message(p, "%HDisplays information about that player.");
            Player.Message(p, "%HNote: Works for both online and offline players.");
        }
    }
}
