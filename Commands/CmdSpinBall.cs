using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;

namespace FootballPlugin.Commands
{
    public class CmdSpinball : Command
    {
        public override string name { get { return "spinball"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdSpinball() { }
        public override void Use(Player p, string message)
        {
            if (message.Length == 0)
            {
                p.SendMessage("Enter arguments");
                return;
            }
            string[] message_ = message.SplitSpaces(2);
            string dimension = message_[0];
            
            string function = message_[1];
            if (dimension == "p")
            {
                if (function == "add")
                {
                    if ((double)p.ExtraData["ShotPower"] >= 1.25d)
                    {
                        //Just in case
                        p.ExtraData.ChangeOrCreate("ShotPower", 1.25d);
                        p.SendMessage("Max power accomplished");
                        return;
                    }
                    p.ExtraData.ChangeOrCreate("ShotPower", (double)p.ExtraData["ShotPower"] + 0.05);
                }
                if (function == "sub")
                {
                    if ((double)p.ExtraData["ShotPower"] <= 1d)
                    {
                        p.ExtraData.ChangeOrCreate("ShotPower", 1d);
                        p.SendMessage("Min power accomplished");
                        return;
                    }
                    p.ExtraData.ChangeOrCreate("ShotPower", (double)p.ExtraData["ShotPower"] - 0.05);
                }
            }
            if (dimension == "y")
            {
                if (function == "add")
                {
                    if ((double)p.ExtraData["SpinY"] == 1)
                    {
                        p.SendMessage("Max spin accomplished");
                        return;
                    }
                    p.ExtraData.ChangeOrCreate("SpinY", (double)p.ExtraData["SpinY"] + 0.25);
                }
                if (function == "sub")
                {
                    if ((double)p.ExtraData["SpinY"] == -1)
                    {
                        p.SendMessage("Max spin accomplished");
                        return;
                    }
                    p.ExtraData.ChangeOrCreate("SpinY", (double)p.ExtraData["SpinY"] - 0.25);
                }
            }
            if (dimension == "z")
            {
                if (function == "add")
                {
                    if ((double)p.ExtraData["SpinZ"] == 1)
                    {
                        p.SendMessage("Max spin accomplished");
                        return;
                    }
                    p.ExtraData.ChangeOrCreate("SpinZ", (double)p.ExtraData["SpinZ"] + 0.25);
                }
                if (function == "sub")
                {
                    if ((double)p.ExtraData["SpinZ"] == -1)
                    {
                        p.SendMessage("Max spin accomplished");
                        return;
                    }
                    p.ExtraData.ChangeOrCreate("SpinZ", (double)p.ExtraData["SpinZ"] - 0.25);
                }
            }
            HUD.UpdateAll(p);
        }
        public override void Help(Player p)
        {
            Player.Message(p, "%T/spinball [y,p,z,reset] [add,sub]");
            Player.Message(p, "%HChange spin/power!");
        }
    }
}
