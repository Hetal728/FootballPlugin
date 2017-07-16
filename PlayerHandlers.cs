using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events;
using MCGalaxy.Events.EntityEvents;

namespace FootballPlugin
{
    public class PlayerHandlers
    {
        public static void InitHandlers()
        {
            OnPlayerConnectEvent.Register(OnConnect, Priority.High);
            OnPlayerDisconnectEvent.Register(OnLeave, Priority.High);
            OnPlayerMoveEvent.Register(OnMovement, Priority.High);
            OnPlayerClickEvent.Register(OnClick, Priority.High);
            OnPlayerChatEvent.Register(OnChat, Priority.High);
            OnTabListEntryAddedEvent.Register(HUD.GetTabName, Priority.High);
            OnPlayerActionEvent.Register(OnAFK, Priority.Normal);
        }

        static void OnChat(Player p, string message)
        {
            HUD.HandlesChatMessage(p, message);
        }

        static void OnAFK(Player p, PlayerAction action, string message, bool stealth)
        {
            if (action == PlayerAction.AFK && !((bool)p.ExtraData["Spec"]))
                Command.all.Find("spec").Use(p, null);
        }

        static void OnConnect(Player p)
        {
            FootballStats futstats = Stats.LoadFootballStats(p.name);
            p.ExtraData.ChangeOrCreate("TotalRounds", futstats.TotalRounds);
            p.ExtraData.ChangeOrCreate("Wins", futstats.Wins);
            p.ExtraData.ChangeOrCreate("Losses", futstats.Losses);
            p.ExtraData.ChangeOrCreate("Draws", futstats.Draws);
            p.ExtraData.ChangeOrCreate("Assists", futstats.Assists);
            p.ExtraData.ChangeOrCreate("Goals", futstats.Goals);
            p.ExtraData.ChangeOrCreate("Saves", futstats.Saves);
            p.ExtraData.ChangeOrCreate("OwnGoals", futstats.OwnGoals);
            if (futstats.Skills == 0)
                p.ExtraData.ChangeOrCreate("Skills", 1);
            else
                p.ExtraData.ChangeOrCreate("Skills", futstats.Skills);
            p.ExtraData.ChangeOrCreate("Fouls", futstats.Fouls);
            //Go ahead and create all the ExtraData variables so nothing goes wrong
            p.ExtraData.ChangeOrCreate("Speed", Constants.DefaultSpeed);
            p.ExtraData.ChangeOrCreate("OldSpeed", Constants.DefaultSpeed);
            p.ExtraData.ChangeOrCreate("InControl", 0);
            p.ExtraData.ChangeOrCreate("Spec", false);
            p.ExtraData.ChangeOrCreate("ShotPower", 1d);
            p.ExtraData.ChangeOrCreate("SpinY", 0d);
            p.ExtraData.ChangeOrCreate("SpinZ", 0d);
            p.ExtraData.ChangeOrCreate("SkillSequence", "");
            p.ExtraData.ChangeOrCreate("SkillTime", DateTime.Now);
            p.ExtraData.ChangeOrCreate("IsSkilling", false);
            p.ExtraData.ChangeOrCreate("Teleported", true);
            p.Send(Packet.TextHotKey("SpinYAdd", "/spinball y add◙", 200, 0, true));
            p.Send(Packet.TextHotKey("SpinYSub", "/spinball y sub◙", 208, 0, true));
            p.Send(Packet.TextHotKey("SpinZAdd", "/spinball z add◙", 205, 0, true));
            p.Send(Packet.TextHotKey("SpinZSub", "/spinball z sub◙", 203, 0, true));
            p.Send(Packet.TextHotKey("ShotPowerAdd", "/spinball p add◙", 13, 0, true));
            p.Send(Packet.TextHotKey("ShotPowerAdd", "/spinball p sub◙", 12, 0, true));
            p.Send(Packet.TextHotKey("SkillY", "/skill y◙", 21, 0, true));
            p.Send(Packet.TextHotKey("SkillH", "/skill h◙", 35, 0, true));
            p.Send(Packet.TextHotKey("SkillN", "/skill n◙", 49, 0, true));
            p.Send(Packet.TextHotKey("SkillM", "/skill m◙", 50, 0, true));
            p.Send(Packet.TextHotKey("Skill,", "/skill ,◙", 51, 0, true));
            p.Send(Packet.TextHotKey("SkillK", "/skill k◙", 37, 0, true));
            p.Send(Packet.TextHotKey("SkillI", "/skill i◙", 23, 0, true));
            p.Send(Packet.TextHotKey("SkillU", "/skill u◙", 22, 0, true));
            Team.OnJoin(p);
        }

        static void OnLeave(Player p, string reason)
        {
            Stats.SaveFootballStats(p);
            Team.OnLeave(p);
        }
        
        static void OnMovement(Player player, Position pos, byte yaw, byte pitch)
        {
            if ((bool)player.ExtraData["Spec"])
                return;
            if (Core.Match.State != MatchState.InGame)
            {
                Task.Run(() => Barriers.HandleMovement(player, (ushort)pos.X, (ushort)pos.Y, (ushort)pos.Z));
            }
            else
            {
                Task.Run(() => CheckInBox(player, (ushort)pos.X, (ushort)pos.Y, (ushort)pos.Z));
                Task.Run(() => Dribble(player, (ushort)pos.X, (ushort)pos.Y, (ushort)pos.Z, yaw, pitch));
            }
        }
        //Clean this stuff up later
        static void OnClick(Player player, MouseButton Button, MouseAction Action, ushort Yaw, ushort Pitch, byte EntityID, ushort X, ushort Y, ushort Z, TargetBlockFace target)
        {
            if ((bool)player.ExtraData["Spec"] || !Core.Match.started)
                return;
            if (Button == MouseButton.Right && Action == MouseAction.Pressed)
            {
                KickBall(player, Yaw, Pitch);
            }
            if (Button == MouseButton.Left && Action == MouseAction.Pressed && Core.Ball.PickedUp == null && Core.Match.State == MatchState.InGame)
                if (EntityID == PlayerBot.Find("ball").id && DistanceFromBall(player.pos[0], player.pos[1], player.pos[2]) <= 128)
                {
                    var team = Team.FindPlayer(player);
                    if (team != null && team.Goalie == player)
                    {
                        if (Core.Ball.LastTouchPlayer == player)
                        {
                            if (Team.FindPlayer(Core.Ball.SecondLastTouchPlayer) == team)
                                return;
                        }
                        else if (Team.FindPlayer(Core.Ball.LastTouchPlayer) == team)
                            return;
                        bool insideBox = false;
                        if (team == Core.Match.TeamLeft)
                            if (player.pos[0] <= Constants.BoxLeft && player.pos[2] <= Constants.BoxBottom && player.pos[2] >= Constants.BoxTop)
                            {
                                if (Core.Ball.Position3d.X <= Constants.BoxLeft && Core.Ball.Position3d.Y <= Constants.BoxBottom && Core.Ball.Position3d.Y >= Constants.BoxTop)
                                    insideBox = true;
                            }
                        if (team == Core.Match.TeamRight)
                            if (player.pos[0] >= Constants.BoxRight && player.pos[2] <= Constants.BoxBottom && player.pos[2] >= Constants.BoxTop)
                                if (Core.Ball.Position3d.X >= Constants.BoxRight && Core.Ball.Position3d.Y <= Constants.BoxBottom && Core.Ball.Position3d.Y >= Constants.BoxTop)
                                    insideBox = true;
                        if (insideBox)
                        {
                            Chat.MessageAll(player.ColoredName + Server.DefaultColor + " picks up the ball");
                            Core.Ball._velocity.X = 0;
                            Core.Ball._velocity.Y = 0;
                            Core.Ball._velocity.Z = 0;
                            Core.Ball.PickedUp = player;
                            player.ExtraData.ChangeOrCreate("Saves", (int)player.ExtraData["Saves"] + 1);
                        }
                        else
                        {
                            player.SendMessage("You need to be in the box to pick up the ball");
                        }
                    }
                }
        }
        #region Game-play
        public static void FirstTouch(Player player, byte yaw)
        {
            int VelX;
            int VelZ;
            DirUtils.EightYaw(yaw, out VelX, out VelZ);
            Core.Ball.Kick(new Vector3d(VelX * 4 * 2.5,
                    VelZ * 4 * 2.5, 0), new Vector3d(0, 0, 0), player);
            if ((int)player.ExtraData["InControl"] < 2)
                player.ExtraData.ChangeOrCreate("InControl", (int)player.ExtraData["InControl"] + 1);
        }
        public static void CheckInBox(Player player, ushort x, ushort y, ushort z)
        {
            var team = Team.FindPlayer(player);
            if (team != null && team.Goalie == player && Core.Ball.PickedUp == player)
            {
                bool insideBox = false;
                if (team == Core.Match.TeamLeft)
                    if (x <= Constants.BoxLeft && z <= Constants.BoxBottom && z >= Constants.BoxTop)
                        insideBox = true;
                if (team == Core.Match.TeamRight)
                    if (x >= Constants.BoxRight && z <= Constants.BoxBottom && z >= Constants.BoxTop)
                        insideBox = true;
                if (!insideBox)
                {
                    Core.Ball.PickedUp = null;
                    Chat.MessageAll(player.ColoredName + Server.DefaultColor + " dropped the ball");
                }
            }
        }
        public static Vector3d ZeroVelocity = new Vector3d(0, 0, 0);
        private static void Dribble(Player player, ushort x, ushort y, ushort z, byte yaw, byte pitch)
        {
            if (DateTime.Now > (DateTime)player.ExtraData["SkillTime"] && (string)player.ExtraData["SkillSequence"] != "")
            {
                SkillHandler.Execute(player, (string)player.ExtraData["SkillSequence"]);
                return;
            }
            if (Core.Match.State == MatchState.Penalty || (bool)player.ExtraData["IsSkilling"])
                return;
            if (DistanceFromBall(x, y, z) <= Constants.FirstTouchRadius && (Core.Match.State == MatchState.InGame || Core.Match.State == MatchState.KickOff) && Core.Ball.PickedUp == null)
            {
                if (Core.Ball.LastTouch == Team.FindPlayer(player) && Core.Ball.LastTouchPlayer != player)
                {
                    FirstTouch(player, yaw);
                    player.ExtraData.ChangeOrCreate("InControl", 2);
                    
                    /* Interferes in kicking
                    Server.Ball._velocity.X = 0;
                    Server.Ball._velocity.Y = 0;
                    Server.Ball._velocity.Z = 0;
                    */
                }
                Core.Ball.InControl = player;
                if ((int)player.ExtraData["InControl"] < 2)
                {
                    FirstTouch(player, yaw);
                }
                else if (Core.Match.CanTouch(player) && ((Core.Match.State == MatchState.InGame) || !Core.Match.started))
                {
                    var team = Team.FindPlayer(player);

                    if (Core.Ball.LastTouchPlayer != player)
                    {
                        Core.Ball.SecondLastTouchPlayer = Core.Ball.LastTouchPlayer;
                        if (Team.FindPlayer(Core.Ball.SecondLastTouchPlayer) == team)
                            team.SecondLastTouch = Core.Ball.SecondLastTouchPlayer;
                    }
                    Core.Ball.LastTouch = team;
                    Core.Ball.LastTouchPlayer = player;
                    double distance = 48;
                    double a = Math.Sin(((double)(128 - yaw) / 256) * 2 * Math.PI);
                    double b = Math.Cos(((double)(128 - yaw) / 256) * 2 * Math.PI);
                    double c = Math.Cos(((double)(pitch + 64) / 256) * 2 * Math.PI);
                    double d = Math.Cos(((double)(pitch) / 256) * 2 * Math.PI);
                    ushort X = (ushort)Math.Round(x + (double)(a * distance));
                    ushort Z = (ushort)Math.Round(z + (double)(b * distance));
                    Core.Ball._position3d.X = X;
                    Core.Ball._position3d.Y = Z;
                    if (Core.Ball.Velocity == ZeroVelocity)
                        player.ExtraData["Speed"] = Constants.DefaultSpeed - 0.5f;
                }
            }
            else
            {
                if ((int)player.ExtraData["InControl"] >= 2 && Core.Ball.InControl != player)
                {
                    player.ExtraData.ChangeOrCreate("Speed", 2.0f);
                    player.ExtraData.ChangeOrCreate("InControl", 0);
                    player.SendMessage("You lost control of the ball");
                }
            }
            if ((float)player.ExtraData["OldSpeed"] != (float)player.ExtraData["Speed"])
            {
                SetSpeed(player);
                player.ExtraData.ChangeOrCreate("OldSpeed", player.ExtraData["Speed"]);
            }
        }
        #endregion
        #region Speed
        public static void SetSpeed(Player p)
        {
            string motd = "-hax horspeed=" + (float)p.ExtraData["Speed"];
            p.Send(MakeMotd(p, motd));
        }

        public static byte[] MakeMotd(Player p, string motd)
        {
            byte[] buffer = new byte[131];
            buffer[0] = Opcode.Handshake;
            buffer[1] = Server.version;

            bool cp437 = p.HasCpeExt(CpeExt.FullCP437);
            if (motd.Length > 64)
            {
                NetUtils.Write(motd, buffer, 2, cp437);
                NetUtils.Write(motd.Substring(64), buffer, 66, cp437);
            }
            else
            {
                NetUtils.Write(Server.SoftwareName, buffer, 2, cp437);
                NetUtils.Write(motd, buffer, 66, cp437);
            }

            buffer[130] = Block.canPlace(p, Block.blackrock) ? (byte)100 : (byte)0;
            return buffer;
        }
        #endregion
        #region Distance
        struct Point3D
        {
            public ushort X, Y, Z;
            public Point3D(ushort x, ushort y, ushort z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        static double Distance(Point3D pointA, Point3D pointB)
        {
            double xDiff = Math.Max(pointA.X, pointB.X) - Math.Min(pointA.X, pointB.X);
            double yDiff = Math.Max(pointA.Y, pointB.Y) - Math.Min(pointA.Y, pointB.Y);
            double zDiff = Math.Max(pointA.Z, pointB.Z) - Math.Min(pointA.Z, pointB.Z);

            if (xDiff == 0 && yDiff == 0)
            {
                return zDiff;
            }
            else if (xDiff == 0 && zDiff == 0)
            {
                return yDiff;
            }
            else if (yDiff == 0 && zDiff == 0)
            {
                return xDiff;
            }

            double xyDist = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));
            double xyzDist = Math.Sqrt((xyDist * xyDist) + (zDiff * zDiff));
            return Math.Floor(xyzDist);
        }

        public static double DistanceFromBall(ushort x, ushort y, ushort z)
        {
            double xDiff = Math.Max(x, Core.Ball.Position3d.X) - Math.Min(x, Core.Ball.Position3d.X);
            double yDiff = Math.Max(y, Core.Ball.Position3d.Z) - Math.Min(y, Core.Ball.Position3d.Z);
            double zDiff = Math.Max(z, Core.Ball.Position3d.Y) - Math.Min(z, Core.Ball.Position3d.Y);

            if (xDiff == 0 && yDiff == 0)
            {
                return zDiff;
            }
            else if (xDiff == 0 && zDiff == 0)
            {
                return yDiff;
            }
            else if (yDiff == 0 && zDiff == 0)
            {
                return xDiff;
            }

            double xyDist = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));
            double xyzDist = Math.Sqrt((xyDist * xyDist) + (zDiff * zDiff));
            return Math.Floor(xyzDist);
        }

        public static double DistanceFromClosestOpposition(Player player, ushort x, ushort y, ushort z)
        {
            double shortestDist = 0;
            Player closestPlayer = null;
            var team = Team.FindPlayer(player);
            Team opposition = null;
            if (team == Team.CurrentTeams[0])
                opposition = Team.CurrentTeams[1];
            if (team == Team.CurrentTeams[1])
                opposition = Team.CurrentTeams[0];
            if (opposition == null)
                return 64;
            foreach (Player pl in opposition.Starting)
            {
                Point3D pPos = new Point3D(x, y, z);
                Point3D plPos = new Point3D(pl.pos[0], pl.pos[1], pl.pos[2]);
                double dist = Distance(pPos, plPos);

                if (closestPlayer == null || dist < shortestDist)
                {
                    closestPlayer = pl;
                    shortestDist = dist;
                }
            }
            if (closestPlayer == null)
                //Out of range of fouls
                return 64;
            return shortestDist;
        }
        public static bool BallInControl(ushort x, ushort y, ushort z)
        {
            if (DistanceFromBall(x, y, z) <= Constants.BallControlRadius)
                return true;
            return false;
        }
        #endregion

        #region Fouls
        public static bool Foul(Player player, ushort x, ushort y, ushort z)
        {
            if (DistanceFromClosestOpposition(player, x, y, z) <= 48)
            {
                var team = Team.FindPlayer(player);
                Team opposition = Core.Match.OppositeTeam(team);
                bool insideBox = false;
                if (opposition == Core.Match.TeamRight)
                    if (player.pos[0] <= Constants.BoxLeft && player.pos[2] <= Constants.BoxBottom && player.pos[2] >= Constants.BoxTop)
                        insideBox = true;
                if (opposition == Core.Match.TeamLeft)
                    if (player.pos[0] >= Constants.BoxRight && player.pos[2] <= Constants.BoxBottom && player.pos[2] >= Constants.BoxTop)
                        insideBox = true;
                Core.Match.TeamInPossession = opposition;
                if (insideBox)
                {
                    if (opposition.Name == Core.Match.TeamRight.Name)
                    {
                        Core.Match.DeadBallLocation = Constants.PenaltyLeft;
                        Chat.MessageAll("Penalty for Team " + opposition.Color + opposition.Name + "(" + player.ColoredName + Server.DefaultColor + ")");
                    }
                    if (opposition.Name == Core.Match.TeamLeft.Name)
                    {
                        Core.Match.DeadBallLocation = Constants.PenaltyRight;
                        Chat.MessageAll("Penalty for Team " + opposition.Color + opposition.Name + "(" + player.ColoredName + Server.DefaultColor + ")");
                    }
                    Barriers.HandlePenalty();
                    Core.Match.State = MatchState.Penalty;
                }
                else
                {
                    if (!PlayAdvantage(player, team, x, y, z))
                    {
                        Core.Match.DeadBallLocation = new Vector2d(player.pos[0], player.pos[2]);
                        Chat.MessageAll("Free Kick for Team " + opposition.Color + opposition.Name + Server.DefaultColor + " (" + player.ColoredName + Server.DefaultColor + ")");
                        Barriers.HandleFreekick();
                        Core.Match.State = MatchState.FreeKick;
                    }
                    else
                    {
                        Chat.MessageAll("Advantage played for " + opposition.Color + opposition.Name + Server.DefaultColor + " (" + player.ColoredName + Server.DefaultColor + ")");
                        player.ExtraData.ChangeOrCreate("Fouls", (int)player.ExtraData["Fouls"] + 1);
                        return false;
                    }
                }
                player.ExtraData.ChangeOrCreate("Fouls", (int)player.ExtraData["Fouls"] + 1);
                Core.Ball.SpawnAt(Core.Match.DeadBallLocation);
                team.Goalie.ExtraData.ChangeOrCreate("Teleported", false);
                if (opposition.Name == Core.Match.TeamLeft.Name)
                {
                    PlayerActions.MoveCoords(team.Goalie, (int)(Constants.GoalLineRight / 32), 1, (int)((Constants.BoxTop + Constants.BoxBottom) / 64), team.Goalie.rot[0], team.Goalie.rot[1]);
                }
                if (opposition.Name == Core.Match.TeamRight.Name)
                {
                    PlayerActions.MoveCoords(team.Goalie, (int)(Constants.GoalLineLeft / 32), 1, (int)((Constants.BoxTop + Constants.BoxBottom) / 64), team.Goalie.rot[0], team.Goalie.rot[1]);
                }
                Server.Background.QueueOnce(OnFoul);
                Thread.Sleep(500);
                team.Goalie.ExtraData.ChangeOrCreate("Teleported", true);
                return true;
            }
            return false;
        }

        public static void OnFoul(SchedulerTask task)
        {
            Core.CanTakeKick = false;
            Core.Ball.PickedUp = null;
            Thread.Sleep(5000);
            Core.CanTakeKick = true;
            Chat.MessageAll("Play resumed");
        }

        public static bool PlayAdvantage(Player Fouler, Team FoulingTeam, ushort x, ushort y, ushort z)
        {
            if (FoulingTeam == Core.Match.TeamInPossession)
                return false;
            if (DistanceFromClosestOpposition(Core.Ball.LastTouchPlayer, x, y, z) > 48)
                return true;
            return false;
        }
        #endregion

        #region Kicking
        public static void KickBall(Player p, ushort Yaw, ushort Pitch)
        {
            Yaw = (byte)(Yaw >> 8);
            Pitch = (byte)(Pitch >> 8);
            var Dir = DirUtils.GetDirVector((byte)Yaw, (byte)Pitch);
            KickBall(p, Dir.X, Dir.Y, Dir.Z);
        }
        Vec3F32 MaxDir = DirUtils.GetDirVector(255, 255);
        public static void KickBall(Player p, float X, float Y, float Z, bool spin = true)
        {
            if (Core.Match.State == MatchState.InGame && Core.Ball.PickedUp != p && Core.Match.started)
            {
                if (BallInControl(p.pos[0], p.pos[1], p.pos[2]) && (int)p.ExtraData["InControl"] != 0)
                {
                    KickBallTowardMouse(p, X, Z, Y, spin);
                }
                else if ((int)p.ExtraData["InControl"] == 0)
                    Foul(p, p.pos[0], p.pos[1], p.pos[2]);
            }
            else if (BallInControl(p.pos[0], p.pos[1], p.pos[2]))
            {
                KickBallTowardMouse(p, X, Z, Y, spin);
            }
        }
        /// <summary>
        /// Kicks the ball toward the mouse with the height collected in the height accumulator.
        /// </summary>
        private static void KickBallTowardMouse(Player p, float X, float Y, float Z, bool _spin = true)
        {
            if (Core.Match.State == MatchState.FreeKick || Core.Match.State == MatchState.Penalty)
                if (!Core.CanTakeKick)
                {
                    p.SendMessage("Please wait till play resumes");
                    return;
                }
            { }
            float scale = 16 * 2.5f;
            double ShotPower = (double)p.ExtraData["ShotPower"];
            double SpinY = (double)p.ExtraData["SpinY"];
            double SpinZ = (double)p.ExtraData["SpinZ"];
            if (Core.Match.State == MatchState.ThrowIn)
                scale = 12 * 2.5f;
            if (Core.Match.State != MatchState.FreeKick && Core.Match.State != MatchState.InGame)
            {
                ShotPower = 1;
            }
            Vector3d velocity = new Vector3d(X * scale * ShotPower, Y * scale * ShotPower, Z* scale * ShotPower);
            Vector3d spin = new Vector3d(0, 0, 0);

            if (_spin)
                spin = new Vector3d(0, SpinY * 2.5, SpinZ * 2.5);

            Core.Ball.Kick(velocity, spin, p);
        }
        #endregion
    }
}
