using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCGalaxy;

namespace FootballPlugin
{
    public class Team
    {
        public static List<Team> CurrentTeams = new List<Team>(2);
        public string Name;
        public string Color;
        public List<Player> Starting;
        public List<Player> Substitutes;
        public byte Substitutions;
        public Player Captain;
        public Player Goalie;
        public Player SecondLastTouch;
        public int Goals;
        public bool TakenSetPiece;
        public string Skin;

        public Team(string name, string color, string skin = "")
        {
            Name = name;
            Color = color;
            Starting = new List<Player>(11);
            Substitutes = new List<Player>();
            Substitutions = 0;
            Goals = 0;
            CurrentTeams.Add(this);
            TakenSetPiece = false;
            Skin = skin;
        }

        public static void JoinAll()
        {
            Random r = new Random();

            foreach (Player p in PlayerInfo.Online.Items)
                if (!(bool)p.ExtraData["Spec"])
                    if (!(CurrentTeams[0].Substitutes.Contains(p) || CurrentTeams[1].Substitutes.Contains(p) || CurrentTeams[0].Starting.Contains(p) || CurrentTeams[1].Starting.Contains(p)))
                    {
                        if (CurrentTeams[0].Starting.Count + CurrentTeams[0].Substitutes.Count > CurrentTeams[1].Starting.Count + CurrentTeams[1].Substitutes.Count)
                            CurrentTeams[1].Join(p);
                        else if (CurrentTeams[0].Starting.Count + CurrentTeams[0].Substitutes.Count < CurrentTeams[1].Starting.Count + CurrentTeams[1].Substitutes.Count)
                            CurrentTeams[0].Join(p);
                        else if (CurrentTeams[0].Starting.Count + CurrentTeams[0].Substitutes.Count == CurrentTeams[1].Starting.Count + CurrentTeams[1].Substitutes.Count)
                            CurrentTeams[r.Next(2)].Join(p);
                    }
        }

        public void Join(Player p)
        {
            try
            {
                Starting.Add(p);
                Chat.MessageAll(p.ColoredName + Server.DefaultColor + " has joined Team: " + Color + Name + Server.DefaultColor + " in the Starting 11");
            }
            catch
            {
                Substitutes.Add(p);
                Chat.MessageAll(p.ColoredName + Server.DefaultColor + " has joined Team: " + Color + Name + Server.DefaultColor + " as a substitute");
            }
            p.ExtraData.ChangeOrCreate("Spec", false);
            p.SetPrefix();
            HUD.UpdateAll(p);
            HUD.UpdateTabForAll();
            SetSkin(p);
            Spawn(p);
        }

        public void Spawn(Player p)
        {

            if (Starting.Contains(p))
            {
                if (Core.Match.TeamLeft == this)
                    PlayerActions.MoveCoords(p, (ushort)Constants.GoalKickPoint[0].X / 32, 2, (ushort)Constants.GoalKickPoint[0].Y / 32, p.rot[0], p.rot[1]);
                else
                    PlayerActions.MoveCoords(p, (ushort)Constants.GoalKickPoint[1].X / 32, 2, (ushort)Constants.GoalKickPoint[1].Y / 32, p.rot[0], p.rot[1]);
            }
            else
            {
                Command.all.Find("spawn").Use(p, "");
            }
            p.SendMapMotd();
            //Command.all.Find("reveal").Use(p, "");
        }

        public static void SpawnAll()
        {
            foreach (Player p in Player.players)
            {
                var team = Team.FindPlayer(p);
                if (team != null)
                    team.Spawn(p);
            }
        }

        public static void SetSkin(Player p)
        {
            var team = FindPlayer(p);
            if (team == null)
                return;
            if (team.Skin != "" && team.Skin != null)
                p.SkinName = team.Skin;
            Entities.GlobalDespawn(p, true);
            Entities.GlobalSpawn(p, true);
        }
        public void PickCaptain()
        {
            Random r = new Random();
            Captain = Starting[r.Next(Starting.Count())];
            Goalie = Captain;
            Chat.MessageAll(Captain.ColoredName + Server.DefaultColor + " is the captain and goalie of Team: " + Color + Name);
            //Captain.SendMessage("You may change your team's properties (Goalie, Captain, Substitutions, etc) via /ct");
        }

        public static void Clear()
        {
            Team.CurrentTeams.Clear();
        }

        public static Team Find(string teamname)
        {
            Team match = null; int matches = 0;
            teamname = teamname.ToLower();

            foreach (Team team in CurrentTeams)
            {
                if (team.Name.ToLower() == teamname) return team;
                if (team.Name.ToLower().Contains(teamname))
                {
                    match = team; matches++;
                }
            }
            return matches == 1 ? match : null;
        }

        public static Team FindPlayer(Player p)
        {
            if (p == null || CurrentTeams == null)
                return null;
            foreach (Team team in CurrentTeams)
            {
                if (team.Starting.Contains(p) || team.Substitutes.Contains(p)) return team;
            }
            return null;
        }

        public string Substitute(Player substitute, Player substituted, bool bypass = false)
        {
            if (Substitutions >= 3 && !bypass)
                return "You've already used all your substitutions";
            if (!Substitutes.Contains(substitute))
                return "That player isn't a substitute";
            if (!Starting.Contains(substituted))
                return "That player isn't on the field for your team";
            Substitutions++;
            Substitutes.Remove(substitute);
            Starting.Remove(substituted);
            Substitutes.Add(substituted);
            Join(substitute);
            return substitute.ColoredName + Server.DefaultColor + " has substituted " + substituted.ColoredName;
        }

        public static void OnLeave(Player p)
        {
            var team = Team.FindPlayer(p);
            Random r = new Random();
            if (Core.Match.PlayersNotSpec() == 0)
                Core.Ball.SpawnAt(Constants.CenterPoint);
            if (team != null)
            {
                if (Core.Ball.PickedUp == p)
                {
                    Core.Ball.PickedUp = null;
                }
                if (team.SecondLastTouch == p)
                    team.SecondLastTouch = null;
                if (team.Starting.Contains(p) && team.Substitutes.Count != 0)
                    team.Substitute(team.Substitutes[r.Next(team.Substitutes.Count())], p, true);
                if (team.Starting.Contains(p))
                    team.Starting.Remove(p);
                if (team.Substitutes.Contains(p))
                    team.Substitutes.Remove(p);
                if (team.Starting.Count() == 0)
                {
                    Core.Match.End();
                }
                else
                {
                    if (team.Captain == p)
                        team.Captain = team.Starting[r.Next(team.Starting.Count())];
                    if (team.Goalie == p)
                        team.Goalie = team.Starting[r.Next(team.Starting.Count())];
                    foreach (Player pl in Player.players)
                        if (Team.FindPlayer(pl) == team)
                            HUD.UpdateAll(pl);
                }
            }
            HUD.UpdateTabForAll();
        }

        public static void OnJoin(Player p)
        {
            try
            {
                if (!(CurrentTeams[0].Substitutes.Contains(p) || CurrentTeams[1].Substitutes.Contains(p) || CurrentTeams[0].Starting.Contains(p) || CurrentTeams[1].Starting.Contains(p)))
                    if (CurrentTeams[0].Starting.Count + CurrentTeams[0].Substitutes.Count > CurrentTeams[1].Starting.Count + CurrentTeams[1].Substitutes.Count)
                        CurrentTeams[1].Join(p);
                    else
                        CurrentTeams[0].Join(p);
            }
            catch { }
            HUD.UpdateAll(p);
        }

        #region Teams
        public static PlayerExtList Teams;
        public static bool teamsLoaded = false;
        public static void AddTeam(string name, string color, string skinUrl)
        {
            Teams.AddOrReplace(name, color + " " + skinUrl);
            Teams.Save();
        }
        public static Team GetRandomTeam()
        {
            if (!teamsLoaded)
                LoadTeams();
            Random r = new Random();
            string line = Teams.AllLines()[r.Next(Teams.AllLines().Count())];
            string[] team = line.SplitSpaces(3);
            try
            {
                return new Team(team[0].Replace("%20", " "), team[1], team[2]);
            }
            catch
            {
                return new Team(team[0].Replace("%20", " "), team[1], "");
            }
        }
        public static void LoadTeams()
        {
            Teams = PlayerExtList.Load("teams.txt");
            teamsLoaded = true;
        }
        #endregion
    }
}
