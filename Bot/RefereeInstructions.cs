using System;
using System.IO;
using MCGalaxy;
using MCGalaxy.Bots;
using MCGalaxy.Maths;

namespace FootballPlugin.Bot
{
    public sealed class RefereeInstruction : BotInstruction
    {
        public override string Name { get { return "referee"; } }
        public bool skinSet = false;
        public override bool Execute(PlayerBot bot, InstructionData data)
        {
            bot.TargetPos.X = (ushort)(Core.Ball.Position3d.X - 64);
            bot.TargetPos.Y = (ushort)32 + 51;
            bot.TargetPos.Z = (ushort)Constants.TouchLineTop;
            bot.movement = true;
            int dx = (ushort)(Core.Ball.Position3d.X) - bot.pos[0], dy = (ushort)Core.Ball.Position3d.Z - bot.pos[1], dz = (ushort)Core.Ball.Position3d.Y - bot.pos[2];

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            Orientation rot = bot.Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
            bot.Rot = rot;
            return true;
        }

        static void MoveTowards(PlayerBot bot)
        {
            int dx = (int)Core.Ball.Position3d.X - bot.pos[0], dy = (int)Core.Ball.Position3d.Z - bot.pos[1], dz = (int)Core.Ball.Position3d.Y - bot.pos[2];
            bot.TargetPos.X = (ushort)(Core.Ball.Position3d.X - 64);
            bot.TargetPos.Y = (ushort)32 + 51;
            bot.TargetPos.Z = (ushort)Constants.TouchLineTop;
            bot.movement = true;

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            byte yaw, pitch;
            DirUtils.GetYawPitch(dir, out yaw, out pitch);

            // If we are very close to a player, switch from trying to look
            // at them to just facing the opposite direction to them
            if (Math.Abs(dx) >= 4 || Math.Abs(dz) >= 4)
            {
                bot.rot[0] = yaw;
            }
            bot.rot[1] = pitch;
        }

        public override InstructionData Parse(string[] args)
        {
            InstructionData data = default(InstructionData);
            if (args.Length > 1)
                data.Metadata = ushort.Parse(args[1]);
            return data;
        }

        public override void Output(Player p, string[] args, StreamWriter w)
        {
            if (args.Length > 3)
            {
                w.WriteLine(Name + " " + ushort.Parse(args[3]));
            }
            else
            {
                w.WriteLine(Name);
            }
        }

        public override string[] Help { get { return help; } }
        static string[] help = { "%T/botai add [name] hunt <radius>",
            "%HCauses the bot to move towards the closest player in the search radius.",
            "%H  <radius> defaults to 75 blocks.",
        };
    }
}
