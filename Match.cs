using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using MCGalaxy;
using MCGalaxy.Tasks;
using System.Threading.Tasks;

namespace FootballPlugin
{
    public enum MatchState : sbyte
    {
        InGame = 0,
        KickOff,
        GoalKick,
        Corner,
        ThrowIn,
        FreeKick,
        Penalty,
        NotStarted
    }

    public enum BallState : sbyte
    {
        InBounds = 0,
        NetLeft,
        NetRight,
        OutLeftTop,
        OutLeftBottom,
        OutRightTop,
        OutRightBottom,
        ThrowTop,
        ThrowBottom,
        NotStarted
    }

    public class Match
    {
        private float _timeEllapsed;
        public float TimeEllapsed
        {
            get
            {
                return _timeEllapsed;
            }
        }
        public Timer MatchTimer = new Timer(900000);
        public DateTime EndTime;
        private MatchState _state;
        public MatchState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        private Team _teamLeft;
        public Team TeamLeft
        {
            get
            {
                return _teamLeft;
            }
            set
            {
                _teamLeft = value;
            }
        }

        private Team _teamRight;
        public Team TeamRight
        {
            get
            {
                return _teamRight;
            }
            set
            {
                _teamRight = value;
            }
        }

        private Team _teamInPossession;
        public Team TeamInPossession
        {
            get
            {
                return _teamInPossession;
            }
            set
            {
                _teamInPossession = value;
            }
        }

        private Vector2d _deadBallLocation;
        public Vector2d DeadBallLocation
        {
            get
            {
                return _deadBallLocation;
            }
            set
            {
                _deadBallLocation = value;
            }
        }

        public Team OppositeTeam(Team aTeam)
        {
            if (_teamLeft == aTeam)
            {
                return _teamRight;
            }

            return _teamLeft;
        }

        private BallState DetermineBallState(Vector3d position)
        {
            // out overrides throw in
            if (position.X < Constants.GoalLineLeft)
            {
                if (position.Y < Constants.GoalTop)
                {
                    return BallState.OutLeftTop;
                }
                else if (position.Y < Constants.GoalBottom && position.Z < Constants.GoalHeight)
                {
                    return BallState.NetLeft;
                }
                else
                {
                    return BallState.OutLeftBottom;
                }
            }
            else if (position.X < Constants.GoalLineRight)
            {
                if (position.Y < Constants.TouchLineTop)
                {
                    return BallState.ThrowTop;
                }
                else if (position.Y < Constants.TouchLineBottom)
                {
                    return BallState.InBounds;
                }
                else
                {
                    return BallState.ThrowBottom;
                }
            }
            else
            {
                if (position.Y < Constants.GoalTop)
                {
                    return BallState.OutRightTop;
                }
                else if (position.Y < Constants.GoalBottom && position.Z < Constants.GoalHeight)
                {
                    return BallState.NetRight;
                }
                else
                {
                    return BallState.OutRightBottom;
                }
            }
        }
        public bool DetermineIfInBound(Vector3d position)
        {
            // out overrides throw in
            if (position.X < Constants.GoalLineLeft)
            {
                if (position.Y < Constants.GoalTop)
                {
                    return false;
                }
                else if (position.Y < Constants.GoalBottom && position.Z < Constants.GoalHeight)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (position.X < Constants.GoalLineRight)
            {
                if (position.Y < Constants.TouchLineTop)
                {
                    return false;
                }
                else if (position.Y < Constants.TouchLineBottom)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (position.Y < Constants.GoalTop)
                {
                    return false;
                }
                else if (position.Y < Constants.GoalBottom && position.Z < Constants.GoalHeight)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public void UnforcedEnd(object source = null, ElapsedEventArgs e = null)
        {
            End(false);
        }
        public void End(bool forced = true)
        {
            if (!forced)
                OnEnd();
            Team.Clear();
            started = false;
            Core.Match.State = MatchState.KickOff;
            MatchTimer.Stop();
            Chat.MessageAll("Restarting game");
            Core.Ball.PickedUp = null;
            Core.Ball.SpawnAt(Constants.CenterPoint);
            Barriers.OnSetPieceTake();
            Task.Run(() => KickOff(true));
        }
        public void OnEnd()
        {
            Team winner = null;
            if (Team.CurrentTeams[0].Goals > Team.CurrentTeams[1].Goals)
            {
                winner = Team.CurrentTeams[0];
            }
            if (Team.CurrentTeams[1].Goals > Team.CurrentTeams[0].Goals)
            {
                winner = Team.CurrentTeams[1];
            }
            if (winner != null)
                Chat.MessageAll(winner.Color + winner.Name + Server.DefaultColor + " won " + Team.CurrentTeams[0].Goals + "&a - " + Server.DefaultColor + Team.CurrentTeams[1].Goals);
            else
                Chat.MessageAll("It was a draw by " + Team.CurrentTeams[0].Goals + "&a - " + Server.DefaultColor + Team.CurrentTeams[1].Goals);
            foreach (Player p in Player.players)
            {
                var team = Team.FindPlayer(p);
                if (!(bool)p.ExtraData["Spec"])
                {
                    p.ExtraData.ChangeOrCreate("TotalRounds", (int)p.ExtraData["TotalRounds"] + 1);
                    p.SendMessage("+5 &a" + Server.moneys + Server.DefaultColor + " for Participation");
                    p.money += 5;
                }
                if (winner != null)
                {
                    if (team != null)
                        if (team == winner)
                        {
                            p.ExtraData.ChangeOrCreate("Wins", (int)p.ExtraData["Wins"] + 1);
                            p.SendMessage("+3 &a" + Server.moneys + Server.DefaultColor + " for Winning");
                            p.money += 3;
                        }
                        else
                            p.ExtraData.ChangeOrCreate("Losses", (int)p.ExtraData["Losses"] + 1);
                }
                else if (!(bool)p.ExtraData["Spec"])
                {
                    p.ExtraData.ChangeOrCreate("Draws", (int)p.ExtraData["Draws"] + 1);
                    p.SendMessage("+1 &a" + Server.moneys + Server.DefaultColor + " for Drawing");
                    p.money += 1;
                }
                Stats.SaveFootballStats(p);
            }
        }
        private void UpdateStateOfPlay()
        {
            #region in play
            if (!started)
                return;
            if (_state == MatchState.InGame)
            {
                // assuming the ball was in play last time we checked.
                BallState ballState = DetermineBallState(Core.Ball.Position3d);
                switch (ballState)
                {
                    case BallState.InBounds:
                        break;

                    case BallState.NetLeft:
                        Core.Ball.PickedUp = null;
                        _state = MatchState.KickOff;
                        Core.Ball.SpawnAt(Constants.CenterPoint);
                        _teamInPossession = _teamLeft;
                        TeamRight.Goals++;
                        if (Core.Ball.LastTouch == TeamLeft)
                        {
                            Chat.MessageAll("Magnificent OWN goal by Team " + TeamLeft.Color + TeamLeft.Name + " " + Server.DefaultColor + "(" + Core.Ball.LastTouchPlayer.ColoredName + Server.DefaultColor + ")");
                            Core.Ball.LastTouchPlayer.ExtraData.ChangeOrCreate("OwnGoals", (int)Core.Ball.LastTouchPlayer.ExtraData["OwnGoals"] + 1);
                        }
                        else
                        {
                            Chat.MessageAll("Magnificent goal by Team " + TeamRight.Color + TeamRight.Name + " " + Server.DefaultColor + "(" + Core.Ball.LastTouchPlayer.ColoredName + Server.DefaultColor + ")");
                            try
                            {
                                if (TeamRight.SecondLastTouch != null)
                                {
                                    Chat.MessageAll("Awesome assist by " + TeamRight.SecondLastTouch.ColoredName);
                                    TeamRight.SecondLastTouch.ExtraData.ChangeOrCreate("Assists", (int)TeamRight.SecondLastTouch.ExtraData["Assists"] + 1);
                                }
                            }
                            catch { }
                            Core.Ball.LastTouchPlayer.ExtraData.ChangeOrCreate("Goals", (int)Core.Ball.LastTouchPlayer.ExtraData["Goals"] + 1);
                        }
                        foreach (Player p in Player.players)
                            HUD.UpdateAll(p);
                        Task.Run(() => KickOff(true));
                        break;

                    case BallState.NetRight:
                        Core.Ball.PickedUp = null;
                        _state = MatchState.KickOff;
                        Core.Ball.SpawnAt(Constants.CenterPoint);
                        _teamInPossession = _teamRight;
                        TeamLeft.Goals++;
                        if (Core.Ball.LastTouch == TeamRight)
                        {
                            Player.GlobalMessage("Magnificent OWN goal by Team " + TeamRight.Color + TeamRight.Name + " " + Server.DefaultColor + "(" + Core.Ball.LastTouchPlayer.ColoredName + Server.DefaultColor + ")");
                            Core.Ball.LastTouchPlayer.ExtraData.ChangeOrCreate("OwnGoals", (int)Core.Ball.LastTouchPlayer.ExtraData["OwnGoals"] + 1);
                        }
                        else
                        {
                            Player.GlobalMessage("Magnificent goal by Team " + TeamLeft.Color + TeamLeft.Name + " " + Server.DefaultColor + "(" + Core.Ball.LastTouchPlayer.ColoredName + Server.DefaultColor + ")");
                            Core.Ball.LastTouchPlayer.ExtraData.ChangeOrCreate("Goals", (int)Core.Ball.LastTouchPlayer.ExtraData["Goals"] + 1);
                            try
                            {
                                if (TeamLeft.SecondLastTouch != null)
                                {
                                    Chat.MessageAll("Awesome assist by " + TeamRight.SecondLastTouch.ColoredName);
                                    TeamLeft.SecondLastTouch.ExtraData.ChangeOrCreate("Assists", (int)TeamLeft.SecondLastTouch.ExtraData["Assists"] + 1);
                                }
                            }
                            catch { }
                        }
                        foreach (Player p in Player.players)
                            HUD.UpdateAll(p);
                        Task.Run(() => KickOff(false));
                        break;

                    case BallState.OutLeftTop:
                        if (Core.Ball.LastTouch == _teamLeft)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[0];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                            Chat.MessageAll("Corner for Team " + _teamInPossession.Color + _teamInPossession.Name);
                            Barriers.HandleCorner();
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[0];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                            Chat.MessageAll("Goalkick for Team " + _teamInPossession.Color + _teamInPossession.Name);
                        }
                        break;
                    case BallState.OutLeftBottom:
                        if (Core.Ball.LastTouch == _teamLeft)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[1];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                            Chat.MessageAll("Corner for Team " + _teamInPossession.Color + _teamInPossession.Name);
                            Barriers.HandleCorner();
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[0];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                            Chat.MessageAll("Goalkick for Team " + _teamInPossession.Color + _teamInPossession.Name);
                        }
                        break;
                    case BallState.OutRightTop:
                        if (Core.Ball.LastTouch == _teamRight)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[2];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                            Chat.MessageAll("Corner for Team " + _teamInPossession.Color + _teamInPossession.Name);
                            Barriers.HandleCorner();
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[1];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                            Chat.MessageAll("Goalkick for Team " + _teamInPossession.Color + _teamInPossession.Name);
                        }
                        break;
                    case BallState.OutRightBottom:
                        if (Core.Ball.LastTouch == _teamRight)
                        {
                            _state = MatchState.Corner;
                            _deadBallLocation = Constants.CornerPoint[3];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamLeft;
                            Chat.MessageAll("Corner for Team " + _teamInPossession.Color + _teamInPossession.Name);
                            Barriers.HandleCorner();
                        }
                        else
                        {
                            _state = MatchState.GoalKick;
                            _deadBallLocation = Constants.GoalKickPoint[1];
                            Core.Ball.SpawnAt(_deadBallLocation);
                            _teamInPossession = _teamRight;
                            Chat.MessageAll("Goalkick for Team " + _teamInPossession.Color + _teamInPossession.Name);
                        }
                        break;
                    case BallState.ThrowTop:
                        _state = MatchState.ThrowIn;
                        _deadBallLocation = new Vector2d(Core.Ball.Position3d.X, Constants.TouchLineTop);
                        Core.Ball.SpawnAt(_deadBallLocation);

                        if (Core.Ball.LastTouch == _teamLeft)
                        {
                            _teamInPossession = _teamRight;
                        }
                        else
                        {
                            _teamInPossession = _teamLeft;
                        }
                        Chat.MessageAll("Throw In for Team " + _teamInPossession.Color + _teamInPossession.Name);
                        Barriers.HandleThrowIn();
                        break;
                    case BallState.ThrowBottom:
                        _state = MatchState.ThrowIn;
                        _deadBallLocation = new Vector2d(Core.Ball.Position3d.X, Constants.TouchLineBottom);
                        Core.Ball.SpawnAt(_deadBallLocation);
                        if (Core.Ball.LastTouch == _teamLeft)
                        {
                            _teamInPossession = _teamRight;
                        }
                        else
                        {
                            _teamInPossession = _teamLeft;
                        }
                        Chat.MessageAll("Throw In for Team " + _teamInPossession.Color + _teamInPossession.Name);
                        Barriers.HandleThrowIn();
                        break;
                }
            }
            #endregion
            else if (_teamInPossession.TakenSetPiece)
            {
                Barriers.OnSetPieceTake();
                Chat.MessageAll("Set piece taken by Team " + _teamInPossession.Color + _teamInPossession.Name);
                _teamInPossession.TakenSetPiece = false;
                _state = MatchState.InGame;
            }
        }

        public void Update()
        {
            _timeEllapsed += Constants.MatchTimeDelta;
            UpdateStateOfPlay();
        }
        public int PlayersNotSpec()
        {
            List<Player> players;
            try
            {
                players = Player.players.ToList();
                if (!players.Any(p => !(bool)p.ExtraData["Spec"]))
                    return 0;
                return players.Count(p => !(bool)p.ExtraData["Spec"]);
            }
            catch { return 0; }
        }
        public void KickOff(bool left)
        {
            while (PlayersNotSpec() < 2 && !started)
            {
                System.Threading.Thread.Sleep(1000);
            }

            System.Threading.Thread.Sleep(1000);
            if (!started)
            {
                _timeEllapsed = 0;
                Team.CurrentTeams.Clear();
                Team team1 = Team.GetRandomTeam();
                Team team2 = Team.GetRandomTeam();
                while (team1.Name == team2.Name)
                {
                    team2 = Team.GetRandomTeam();
                }
                Team.CurrentTeams[0] = team1;
                Team.CurrentTeams[1] = team2;
                Team.JoinAll();
                Team.CurrentTeams[0].PickCaptain();
                Team.CurrentTeams[1].PickCaptain();
                foreach (Player pl in Player.players)
                    HUD.UpdateAll(pl);
                // Chat.MessageAll("Kick off is in 45 seconds. Use this time to talk with your team");
                // System.Threading.Thread.Sleep(45000);
                _timeEllapsed = 0;
                started = true;
                MatchTimer.Start();
                EndTime = DateTime.Now.AddMilliseconds(900000);
                _teamLeft = Team.CurrentTeams[0];
                _teamRight = Team.CurrentTeams[1];
            }
            Team.SpawnAll();
            Core.Ball.PickedUp = null;
            Team.CurrentTeams[0].SecondLastTouch = null;
            Team.CurrentTeams[1].SecondLastTouch = null;
            Server.s.Log("Ball spawning");
            Core.Ball.SpawnAt(new Vector2d(Constants.CenterPoint.X + 16, Constants.CenterPoint.Y + 16));
            Server.s.Log("Ball spawned");
            _state = MatchState.KickOff;
            _teamInPossession = left ? _teamLeft : _teamRight;
            Chat.MessageAll("Kicking off. " + _teamInPossession.Color + _teamInPossession.Name + Server.DefaultColor + " is starting");
            Barriers.HandleKickOff();
        }
        public Player SetpieceTaker;
        public bool CanTouch(Player p)
        {
            if (!started && !(bool)p.ExtraData["Spec"])
                return true;
            var team = Team.FindPlayer(p);
            if (team == null)
                return false;
            //If a goalie picks up a ball and another player attempts to kick it
            if (Core.Ball.PickedUp != null && Core.Ball.PickedUp != p)
                return false;
            //Goalie picks up and kicks
            if (Core.Ball.PickedUp != null && Core.Ball.PickedUp == p && Core.Match.State == MatchState.InGame)
            {
                Core.Ball.PickedUp = null;
                return true;
            }
            //In game and no set piece checking
            if (SetpieceTaker == null && _state == MatchState.InGame)
                return true;
            if (team != null)
            {
                if (team.Starting.Count() < 2 && _state == MatchState.InGame)
                {
                    SetpieceTaker = null;
                    return true;
                }
            }
            //Set piece and not the team that's taking it
            if (_state != MatchState.InGame)
            {
                if (_teamInPossession != team)
                {
                    return false;
                }
                if (_teamInPossession == team)
                {
                    team.TakenSetPiece = true;
                    SetpieceTaker = p;
                    if (team.Starting.Count() < 2)
                    {
                        SetpieceTaker = null;
                    }
                    return true;
                }
            }
            //Set piece taker tries to kick without another player taking a touch
            if (_state == MatchState.InGame && SetpieceTaker == p)
            {
                return false;
            }
            if (_state == MatchState.InGame)
                SetpieceTaker = null;
            return true;
        }

        public bool started = false;
        public Match(Ball gameBall)
        {
            Core.Ball = gameBall;
            _timeEllapsed = 0;
            _state = MatchState.KickOff;
        }
    }
}
