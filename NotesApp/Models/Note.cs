
// Models/Note.cs
// Le Model = les donnees pures. Pas de logique UI ici.
namespace NotesApp.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Contenu { get; set; } = "";
        public string Categorie { get; set; } = "";
        public DateTime DateCreation { get; set; } = DateTime.Now;

        /// <summary>
        /// Retourne les initiales du titre pour l'avatar
        /// Ex: "Mon projet" → "MP"
        /// </summary>
        public string Initiales
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Titre)) return "?";
                var mots = Titre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (mots.Length >= 2)
                    return $"{mots[0][0]}{mots[1][0]}".ToUpper();
                return Titre[0].ToString().ToUpper();
            }
        }

        /// <summary>
        /// Date formatee pour l'affichage
        /// </summary>
        public string DateFormatee => DateCreation.ToString("dd/MM/yyyy HH:mm");
    }
}
