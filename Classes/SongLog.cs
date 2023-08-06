using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace Music_thing.Classes
{
    public static class SongLog
    {
        private const string dbname = "songlog.db";
        private const string externalDbs = "externaldbs";

        public async static void InitialiseDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(dbname, CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS SongKeys (SongKey NVARCHAR(2048) PRIMARY KEY NOT NULL); " +
                    "CREATE TABLE IF NOT EXISTS Listens (SongKey NVARCHAR(2048), Time integer, FOREIGN KEY(SongKey) REFERENCES SongKeys(SongKey));" +
                    "CREATE TABLE IF NOT EXISTS AlbumKeys (AlbumKey NVARCHAR(2048) PRIMARY KEY NOT NULL);" +
                    "CREATE TABLE IF NOT EXISTS AlbumStartListens (AlbumKey NVARCHAR(2048), Time integer, FOREIGN KEY(AlbumKey) REFERENCES AlbumKeys(AlbumKey));";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();

                //db.Close();
            }
        }

        public static void AddSong(string songkey)
        {
            if (!CheckIfSongExists(songkey))
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
                using (SqliteConnection db =
                  new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    insertCommand.CommandText = "INSERT INTO SongKeys VALUES (@Entry);";
                    insertCommand.Parameters.AddWithValue("@Entry", songkey);

                    insertCommand.ExecuteReader();

                    db.Close();
                }
            }
        }

        public static void AddListen(string songkey)
        {
            AddSong(songkey);

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT INTO Listens VALUES (@Entry, @Time);";
                insertCommand.Parameters.AddWithValue("@Entry", songkey);
                insertCommand.Parameters.AddWithValue("@Time", DateTime.Now.Ticks);
                var result = insertCommand.ExecuteReader();

                if (result.RecordsAffected == 1)
                {
                    SongListStorage.LastLoggedSong = songkey;
                }

                db.Close();
            }
        }

        private static bool CheckIfSongExists(string songid)
        {
            List<String> entries = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * from SongKeys where SongKey = @Entry", db);
                selectCommand.Parameters.AddWithValue("@Entry", songid);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries.Count != 0;
        }


        public static void AddAlbum(string albumkey)
        {
            if (!CheckIfAlbumExists(albumkey))
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
                using (SqliteConnection db =
                  new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    insertCommand.CommandText = "INSERT INTO AlbumKeys VALUES (@Entry);";
                    insertCommand.Parameters.AddWithValue("@Entry", albumkey);

                    insertCommand.ExecuteReader();

                    db.Close();
                }
            }
        }

        public static void AddAlbumListen(string albumkey)
        {
            AddAlbum(albumkey);

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT INTO AlbumListens VALUES (@Entry, @Time);";
                insertCommand.Parameters.AddWithValue("@Entry", albumkey);
                insertCommand.Parameters.AddWithValue("@Time", DateTime.Now.Ticks);
                insertCommand.ExecuteReader();

                db.Close();
            }
        }

        private static bool CheckIfAlbumExists(string albumkey)
        {
            List<String> entries = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * from AlbumKeys where AlbumKey = @Entry", db);
                selectCommand.Parameters.AddWithValue("@Entry", albumkey);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries.Count != 0;
        }

        /*private static List<String> Select(string selectcommand)
        {
            List<String> entries = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    (selectcommand, db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries;
        }*/

        public static string GetSongListenCount(string songkey)
        {
            List<string> dbs = new List<string>();
            string folderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, externalDbs);

            if (Directory.Exists(folderPath))
            {
                dbs = (from file in Directory.EnumerateFiles(folderPath) select file).ToList();
            }
            
            List<String> entries = new List<string>();

            string maindbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            dbs.Add(maindbpath);

            foreach (string dbpath in dbs)
            {
                using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand selectCommand = new SqliteCommand
                        ("SELECT COUNT(SongKey) from Listens where SongKey = @Entry", db);
                    selectCommand.Parameters.AddWithValue("@Entry", songkey);

                    SqliteDataReader query = selectCommand.ExecuteReader();

                    while (query.Read())
                    {
                        entries.Add(query.GetString(0));
                    }

                    db.Close();
                }
            }
            

            if (entries.Count != 0)
            {
                return entries[0];
            }
            return "0";
            
        }

        public static List<(string, string)> GetRecentListens(int count)
        {
            List<(string, string)> entries = new List<(string, string)>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * from Listens " +
                    "ORDER BY Time desc " +
                    "LIMIT @Count", db);
                selectCommand.Parameters.AddWithValue("@Count", count);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add((query.GetString(0), query.GetString(1)));
                }

                db.Close();
            }

            return entries;
        }
    }
}
