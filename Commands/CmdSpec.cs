using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Network;

namespace FootballPlugin.Commands
{
    public sealed class CmdSpec : Command
    {
        public override string name { get { return "spectator"; } }
        public override string shortcut { get { return "spec"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override CommandEnable Enabled { get { return CommandEnable.Always; } }
        public CmdSpec() { }

        public override void Use(Player p, string message)
        {

            if ((bool)p.ExtraData["Spec"])
            {
                p.ExtraData.ChangeOrCreate("Spec", !(bool)p.ExtraData["Spec"]);
                Player.SendChatFrom(p, p.ColoredName + " %Sis no longer a spectator", false);
                p.SendMapMotd();
                Team.OnJoin(p);
                if (p.HasCpeExt(CpeExt.HackControl))
                    p.Send(Hacks.MakeHackControl(p));
            }
            else
            {
                p.ExtraData.ChangeOrCreate("Spec", !(bool)p.ExtraData["Spec"]);
                Player.SendChatFrom(p, p.ColoredName + " %Sis now a spectator", false);
                Team.OnLeave(p);
                p.SendMapMotd();
                if (p.HasCpeExt(CpeExt.HackControl))
                    p.Send(Packet.HackControl(true, true, true, true, true, -1));
                Entities.GlobalDespawn(p, false, false);

            }

            Entities.GlobalSpawn(p, false, "");
            HUD.UpdateAll(p);
            HUD.SetPrefix(p);
        }

        public override void Help(Player p)
        {
            Player.Message(p, "%T/spectator");
            Player.Message(p, "%HTurns spectator mode on/off.");
            Player.Message(p, "%H  Note that leaving spectator mode automatically makes you join a team");
        }
    }
}
