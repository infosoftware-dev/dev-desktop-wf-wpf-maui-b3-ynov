
// Services/TodoService.cs
using System.Text.Json;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TodoService
    {
        // Chemin du fichier JSON ou on sauvegarde les taches
        // AppDataDirectory = dossier de donnees de l'app (different par OS)
        private readonly string _filePath;

        public TodoService()
        {
            _filePath = Path.Combine(
                AppContext.BaseDirectory, "todos.json");
        }

        /// <summary>
        /// Charge toutes les taches depuis le fichier JSON
        /// </summary>
        public List<TodoItem> GetAll()
        {
            if (!File.Exists(_filePath))
                return new List<TodoItem>();

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<TodoItem>>(json)
                   ?? new List<TodoItem>();
        }

        /// <summary>
        /// Sauvegarde la liste complete dans le fichier JSON
        /// </summary>
        public void SaveAll(List<TodoItem> todos)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(todos, options);
            File.WriteAllText(_filePath, json);
        }

        /// <summary>
        /// Ajoute une tache et sauvegarde
        /// </summary>
        public void Add(TodoItem item)
        {
            var todos = GetAll();
            todos.Add(item);
            SaveAll(todos);
        }

        /// <summary>
        /// Met a jour une tache existante (par son Id) et sauvegarde
        /// </summary>
        public void Update(TodoItem item)
        {
            var todos = GetAll();
            var index = todos.FindIndex(t => t.Id == item.Id);
            if (index >= 0)
            {
                todos[index] = item;
                SaveAll(todos);
            }
        }

        /// <summary>
        /// Supprime une tache par son Id et sauvegarde
        /// </summary>
        public void Delete(string id)
        {
            var todos = GetAll();
            todos.RemoveAll(t => t.Id == id);
            SaveAll(todos);
        }
    }
}
