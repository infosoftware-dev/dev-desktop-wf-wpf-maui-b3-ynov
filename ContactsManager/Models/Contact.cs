// Models/Contact.cs
// Le modèle représente UN contact dans notre base de données.
// C'est l'équivalent d'un objet JavaScript :
// { id: 1, nom: "Jean", email: "jean@mail.com", telephone: "0612345678" }

namespace ContactManager.Models
{
    public class Contact
    {
        /// <summary>Identifiant unique (clé primaire en base de données)</summary>
        public int Id { get; set; }

        /// <summary>Nom complet du contact</summary>
        public string Nom { get; set; } = "";

        /// <summary>Adresse email</summary>
        public string Email { get; set; } = "";

        /// <summary>Numéro de téléphone</summary>
        public string Telephone { get; set; } = "";

        public string Categorie { get; set; } = "";
    }
}