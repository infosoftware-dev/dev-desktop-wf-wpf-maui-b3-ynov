// Models/TodoItem.cs
namespace ToDoApp.Models
{
    public class TodoItem
    {
        /// <summary>Identifiant unique</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Titre de la tache</summary>
        public string Titre { get; set; } = "";

        /// <summary>Description detaillee (optionnel)</summary>
        public string Description { get; set; } = "";

        /// <summary>La tache est-elle terminee ?</summary>
        public bool EstTerminee { get; set; } = false;

        /// <summary>Date de creation</summary>
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}