﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Diagnostics;
using SML.Models;
using static SML.Models.Player;

namespace SML.DAL.Repositories {
    public class SeasonRepository : BaseRepository {

        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;

        public SeasonRepository(SqlConnection connection) {
            _connection = connection;
        }

        public void SetTransaction(SqlTransaction transaction) {
            _transaction = transaction;
        }

        // Season Data Table
        public DataTable GetSeasonData() {
            string query = @"
                SELECT 
                    s.season_name, 
                    s.status,
                    COUNT(p.player_id) AS player_count
                FROM 
                    Season s
                LEFT JOIN 
                    Player p ON p.season_id = s.season_id
                GROUP BY 
                    s.season_name, s.status";

            DataTable table = new DataTable();
            try {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, _connection)) {
                    adapter.Fill(table);
                }

                // Debugging: Check the data after filling the table
                Logger.Log("Fetched Rows: " + table.Rows.Count);
                foreach (DataRow row in table.Rows) {
                    Logger.Log($"Row Data - Name: {row["season_name"]}, Status: {row["status"]}, Players: {row["player_count"]}");
                }

            }
            catch (Exception ex) {
                Logger.Log("SQL Query Error: " + ex.Message);
            }

            return table;
        }

        // Season Objects
        public Season GetSeasonByID(int season_id) {
            Logger.Log($"GetSeason: {season_id}");
            Season season = null;
            try {
                string query = "SELECT season_id, season_name, status, unregistered_upload FROM Season WHERE season_id = @season_id";
                using SqlCommand command = new SqlCommand(query, _connection, _transaction);
                command.Parameters.AddWithValue("@season_id", season_id);
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) {
                    season = new Season(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3)
                    );
                }
            }
            catch (Exception ex) {
                Logger.Log($"Error fetching season: {ex.Message}");
            }

            Logger.Log("Returning season from GetSeason");
            Logger.Log(season.ToString());
            return season;
        }


        public DataTable GetPlayersFromSeason(string eventName) {
            string query = @"
                SELECT 
                    p.player_name,
                    d.division_name,
                    CASE 
                        WHEN p.forfeit = 1 THEN 'Forfeit'
                        ELSE 'Active'
                    END AS forfeit,
                    p.win,
                    p.tie,
                    p.loss
                    
                FROM 
                    Player p
                INNER JOIN 
                    Season s ON p.season_id = s.season_id
                INNER JOIN 
                    Division d ON p.division_id = d.division_id
                WHERE 
                    s.season_name = @SeasonName;";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(query, _connection)) {
                cmd.Parameters.AddWithValue("@SeasonName", eventName);  // Bind eventName to the parameter

                using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(table);
            }
            return table;
        }


        public List<Season> LoadSeasons() {
            Logger.Log($"LoadSeasons()");

            List<Season> seasons = new List<Season>();

            try {
                string query = "SELECT season_ID, season_name, status, unregistered_upload, scoreboard_visible FROM dbo.Season";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    seasons.Add(new Season(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3),
                        reader.GetInt32(4)
                    ));
                }
            }
            catch (Exception ex) {
                Logger.Log($"Error loading seasons: {ex.Message}");
            }

            return seasons;
        }


        public bool CheckSeasonName(string name) {
            try {
                string query = @"SELECT COUNT(*) FROM Season WHERE season_name = @name";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);
                command.Parameters.AddWithValue("@name", name);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex) {
                throw new Exception("Error checking season name.", ex);
            }
        }



        public List<Division> GetSeasonDivisions(int seasonID) {
            Logger.Log($"GetSeasonDivisions {seasonID}");

            List<Division> divisionList = new List<Division>();

            try {
                string query = "SELECT division_id, division_name, load_order FROM Division " +
                "WHERE season_id = @seasonID";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);

                command.Parameters.AddWithValue("@seasonID", seasonID);
                Logger.Log($"Fetching all divisions for seasonID:{seasonID}");

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    divisionList.Add(new Division {
                        SeasonID = seasonID,
                        DivisionID = reader.GetInt32(0),
                        DivisionName = reader.GetString(1),
                        LoadOrder = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    });
                }
            }
            catch (Exception ex) {
                Logger.Log($"Error fetching divisions for seasonID: {seasonID}\n{ex.Message}");
                throw;
            }

            return divisionList;
        }


        public List<Player> GetPlayersFromDivision(Division division) {
            Logger.Log($"GetPlayersFromDivision: {division.DivisionName}");

            if (division == null) throw new ArgumentNullException();
            Logger.Log($"Fetching players for seasonID:{division.SeasonID}\n divisionID:{division.DivisionID} division:{division.DivisionName}");

            List<Player> playersList = new List<Player>();

            string query = "SELECT player_id, player_name, forfeit, division_id, season_id, username, win, loss, tie, load_order, spy_shot, spy_mission_win, spy_civilian_shot, spy_timeout, sniper_shot, sniper_mission_win, sniper_civilian_shot, sniper_timeout " +
                "FROM Player WHERE season_id = @seasonID AND division_id = @divisionID";

            using SqlCommand command = new SqlCommand(query, _connection, _transaction);

            command.Parameters.AddWithValue("@seasonID", division.SeasonID);
            command.Parameters.AddWithValue("@divisionID", division.DivisionID);

            try {
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    playersList.Add(new Player {
                        PlayerID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Forfeit = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                        Division = reader.IsDBNull(3) ? -1 : reader.GetInt32(3),
                        Season = reader.IsDBNull(4) ? -1 : reader.GetInt32(4),
                        Username = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Wins = reader.GetInt32(6),
                        Losses = reader.GetInt32(7),
                        Ties = reader.GetInt32(8),
                        LoadOrder = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),

                        Results = new Results {
                            Spy_SpyShot = reader.GetInt32(10),
                            Spy_MissionsWin = reader.GetInt32(11),
                            Spy_CivilianShot = reader.GetInt32(12),
                            Spy_TimeOut = reader.GetInt32(13),
                            Sniper_SpyShot = reader.GetInt32(14),
                            Sniper_MissionsWin = reader.GetInt32(15),
                            Sniper_CivilianShot = reader.GetInt32(16),
                            Sniper_TimeOut = reader.GetInt32(17)
                        }
                    });
                }
            }
            catch (Exception ex) {
                Logger.Log($"Error fetching players from divisionID:{division.DivisionID}, seasonID:{division.SeasonID}\n{ex.Message}");
            }

            return playersList;
        }



        public void CreateSeason(string name, string password) {
            try {
                string query = @"INSERT INTO Season (season_name, season_password, status) 
                         VALUES (@name, @password, @status)";

                using SqlCommand command = new SqlCommand(query, _connection, _transaction);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@status", string.Empty);

                command.ExecuteNonQuery();
            }
            catch (Exception ex) {
                throw new Exception("Error inserting season: " + ex.Message, ex);
            }
        }


        public string VerifyPassword(string name) {
            try {
                string query = "SELECT season_password FROM Season WHERE season_name = @EventName";

                using SqlCommand cmd = new SqlCommand(query, _connection, _transaction);
                cmd.Parameters.AddWithValue("@EventName", name);
                string storedHash = cmd.ExecuteScalar()?.ToString();
                return storedHash;
            }
            catch (Exception ex) {
                throw ex;

            }
        }

    }
}
