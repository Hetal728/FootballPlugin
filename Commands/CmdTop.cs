using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.SQL;
using MCGalaxy.DB;
using MCGalaxy.Commands;

namespace FootballPlugin.Commands
{
    public sealed class CmdTop : Command
    {

        public override string name { get { return "top"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Information; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public CmdTop() { }
        public override CommandAlias[] Aliases
        {
            get { return new[] { new CommandAlias("topten", null, "10"), new CommandAlias("topfive", null, "5") }; }
        }

        public override void Use(Player p, string message)
        {
            string[] args = message.Split(' ');
            if (args.Length < 2) { Help(p); return; }

            int limit = ParseLimit(p, args);
            int offset = ParseOffset(p, args);
            if (limit == -1 || offset == -1) return;

            string col, title;
            string table = "Players", order = "desc";
            switch (args[0])
            {
                case "1":
                    col = "TotalLogin";
                    title = "&aMost logins:";
                    break;
                case "2":
                    col = "TotalDeaths";
                    title = "&aMost deaths:";
                    break;
                case "3":
                    col = "money";
                    title = "&aMost " + Server.moneys + ":";
                    break;
                case "4":
                    col = "firstlogin";
                    title = "&aOldest players:";
                    order = "asc";
                    break;
                case "5":
                    col = "lastlogin";
                    title = "&aMost recent players:";
                    break;
                case "6":
                    col = "TotalKicked";
                    title = "&aMost times kicked:";
                    break;
                case "7":
                    col = "totalBlocks & " + PlayerData.LowerBitsMask;
                    title = "&aMost blocks modified:";
                    break;
                case "8":
                    col = "totalCuboided & " + PlayerData.LowerBitsMask;
                    title = "&aMost blocks drawn:";
                    break;
                case "9":
                    col = "totalBlocks >> " + PlayerData.LowerBits;
                    title = "&aMost blocks placed:";
                    break;
                case "10":
                    col = "totalCuboided >> " + PlayerData.LowerBits;
                    title = "&aMost blocks deleted:";
                    break;
                case "11":
                    col = "TotalRounds";
                    title = "&aMost games played:";
                    table = "FootballStats"; break;
                case "12":
                    col = "Wins";
                    title = "&aMost games won:";
                    table = "FootballStats"; break;
                case "13":
                    col = "Losses";
                    title = "&aMost games lost:";
                    table = "FootballStats"; break;
                case "14":
                    col = "Draws";
                    title = "&aMost games drawn:";
                    table = "FootballStats"; break;
                case "15":
                    col = "Goals";
                    title = "&aMost goals scored:";
                    table = "FootballStats"; break;
                case "16":
                    col = "Assists";
                    title = "&aMost assists made:";
                    table = "FootballStats"; break;
                case "17":
                    col = "Saves";
                    title = "&aMost shots saved:";
                    table = "FootballStats"; break;
                case "18":
                    col = "OwnGoals";
                    title = "&aMost own-goals scored:";
                    table = "FootballStats"; break;
                case "19":
                    col = "Fouls";
                    title = "&aMost fouls made:";
                    table = "FootballStats"; break;
                default:
                    Player.Message(p, "/Top: Unrecognised type \"{0}\".", args[0]);
                    return;
            }

            string strLimit = " LIMIT " + offset + "," + limit;
            DataTable db = Database.Backend.GetRows(table, "DISTINCT Name, " + col,
                                                    "ORDER BY " + col + " " + order + strLimit);

            Player.Message(p, title);
            for (int i = 0; i < db.Rows.Count; i++)
            {
                Player.Message(p, (i + 1) + ") " + db.Rows[i]["Name"] + " - [" + db.Rows[i][col] + "]");
            }
            db.Dispose();
        }

        static int ParseLimit(Player p, string[] args)
        {
            int limit = 0;
            if (!Int32.TryParse(args[1], out limit))
            {
                Player.Message(p, "&c\"{0}\" is not an integer.", args[1]); return -1;
            }

            if (limit < 1)
            {
                Player.Message(p, "&c\"{0}\" is too small, the min limit is 1.", args[1]); return -1;
            }
            if (limit > 19)
            {
                Player.Message(p, "&c\"{0}\" is too large, the max limit is 18.", args[1]); return -1;
            }
            return limit;
        }

        static int ParseOffset(Player p, string[] args)
        {
            if (args.Length <= 2) return 0;
            int offset = 0;
            if (!Int32.TryParse(args[2], out offset))
            {
                Player.Message(p, "&c\"{0}\" is not an integer.", args[2]); return -1;
            }

            if (offset < 0)
            {
                Player.Message(p, "&cOffset must be greater than or equal to 0.", args[2]); return -1;
            }
            return offset;
        }

        public override void Help(Player p)
        {
            Player.Message(p, "%T/top [stat] [number of players to show] <offset>");
            Player.Message(p, "%HPrints a list of players who have the " +
                           "most/top of a particular stat. Available stats:");

            Player.Message(p, "1) Most logins, 2) Most deaths, 3) Money");
            Player.Message(p, "4) First joined, 5) Recently joined, 6) Most kicks");
            Player.Message(p, "7) Blocks modified, 8) Blocks drawn");
            Player.Message(p, "9) Blocks placed, 10) Blocks deleted");

            if (Core.Match == null) return;
            Player.Message(p, "11) Most games, 12) Most games won");
            Player.Message(p, "13) Most games lost, 14) Most games drawn");
            Player.Message(p, "15) Most goals scored, 16) Most assists made");
            Player.Message(p, "17) Most shots saved, 18) Most own-goals scored");
            Player.Message(p, "19) Most fouls made");
        }
    }
}
