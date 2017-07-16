using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Games;

namespace FootballPlugin
{
    /*public class TabListHandler : TabList
    {
        public override void GetEntry(Player p, Player dst, out string name, out string group)
        {
            string col = GetSupportedCol(dst, p.color);
            group = ServerConfig.TablistGlobal ? "On " + p.level.name : "&fPlayers";
            name = col + p.truename;
            if (Core.Match != null) HUD.GetTabName(p, dst, ref name, ref group);
            if (p.hidden && p.IsAfk) { name += " &f(Hid, &7AFK)"; return; }
            if (p.hidden) name += " &f(Hid)";
            if (p.IsAfk) name += " &7(AFK)";
        }

        internal static string GetSupportedCol(Player dst, string col)
        {
            if (col == null) return null;
            if (col.Length >= 2 && !Colors.IsStandardColor(col[1]) && !dst.HasCpeExt(CpeExt.TextColors))
                col = "&" + Colors.GetFallback(col[1]);
            return col;
        }
    }*/
}
