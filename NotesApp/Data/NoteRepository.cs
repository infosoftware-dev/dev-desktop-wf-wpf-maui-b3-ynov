// Data/NoteRepository.cs
using Microsoft.Data.Sqlite;
using NotesApp.Models;

namespace NotesApp.Data
{
    public class NoteRepository
    {
        private readonly string _connectionString;

        public NoteRepository(string dbPath = "notes.db")
        {
            _connectionString = $"Data Source={dbPath}";
            Init();
        }

        private void Init()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS notes (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    titre TEXT NOT NULL,
                    contenu TEXT NOT NULL DEFAULT '',
                    date_creation TEXT NOT NULL
                )";
            cmd.ExecuteNonQuery();

            try
                {
                    cmd.CommandText = "ALTER TABLE notes ADD COLUMN categorie TEXT NOT NULL DEFAULT 'General'";
                    cmd.ExecuteNonQuery();
                }
            catch (SqliteException) { /* colonne deja presente */ }
        }

        public List<Note> GetAll()
        {
            var notes = new List<Note>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, titre, contenu, categorie, date_creation FROM notes ORDER BY date_creation DESC";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                notes.Add(new Note
                {
                    Id = r.GetInt32(0),
                    Titre = r.GetString(1),
                    Contenu = r.GetString(2),
                    Categorie = r.GetString(3),
                    DateCreation = DateTime.Parse(r.GetString(4))
                });
            }
            return notes;
        }

        public void Add(Note note)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO notes (titre, contenu, categorie, date_creation) 
                                VALUES ($titre, $contenu, $categorie, $date)";
            cmd.Parameters.AddWithValue("$titre", note.Titre);
            cmd.Parameters.AddWithValue("$contenu", note.Contenu);
            cmd.Parameters.AddWithValue("$categorie", note.Categorie);
            cmd.Parameters.AddWithValue("$date", note.DateCreation.ToString("o"));
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT last_insert_rowid()";
            note.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Update(Note note)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE notes SET titre=$titre, contenu=$contenu, categorie=$categorie WHERE id=$id";
            cmd.Parameters.AddWithValue("$id", note.Id);
            cmd.Parameters.AddWithValue("$titre", note.Titre);
            cmd.Parameters.AddWithValue("$contenu", note.Contenu);
            cmd.Parameters.AddWithValue("$categorie", note.Categorie);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM notes WHERE id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public List<Note> Search(string query)
        {
            var notes = new List<Note>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, titre, contenu, categorie, date_creation FROM notes 
                WHERE titre LIKE '%' || $q || '%' OR contenu LIKE '%' || $q || '%'
                ORDER BY date_creation DESC";
            cmd.Parameters.AddWithValue("$q", query);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                notes.Add(new Note
                {
                    Id = r.GetInt32(0),
                    Titre = r.GetString(1),
                    Contenu = r.GetString(2),
                    Categorie = r.GetString(3),
                    DateCreation = DateTime.Parse(r.GetString(4))
                });
            }
            return notes;
        }
    }
}