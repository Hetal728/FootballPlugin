using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Maths;
using System.Reflection;
using System.Threading;

namespace FootballPlugin
{
    public class SkillHandler
    {
        public string Sequence = "";
        public DateTime Time = DateTime.Now;
        public static List<Skill> Skills = new List<Skill>();
        public void GetEnumerator()
        {

        }
        public static void Init()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (!type.IsSubclassOf(typeof(Skill)) || type.IsAbstract) continue;
                Skill skill = (Skill)Activator.CreateInstance(type);
                Skills.Add(skill);
            }
        }

        public static bool Matches(Player p, string sequence)
        {
            List<string> Sequences = new List<string>();
            foreach (Skill skill in Skills)
            {
                Sequences.Add(skill.LeftHandedSequence);
                Sequences.Add(skill.RightHandedSequence);
            }
            if (Sequences.Contains(sequence))
            {
                byte count = 0;
                foreach (string s in Sequences)
                {
                    if (s.Contains(sequence))
                        count++;
                }
                if (count == 1)
                    return true;
            }
            return false;
        }
        public static void Execute(Player p, string sequence)
        {
            HUD.UpdateLower(p);
            try
            {
                p.ExtraData["IsSkilling"] = true;
                foreach (Skill skill in Skills)
                {
                    if (skill.LeftHandedSequence == sequence)
                        skill.ExecuteLeft(p);
                    if (skill.RightHandedSequence == sequence)
                        skill.ExecuteRight(p);
                }
                p.ExtraData.ChangeOrCreate("SkillSequence", "");
                p.ExtraData["IsSkilling"] = false;
            }
            catch
            {
                p.SendMessage("Error occurred in skill with a sequence of " + sequence);
                p.ExtraData["IsSkilling"] = false;
            }
            HUD.UpdateLower(p);
        }
    }

    public abstract class Skill
    {
        public abstract string Name { get; }
        public abstract string RightHandedSequence { get; }
        public abstract string LeftHandedSequence { get; }
        public abstract void ExecuteRight(Player p);
        public abstract void ExecuteLeft(Player p);
    }

    public class Feint : Skill
    {
        public override string Name { get { return "Feint"; } }
        public override string RightHandedSequence { get { return "k"; } }
        public override string LeftHandedSequence { get { return "h"; } }
        public override void ExecuteRight(Player p)
        {
            byte length = 32;
            byte yaw = (byte)((p.rot[0] + 64) % 256);
            Vec3F32 dirVector = DirUtils.GetDirVector(yaw, p.rot[1]);

            ushort newX = (ushort)(p.pos[0] + dirVector.X * length);
            ushort newZ = (ushort)(p.pos[2] + dirVector.Z * length);

            foreach (Player pl in Player.players)
            {
                if (pl != p && pl.level == p.level)
                    pl.SendPos(p.id, newX, p.pos[1], newZ, p.rot[0], p.rot[1]);
            }
        }
        public override void ExecuteLeft(Player p)
        {
            byte length = 32;
            byte yaw = (byte)((p.rot[0] - 64) % 256);
            Vec3F32 dirVector = DirUtils.GetDirVector(yaw, p.rot[1]);

            ushort newX = (ushort)(p.pos[0] + dirVector.X * length);
            ushort newZ = (ushort)(p.pos[2] + dirVector.Z * length);

            foreach (Player pl in Player.players)
            {
                if (pl != p && pl.level == p.level)
                    pl.SendPos(pl.id, newX, p.pos[1], newZ, yaw, p.rot[1]);
            }
        }
    }
    public class Scissor : Skill
    {
        public override string Name { get { return "Scissor"; } }
        public override string RightHandedSequence { get { return "uik"; } }
        public override string LeftHandedSequence { get { return "uyh"; } }
        public override void ExecuteRight(Player p)
        {
            byte length = 32;
            byte yaw = (byte)((p.rot[0] + 32) % 256);
            Vec3F32 dirVector = DirUtils.GetDirVector(yaw, p.rot[1]);

            ushort newX = (ushort)(p.pos[0] + dirVector.X * length);
            ushort newZ = (ushort)(p.pos[2] + dirVector.Z * length);

            foreach (Player pl in Player.players)
            {
                if (pl != p && pl.level == p.level)
                    pl.SendPos(p.id, newX, p.pos[1], newZ, yaw, p.rot[1]);
            }
        }
        public override void ExecuteLeft(Player p)
        {
            byte length = 32;
            Vec3F32 dirVector = DirUtils.GetDirVector((byte)((p.rot[0] - 32) % 256), p.rot[1]);

            ushort newX = (ushort)(p.pos[0] + dirVector.X * length);
            ushort newZ = (ushort)(p.pos[2] + dirVector.Z * length);

            foreach (Player pl in Player.players)
            {
                if (pl != p && pl.level == p.level)
                    pl.SendPos(p.id, newX, p.pos[1], newZ, p.rot[0], p.rot[1]);
            }
        }
    }
    public class Stepover : Skill
    {
        public override string Name { get { return "Stepover"; } }
        public override string RightHandedSequence { get { return "kiu"; } }
        public override string LeftHandedSequence { get { return "hyu"; } }
        public override void ExecuteRight(Player p)
        {
            byte length = 32;
            byte yaw = (byte)((p.rot[0] + 50) % 256);
            Vec3F32 dirVector = DirUtils.GetDirVector(yaw, p.rot[1]);

            ushort newX = (ushort)(p.pos[0] + dirVector.X * length);
            ushort newZ = (ushort)(p.pos[2] + dirVector.Z * length);

            foreach (Player pl in Player.players)
            {
                if (pl != p && pl.level == p.level)
                    pl.SendPos(p.id, newX, p.pos[1], newZ, yaw, p.rot[1]);
            }
        }
        public override void ExecuteLeft(Player p)
        {
            byte length = 32;
            byte yaw = (byte)((p.rot[0] - 50) % 256);
            Vec3F32 dirVector = DirUtils.GetDirVector(yaw, p.rot[1]);

            ushort newX = (ushort)(p.pos[0] + dirVector.X * length);
            ushort newZ = (ushort)(p.pos[2] + dirVector.Z * length);

            foreach (Player pl in Player.players)
            {
                if (pl != p && pl.level == p.level)
                    pl.SendPos(p.id, newX, p.pos[1], newZ, yaw, p.rot[1]);
            }
        }
    }

    public class HeelFlick : Skill
    {
        public override string Name { get { return "Heel Flick"; } }
        public override string RightHandedSequence { get { return "um"; } }
        public override string LeftHandedSequence { get { return "um"; } }
        public override void ExecuteRight(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * -0.1), 0, (float)(dir.Z * -0.3));
            Thread.Sleep(250);
            PlayerHandlers.KickBall(p, (float)(dir.X * 0.5), 0, (float)(dir.Z * 0.5));
        }
        public override void ExecuteLeft(Player p)
        {
            ExecuteRight(p);
        }
    }

    public class FlickUp : Skill
    {
        public override string Name { get { return "Flick Up"; } }
        public override string RightHandedSequence { get { return "uuu"; } }
        public override string LeftHandedSequence { get { return "uuu"; } }
        public override void ExecuteRight(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * 0.1), 0.3f, (float)(dir.Z * 0.1));
        }
        public override void ExecuteLeft(Player p)
        {
            ExecuteRight(p);
        }
    }

    public class Roulette : Skill
    {
        public override string Name { get { return "Roulette"; } }
        public override string RightHandedSequence { get { return "mnhyuik"; } }
        public override string LeftHandedSequence { get { return "m,kiuyh"; } }
        public override void ExecuteRight(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * -0.2), 0, (float)(dir.Z * -0.2));
            byte length = 32;
            byte yaw = (byte)((p.rot[0] + 128) % 256);
            byte targetkick = (byte)((p.rot[0] + 32) % 256);
            var targetdir = DirUtils.GetDirVector(targetkick, p.rot[1]);
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], yaw, p.rot[1]);
            Thread.Sleep(1000);
            PlayerHandlers.KickBall(p, (float)(targetdir.X * 0.4), 0, (float)(targetdir.Z * 0.4));
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], targetkick, p.rot[1]);
        }
        public override void ExecuteLeft(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * -0.2), 0, (float)(dir.Z * -0.2));
            byte length = 32;
            byte yaw = (byte)((p.rot[0] + 128) % 256);
            byte targetkick = (byte)((p.rot[0] - 32) % 256);
            var targetdir = DirUtils.GetDirVector(targetkick, p.rot[1]);
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], yaw, p.rot[1]);
            Thread.Sleep(1000);
            PlayerHandlers.KickBall(p, (float)(targetdir.X * 0.4), 0, (float)(targetdir.Z * 0.4));
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], targetkick, p.rot[1]);
        }
    }

    public class Elastico : Skill
    {
        public override string Name { get { return "Elastico"; } }
        public override string RightHandedSequence { get { return "hnm,k"; } }
        public override string LeftHandedSequence { get { return "k,mnh"; } }
        public override void ExecuteRight(Player p)
        {
            byte first = (byte)((p.rot[0] - 90) % 256);
            var firstdir = DirUtils.GetDirVector(first, p.rot[1]);
            byte second = (byte)((p.rot[0] + 60) % 256);
            var seconddir = DirUtils.GetDirVector(second, p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(firstdir.X * 0.1), 0, (float)(firstdir.Z * 0.1));
            Thread.Sleep(500);
            PlayerHandlers.KickBall(p, (float)(seconddir.X * 0.3), 0, (float)(seconddir.Z * 0.3));
        }
        public override void ExecuteLeft(Player p)
        {
            byte first = (byte)((p.rot[0] + 90) % 256);
            var firstdir = DirUtils.GetDirVector(first, p.rot[1]);
            byte second = (byte)((p.rot[0] - 60) % 256);
            var seconddir = DirUtils.GetDirVector(second, p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(firstdir.X * 0.1), 0, (float)(firstdir.Z * 0.1));
            Thread.Sleep(500);
            PlayerHandlers.KickBall(p, (float)(seconddir.X * 0.3), 0, (float)(seconddir.Z * 0.3));
        }
    }

    public class Spin : Skill
    {
        public override string Name { get { return "Berba Spin"; } }
        public override string RightHandedSequence { get { return ",,"; } }
        public override string LeftHandedSequence { get { return "nn"; } }
        public override void ExecuteRight(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * -0.2), 0, (float)(dir.Z * -0.2));
            byte yaw = (byte)((p.rot[0] + 128) % 256);
            byte targetkick = (byte)((p.rot[0] + 64) % 256);
            var targetdir = DirUtils.GetDirVector(targetkick, p.rot[1]);
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], yaw, p.rot[1]);
            Thread.Sleep(1000);
            PlayerHandlers.KickBall(p, (float)(targetdir.X * 0.4), 0, (float)(targetdir.Z * 0.4));
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], targetkick, p.rot[1]);
        }
        public override void ExecuteLeft(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * -0.2), 0, (float)(dir.Z * -0.2));
            byte yaw = (byte)((p.rot[0] + 128) % 256);
            byte targetkick = (byte)((p.rot[0] - 64) % 256);
            var targetdir = DirUtils.GetDirVector(targetkick, p.rot[1]);
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], yaw, p.rot[1]);
            Thread.Sleep(1000);
            PlayerHandlers.KickBall(p, (float)(targetdir.X * 0.4), 0, (float)(targetdir.Z * 0.4));
            p.SendPos(p.id, p.pos[0], p.pos[1], p.pos[2], targetkick, p.rot[1]);
        }
    }

    public class Chop : Skill
    {
        public override string Name { get { return "Chop"; } }
        public override string RightHandedSequence { get { return "i"; } }
        public override string LeftHandedSequence { get { return "y"; } }
        public override void ExecuteRight(Player p)
        {
            byte targetkick = (byte)((p.rot[0] + 32) % 256);
            var targetdir = DirUtils.GetDirVector(targetkick, p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(targetdir.X * 0.6), 0, (float)(targetdir.Z * 0.6), false);
        }
        public override void ExecuteLeft(Player p)
        {
            byte targetkick = (byte)((p.rot[0] - 32) % 256);
            var targetdir = DirUtils.GetDirVector(targetkick, p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(targetdir.X * 0.5), 0, (float)(targetdir.Z * 0.5), false);
        }
    }

    public class SimpleRainbow : Skill
    {
        public override string Name { get { return "Rainbow"; } }
        public override string RightHandedSequence { get { return "muu"; } }
        public override string LeftHandedSequence { get { return "muu"; } }
        public override void ExecuteRight(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * 0.4), 0.4f, (float)(dir.Z * 0.4));
        }
        public override void ExecuteLeft(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * 0.4), 0.4f, (float)(dir.Z * 0.4));
        }
    }

    public class SombreroFlick : Skill
    {
        public override string Name { get { return "Sombrero Flick"; } }
        public override string RightHandedSequence { get { return "uum"; } }
        public override string LeftHandedSequence { get { return "uum"; } }
        public override void ExecuteRight(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * -0.4), 0.4f, (float)(dir.Z * -0.4));
        }
        public override void ExecuteLeft(Player p)
        {
            var dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
            PlayerHandlers.KickBall(p, (float)(dir.X * -0.4), 0.4f, (float)(dir.Z * -0.4));
        }
    }
}
