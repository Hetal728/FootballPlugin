using System;
using System.Threading.Tasks;
using System.Timers;
using FootballPlugin.Bot;
using MCGalaxy;
using MCGalaxy.Bots;

namespace FootballPlugin
{
    public class Core : Plugin
    {
        public override int build
        {
            get { return 1; }
        }

        public override string creator
        {
            get { return "Hetal"; }
        }

        public override bool LoadAtStartup
        {
            get
            {
                return true;
            }
        }

        public override string MCGalaxy_Version
        {
            get { return Server.VersionString; }
        }

        public override string name
        {
            get { return "FootballPlugin"; }
        }

        public override string website
        {
            get { return "CCFA.tk"; }
        }

        public override string welcome
        {
            get { return ""; }
        }

        public override void Help(Player p)
        {
        }

        public static Match Match;
        public static Ball Ball;
        public static Scheduler Scheduler = new Scheduler();
        public static System.Timers.Timer footballTimer = new System.Timers.Timer(100);
        public static bool CanTakeKick = true;

        public override void Load(bool startup)
        {
            Task.Run(() => InitFootball());
        }

        public void InitFootball()
        {
            Stats.CheckTableExists();
            SkillHandler.Init();
            PlayerHandlers.InitHandlers();
            BotInstruction.Instructions.Add(new RefereeInstruction());
            BotInstruction.Instructions.Add(new BallInstruction());
            Server.s.Log("Added player handlers");
            Commands.CommandHandler.AddAll();
            Server.s.Log("Added commands");
            Ball = new Ball();
            Match = new Match(Ball);
            footballTimer.Interval = ServerConfig.PositionUpdateInterval;
            footballTimer.Elapsed += delegate
            {
                Scheduler.Update();
                foreach (Player p in Player.players)
                    HUD.UpdateAll(p);
            };
            footballTimer.Start();
            Core.Match.MatchTimer.Elapsed += new ElapsedEventHandler(Match.UnforcedEnd);
            Task.Run(() => Match.KickOff(true));
            Server.s.Log("Football started");
        }
        public override void Unload(bool shutdown)
        {
            Match.End(false);
        }
    }
}
