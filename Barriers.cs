using System;
using System.Collections.Generic;
using System.Linq;
using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;

namespace FootballPlugin
{
    public static class Barriers
    {
        static Dictionary<string, Vec3U16> Mappings;

        public const byte SetPieceCuboidID = 11;

        public static Vec3U16 MinBB, MaxBB;

        public static void HandleFreekick()
        {
            Core.Ball.PickedUp = null;
            int X = (int)(Core.Match.DeadBallLocation.X / 32);
            int Y = 1;
            int Z = (int)(Core.Match.DeadBallLocation.Y / 32);

            CreateBoundaries(X, Y, Z, 6);
        }
        public static void HandleKickOff()
        {
            Core.Ball.PickedUp = null;
            CreateBoundaries((int)(Constants.CenterPoint.X / 32), 1, (int)(Constants.CenterPoint.Y / 32), 11);
        }
        public static void HandleThrowIn()
        {
            Core.Ball.PickedUp = null;
            CreateBoundaries((int)(Core.Match.DeadBallLocation.X / 32), 1, (int)(Core.Match.DeadBallLocation.Y / 32), 4);
        }
        public static void HandleCorner()
        {
            Core.Ball.PickedUp = null;
            CreateBoundaries((int)(Core.Match.DeadBallLocation.X / 32), 1, (int)(Core.Match.DeadBallLocation.Y / 32), 7);
        }
        public static void HandlePenalty()
        {
            Core.Ball.PickedUp = null;
            CreateBoundaries(-1, -1, -1, -1);
        }

        public static void CreateBoundaries(int X, int Y, int Z, int radius)
        {
            if (!colorsAdded)
                AddMappings();
            if (X != -1)
            {
                MinBB.X = (ushort)Math.Max(0, X - radius);
                MinBB.Y = (ushort)Math.Max(0, Y - radius);
                MinBB.Z = (ushort)Math.Max(0, Z - radius);
                MaxBB.X = (ushort)Math.Min(Server.mainLevel.Width, X + radius);
                MaxBB.Y = (ushort)Math.Min(Server.mainLevel.Height, Y + radius);
                MaxBB.Z = (ushort)Math.Min(Server.mainLevel.Length, Z + radius);
            }
            else
            {
                if (Core.Match.TeamInPossession == Core.Match.TeamLeft)
                    GetPenaltyBoxRight();
                if (Core.Match.TeamInPossession == Core.Match.TeamRight)
                    GetPenaltyBoxLeft();
            }

            Vec3U16 col = Mappings[Core.Match.OppositeTeam(Core.Match.TeamInPossession).Color];

            foreach (Player player in Core.Match.OppositeTeam(Core.Match.TeamInPossession).Starting)
            {
                if (player.HasCpeExt("SelectionCuboid", 1))
                {
                    player.Send(Packet.MakeSelection(
                        SetPieceCuboidID,
                        "",
                        MinBB,
                        MaxBB,
                        (short)col.X,
                        (short)col.Y,
                        (short)col.Z,
                        128,
                        false
                        ));
                }
            }
        }
        public static void OnSetPieceTake()
        {
            List<Player> Playing = PlayerInfo.Online.Items.ToList();
            MinBB = new Vec3U16(0, 0, 0);
            MaxBB = new Vec3U16(0, 0, 0);
            foreach (Player player in Playing)
            {
                if (player.HasCpeExt("SelectionCuboid", 1))
                {
                    player.Send(Packet.MakeSelection(
                        SetPieceCuboidID, "", MinBB, MaxBB, 0, 0, 0, 0, false
                        ));
                }
            }
        }

        public static void HandleMovement(Player player, ushort x, ushort y, ushort z)
        {
            var team = Team.FindPlayer(player);
            if (team != null)
            {
                if (Core.Match.State == MatchState.Penalty && Core.Match.OppositeTeam(Core.Match.TeamInPossession).Goalie == player)
                {

                }
                else if (!(team == Core.Match.TeamInPossession) && InBounds(x / 32, y / 32, z / 32) && (bool)player.ExtraData["Teleported"])
                {
                    if (!Core.Match.DetermineIfInBound(new Vector3d((ushort)(MinBB.X - 1), (ushort)(MinBB.Z - 1), 1)))
                        PlayerActions.MoveCoords(player, MaxBB.X + 1, 1, MaxBB.Z + 1, player.rot[0], player.rot[1]);
                    else
                        PlayerActions.MoveCoords(player, MinBB.X - 1, 1, MinBB.Z - 1, player.rot[0], player.rot[1]);
                }
            }
        }

        public static bool InBounds(int X, int Y, int Z)
        {
            return X >= MinBB.X && Y >= MinBB.Y && Z >= MinBB.Z
                && X < MaxBB.X && Y < MaxBB.Y && Z < MaxBB.Z;
        }

        public static bool colorsAdded = false;
        static void AddMappings()
        {
            colorsAdded = true;
            Mappings = new Dictionary<string, Vec3U16>();
            Mappings.Add("&0", new Vec3U16(0, 0, 0));
            Mappings.Add("&1", new Vec3U16(0, 0, 191));
            Mappings.Add("&2", new Vec3U16(0, 191, 0));
            Mappings.Add("&3", new Vec3U16(0, 191, 191));
            Mappings.Add("&4", new Vec3U16(191, 0, 0));
            Mappings.Add("&5", new Vec3U16(191, 0, 191));
            Mappings.Add("&6", new Vec3U16(191, 191, 0));
            Mappings.Add("&7", new Vec3U16(191, 191, 191));
            Mappings.Add("&8", new Vec3U16(64, 64, 64));
            Mappings.Add("&9", new Vec3U16(64, 64, 255));
            Mappings.Add("&a", new Vec3U16(64, 255, 64));
            Mappings.Add("&b", new Vec3U16(64, 255, 255));
            Mappings.Add("&c", new Vec3U16(255, 255, 64));
            Mappings.Add("&d", new Vec3U16(255, 64, 255));
            Mappings.Add("&e", new Vec3U16(255, 255, 64));
            Mappings.Add("&f", new Vec3U16(255, 255, 255));
        }
        public static void GetPenaltyBoxLeft()
        {
            MinBB.X = (ushort)((Constants.GoalLineLeft / 32));
            MinBB.Y = 1;
            MinBB.Z = (ushort)(Constants.BoxTop / 32);
            MaxBB.X = (ushort)(Constants.BoxLeft / 32);
            MaxBB.Y = 5;
            MaxBB.Z = (ushort)(Constants.BoxBottom / 32);
        }
        public static void GetPenaltyBoxRight()
        {
            MinBB.X = (ushort)((Constants.BoxRight / 32));
            MinBB.Y = 1;
            MinBB.Z = (ushort)(Constants.BoxTop / 32);
            MaxBB.X = (ushort)(Constants.GoalLineRight / 32);
            MaxBB.Y = 5;
            MaxBB.Z = (ushort)(Constants.BoxBottom / 32);
        }
    }
}
