using System;
using System.Data;
using MCGalaxy;
using MCGalaxy.SQL;

namespace FootballPlugin
{
    public struct FootballStats
    {
        public int TotalRounds, Wins, Losses, Draws, Assists, Goals, Saves, Skills, OwnGoals, Fouls;
    }

    public static class Stats
    {
        const string createSyntax =
            @"CREATE TABLE if not exists FootballStats (
ID INTEGER {0}{1} NOT NULL,
Name CHAR(20),
TotalRounds INT,
Wins INT,
Losses INT,
Draws INT,
Assists INT,
Goals INT,
Saves INT,
Skills INT,
OwnGoals INT,
Fouls INT
);";

        public static void CheckTableExists()
        {
            string primKey = ServerConfig.UseMySQL ? "" : "PRIMARY KEY ";
            string autoInc = ServerConfig.UseMySQL ? "AUTO_INCREMENT" : "AUTOINCREMENT";
            string primKey2 = ServerConfig.UseMySQL ? ", PRIMARY KEY (ID)" : "";
            Database.Execute(string.Format(createSyntax, primKey, autoInc, primKey2));
        }

        public static FootballStats LoadFootballStats(string name)
        {
            DataTable table = Database.Backend.GetRows("FootballStats", "*", "WHERE Name=@0", name);
            FootballStats stats = default(FootballStats);

            if (table.Rows.Count > 0)
            {
                DataRow row = table.Rows[0];
                stats.TotalRounds = int.Parse(row["TotalRounds"].ToString());
                stats.Wins = int.Parse(row["Wins"].ToString());
                stats.Losses = int.Parse(row["Losses"].ToString());
                stats.Draws = int.Parse(row["Draws"].ToString());
                stats.Goals = int.Parse(row["Goals"].ToString());
                stats.Assists = int.Parse(row["Assists"].ToString());
                stats.Saves = int.Parse(row["Saves"].ToString());
                stats.Skills = int.Parse(row["Skills"].ToString());
                stats.OwnGoals = int.Parse(row["OwnGoals"].ToString());
                stats.Fouls = int.Parse(row["Fouls"].ToString());
            }
            table.Dispose();
            return stats;
        }

        public static void SaveFootballStats(Player p)
        {
            DataTable table = Database.Backend.GetRows("FootballStats", "*", "WHERE Name=@0", p.name);
            string syntax = table.Rows.Count == 0 ?
                "INSERT INTO FootballStats (TotalRounds, Wins, Losses, Draws, Assists, Goals, Saves, Skills, OwnGoals, Fouls, Name) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10)"
                : "UPDATE FootballStats SET TotalRounds=@0, Wins=@1, Losses=@2, Draws=@3, Assists=@4, Goals=@5, Saves=@6, Skills=@7, OwnGoals=@8, Fouls=@9 WHERE Name=@10";
            Database.Execute(syntax, p.ExtraData["TotalRounds"], p.ExtraData["Wins"], p.ExtraData["Losses"], p.ExtraData["Draws"], p.ExtraData["Assists"], p.ExtraData["Goals"], p.ExtraData["Saves"], p.ExtraData["Skills"], p.ExtraData["OwnGoals"], p.ExtraData["Fouls"], p.name);
            table.Dispose();
        }
    }
}
