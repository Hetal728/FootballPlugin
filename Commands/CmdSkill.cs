using System;
using System.IO;
using System.Threading.Tasks;
using MCGalaxy;
using System.Net;

namespace FootballPlugin.Commands
{
    public sealed class CmdSkills : Command
    {
        public override string name { get { return "skills"; } }
        public override string shortcut { get { return "skill"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override CommandEnable Enabled { get { return CommandEnable.Always; } }
        public CmdSkills() { }

        public override void Use(Player p, string message)
        {
            string[] args = message.Split(' ');
            if (args[0].CaselessEq("execute"))
            {
                try
                {
                    var skill = int.Parse(args[1]);
                    Task.Run(() => Skills.Execute(p, skill));
                }
                catch { p.SendMessage("# must be the number of your skill"); return; }
            }
            if (args[0].CaselessEq("add"))
            {
                try
                {
                    var skill = int.Parse(args[1]);
                    if (skill > (int)p.ExtraData["Skills"])
                    {
                        p.SendMessage("You have not bought that skill slot yet.");
                        return;
                    }
                    if (File.Exists("skills/" + p.name.ToLower() + skill.ToString() + ".txt"))
                    {
                        p.SendMessage("Skill already exists. Remove it by /skill remove #");
                        return;
                    }
                    if (args[2] != null && args[2] != "")
                    {
                        try
                        {
                            using (var client = new WebClient())
                            {
                                client.OpenRead(args[2]);
                                Int64 bytes_total = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
                                if (bytes_total > 50000000)
                                {
                                    p.SendMessage("File is too large. (50MB Max)");
                                    return;
                                }
                                client.DownloadFile(args[2], "skills/" + p.name.ToLower() + skill.ToString() + ".txt");
                                p.SendMessage("Skill added. Add a hotkey with /skill execute " + skill.ToString());
                            }
                        }
                        catch { p.SendMessage("Unable to download file."); return; }
                    }
                }
                catch { Help(p); return; }
            }
            if (args[0].CaselessEq("remove"))
            {
                try
                {
                    var skill = int.Parse(args[1]);
                    if (!File.Exists("skills/" + p.name.ToLower() + skill.ToString() + ".txt"))
                    {
                        p.SendMessage("Skill doesn't exist.");
                        return;
                    }
                    File.Delete("skills/" + p.name.ToLower() + skill.ToString() + ".txt");
                    p.SendMessage("Skill removed");
                }
                catch { Help(p); return; }
            }
            if (args[0].CaselessEq("list"))
            {
                try
                {
                    var files = Directory.GetFiles("skills/", p.name.ToLower());
                    foreach (string file in files)
                        p.SendMessage(file);
                }
                catch { Help(p); return; }
            }
            if (args[0] == "")
                Help(p);
            foreach (Char s in args[0])
            {
                if (s.Equals('y') || s.Equals('h') || s.Equals('n') || s.Equals('m') || s.Equals(',') || s.Equals('k') || s.Equals('i') || s.Equals('u'))
                {
                    p.ExtraData["SkillSequence"] = (string)p.ExtraData["SkillSequence"] + s;
                    p.ExtraData["SkillTime"] = DateTime.Now.AddMilliseconds(500);
                    HUD.UpdateLower(p);
                }
            }
        }

        public override void Help(Player p)
        {
            Player.Message(p, "%T/skill execute #");
            Player.Message(p, "%HExecutes a skill");
            Player.Message(p, "%T/skill add # Website");
            Player.Message(p, "%HDownloads a skill");
            Player.Message(p, "%T/skill remove #");
            Player.Message(p, "%HRemoves a skill");
            Player.Message(p, "%T/skill list");
            Player.Message(p, "%HLists your skills");
            Player.Message(p, "%TTemplate: http://pastebin.com/fjAADhqL");
            Player.Message(p, "%HI.e: /skill add 1 http://pastebin.com/raw/fjAADhqL");
        }
    }
}
