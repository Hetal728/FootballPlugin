using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;

namespace FootballPlugin.Commands
{
    public class CmdTeamSet : Command
    {
        public override string name { get { return "teamset"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public CmdTeamSet() { }

        public override void Use(Player p, string message)
        {
            var team = Team.FindPlayer(p);
            if (team == null)
            {
                p.SendMessage("No team");
                return;
            }
            if (team.Captain != p)
            {
                p.SendMessage("You're not captain");
                return;
            }
            if (message == "" || message == null)
            {
                p.SendMessage("Enter arguments please");
                return;
            }
            string[] split = message.SplitSpaces(2);
            Player target = Player.Find(split[1]);
            if (target == null)
            {
                p.SendMessage("Invalid player");
                return;
            }
            if (team != Team.FindPlayer(target))
            {
                p.SendMessage("The player is on another team.");
                return;
            }
            if (split[0].ToLower() == "captain")
                team.Captain = target;
            if (split[0].ToLower() == "goalie")
            {
                if (Core.Ball.PickedUp != null)
                {
                    Chat.MessageAll(team.Goalie.ColoredName + Server.DefaultColor + " dropped the ball");
                    Core.Ball.PickedUp = null;
                }
                team.Goalie = target;
            }
            if (split[0].ToLower() != "captain" && split[0].ToLower() != "goalie")
            {
                p.SendMessage("Invalid option");
                return;
            }
            foreach (Player pl in Player.players)
                HUD.UpdateAll(pl);
            Chat.MessageAll(target.ColoredName + Server.DefaultColor + " is now &a" + split[0] + Server.DefaultColor + " of Team " + team.Color + team.Name);
        }

        public override void Help(Player p)
        {
            Player.Message(p, "%T/teamset [goalie,captain] [player]");
            Player.Message(p, "%HSets team properties!");
        }
    }
}
