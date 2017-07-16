using System;
using MCGalaxy;

namespace FootballPlugin
{
    public class Scheduler
    {
        public void Update()
        {
            Core.Ball.UpdateMotion3D();
            Core.Match.Update();
            if (Core.Ball != null && PlayerBot.Find("ball") != null)
            {
                Position Pos = new Position();
                Pos.X = (int)Core.Ball.Position3d.X;
                Pos.Y = (int)Core.Ball.Position3d.Z;
                Pos.Z = (int)Core.Ball.Position3d.Y;
                PlayerBot.Find("ball").Pos = Pos;
            }
        }
    }
}
