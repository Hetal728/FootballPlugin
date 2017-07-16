using System.Data;
using System.Threading;
using MCGalaxy;
using MCGalaxy.Maths;

namespace FootballPlugin
{
    public static class Skills
    {
        public static void Execute(Player p, int skill)
        {
            try
            {
                string line;
                var Dir = DirUtils.GetDirVector(p.rot[0], p.rot[1]);
                var MaxDir = DirUtils.GetDirVector(255, 255);
                // Read the file and display it line by line.  
                System.IO.StreamReader file =
                    new System.IO.StreamReader("skills/" + p.name.ToLower() + skill.ToString() + ".txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("Kick("))
                    {
                        //Kick(X, Y, Z);
                        var line_ = line.Replace("Kick(", "").Replace(");", "").Replace(" ", "").Replace("X", Dir.X.ToString())
                            .Replace("Y", Dir.Y.ToString()).Replace("Z", Dir.Z.ToString()).Split(',');
                        var X = Evaluate(line_[0]);
                        var Y = Evaluate(line_[1]);
                        var Z = Evaluate(line_[2]);
                        PlayerHandlers.KickBall(p, X, Y, Z);
                        Thread.Sleep(500);
                    }
                    //Kick X Y Z
                    if (line.StartsWith("Kick "))
                    {
                        //Kick(X, Y, Z);
                        var line_ = line.Replace("Kick ", "").Replace(");", "").Replace(" ", "").Replace("X", Dir.X.ToString())
                            .Replace("Y", Dir.Y.ToString()).Replace("Z", Dir.Z.ToString()).Split(',');
                        var X = Evaluate(line_[0]);
                        var Y = Evaluate(line_[1]);
                        var Z = Evaluate(line_[2]);
                        PlayerHandlers.KickBall(p, X, Y, Z);
                        Thread.Sleep(500);
                    }
                }
            }
            catch
            {
                p.SendMessage("Error in Skill #" + skill.ToString());
            }
        }
        public static float Evaluate(string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("expression", typeof(string), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            return float.Parse((string)row["expression"]);
        }
    }
}
