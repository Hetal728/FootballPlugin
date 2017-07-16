using System;
using MCGalaxy;
using MCGalaxy.Bots;

namespace FootballPlugin.Bot
{
    public class BallInstruction : BotInstruction
    {
        public override string Name
        {
            get { return "ball"; }
        }

        public override bool Execute(PlayerBot bot, InstructionData data)
        {
            if (Core.Ball != null)
            {
                Position Pos = new Position();
                Pos.X = (int)Core.Ball.Position3d.X;
                Pos.Y = (int)Core.Ball.Position3d.Z;
                Pos.Z = (int)Core.Ball.Position3d.Y;
                bot.Pos = Pos;
            }
            return true;
        }


        public override string[] Help { get { return help; } }
        static string[] help = { "%T/botai add [name] ball",
            "ball physics",
        };
    }
}
