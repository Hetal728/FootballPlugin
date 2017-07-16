using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCGalaxy;
using MCGalaxy.Games;

namespace FootballPlugin
{
    public static class HUD
    {
        public static void UpdateAll(Player p)
        {
            Team team = Team.FindPlayer(p);
            try
            {
                if (Team.CurrentTeams[0] != null && Team.CurrentTeams[1] != null)
                    p.SendCpeMessage(CpeMessageType.Status2, Team.CurrentTeams[0].Color + Team.CurrentTeams[0].Name + " " + Team.CurrentTeams[0].Goals + " &a- " + Team.CurrentTeams[1].Color + Team.CurrentTeams[1].Goals + " " + Team.CurrentTeams[1].Name);
                if (team != null && team.Captain != null && team.Goalie != null)
                    p.SendCpeMessage(CpeMessageType.Status3, "Captain: " + team.Captain.ColoredName + " &a||" + Server.DefaultColor + " Goalie: " + team.Goalie.ColoredName);
            }
            catch { }
            SendTimeLeft(true);
            UpdateLower(p);
        }
        public static void UpdateLower(Player p)
        {
            string skill1 = "[y] [u] [i]";
            string skill2 = "[h] &c[j] " + Server.DefaultColor + "[k]";
            string skill3 = "[n] [m] [,]";
            foreach (Char s in (string)(p.ExtraData["SkillSequence"]))
            {
                var count = skill1.Count(x => x == s);

                if (count == 1)
                    skill1 = skill1.Replace("[" + s.ToString() + "]", "&a[" + s.ToString() + "]" + Server.DefaultColor);
                if (count > 1)
                    skill1 = skill1.Replace("[" + s.ToString() + "]", "&b[" + s.ToString() + "]" + Server.DefaultColor);
                count = skill2.Count(x => x == s);
                if (count == 1)
                    skill2 = skill2.Replace("[" + s.ToString() + "]", "&a[" + s.ToString() + "]" + Server.DefaultColor);
                if (count > 1)
                    skill2 = skill2.Replace("[" + s.ToString() + "]", "&b[" + s.ToString() + "]" + Server.DefaultColor);
                count = skill3.Count(x => x == s);
                if (count == 1)
                    skill3 = skill3.Replace("[" + s.ToString() + "]", "&a[" + s.ToString() + "]" + Server.DefaultColor);
                if (count > 1)
                    skill3 = skill3.Replace("[" + s.ToString() + "]", "&b[" + s.ToString() + "]" + Server.DefaultColor);
            }
            p.SendCpeMessage(CpeMessageType.BottomRight3, "SP: " + p.ExtraData["ShotPower"] + " &a|| " + Server.DefaultColor + skill1.ToUpper());

            p.SendCpeMessage(CpeMessageType.BottomRight2, "H: " + p.ExtraData["SpinZ"] + " &a|| " + Server.DefaultColor + skill2.ToUpper());
            p.SendCpeMessage(CpeMessageType.BottomRight1, "V: " + p.ExtraData["SpinY"] + " &a|| " + Server.DefaultColor + skill3.ToUpper());
        }
        public static string LastTimeLeft = "";
        public static void SendTimeLeft(bool forced = false)
        {
            if (Core.Match.EndTime == null)
                return;
            TimeSpan t = Core.Match.EndTime - DateTime.Now;
            string timeleft = GetTimeLeft((int)t.TotalSeconds);
            if (timeleft != LastTimeLeft || forced)
            {

                foreach (Player pl in Player.players)
                {
                    string message = "&a" + timeleft;
                    var team = Team.FindPlayer(pl);
                    if (team == null)
                        message += " &0|| " + Server.DefaultColor + "Team: " + "&2Spectator";
                    else
                        message += " &0|| " + Server.DefaultColor + "Team: " + team.Color + team.Name;

                    pl.SendCpeMessage(CpeMessageType.Status1, message);
                }
                LastTimeLeft = timeleft;
            }
        }

        public static string GetTimeLeft(int seconds)
        {
            if (seconds < 0) return "";
            if (seconds <= 10) return "10s left";
            if (seconds <= 30) return "30s left";
            if (seconds <= 60) return "1m left";
            return ((seconds + 59) / 60) + "m left";
        }
        public static void UpdateTabForAll()
        {
            /*foreach (Player p in Player.players)
            {
                TabList.RemoveAll(p, true, true);
                TabList.Update(p, true);
            }*/
        }
        
        public static void GetTabName(Entity entity, ref string tabName, ref string tabGroup, Player dst)
        {
            try {
                var team = Team.FindPlayer((Player)entity);
                if (team == null)
                {
                    tabGroup = "&2No Team";
                }
                else
                {
                    tabGroup = team.Color + team.Name;
                }
            } catch { }
        }
        public static bool HandlesChatMessage(Player p, string message)
        {
            if (Team.CurrentTeams == null) return false;
            var team = Team.FindPlayer(p);
            if (message[0] == '-' && message.Length > 1 && team != null)
            {
                Player[] players = PlayerInfo.Online.Items;
                string type = " &cto team%S: ";

                foreach (Player pl in players)
                {
                    if (team == Team.FindPlayer(pl))
                        pl.SendMessage(p.ColoredName + type + message.Substring(1));
                }
                p.cancelchat = true;
                return true;
            }
            return false;
        }

        public static void SetPrefix(Player p)
        {
            p.prefix = (bool)p.ExtraData["Spec"] ? "&2[Spec] " + ServerConfig.DefaultColor : p.prefix;
            if (p.group.Prefix != "") p.prefix += "&f" + p.group.Prefix + p.color;
            MCGalaxy.Games.Team team = p.Game.Team;
            p.prefix += team != null ? "<" + team.Color + team.Name + p.color + "> " : "";

            IGame game = p.level == null ? null : p.level.CurrentGame();
            if (game != null) game.AdjustPrefix(p, ref p.prefix);

            bool isOwner = ServerConfig.OwnerName.CaselessEq(p.name);
            string viptitle =
                p.isMod ? string.Format("{0}[&aInfo{0}] ", p.color) :
                p.isDev ? string.Format("{0}[&9Dev{0}] ", p.color) :
                isOwner ? string.Format("{0}[&cOwner{0}] ",p. color) : "";
            p.prefix = p.prefix + viptitle;
            p.prefix = (p.title == "") ? p.prefix : p.prefix + p.color + "[" + p.titlecolor + p.title + p.color + "] ";
        }
    }
}
