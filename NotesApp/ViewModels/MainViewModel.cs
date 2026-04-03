// ViewModels/MainViewModel.cs
// Le ViewModel principal. Contient TOUTE la logique de l'application.
// La View (XAML) se lie a ses proprietes et ses commandes.
//
// C'est comme un "hook personnalise" en React qui contient
// tout le state et toutes les fonctions d'un composant.

using System.Collections.ObjectModel;
using System.Windows;
using NotesApp.Data;
using NotesApp.Models;
using System.Windows.Input;

namespace NotesApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly NoteRepository _repo = new();

        // =============================================
        // PROPRIETES LIEES AU XAML (via Data Binding)
        // =============================================

        // La liste de notes affichee dans la ListBox
        // ObservableCollection notifie l'UI quand on Add/Remove
        public ObservableCollection<Note> Notes { get; } = new();

        // Le titre saisi dans le formulaire
        private string _titre = "";
        public string Titre
        {
            get => _titre;
            set => SetProperty(ref _titre, value);
            // SetProperty fait : _titre = value + notifie le XAML
        }

        private string _categorie = "General";
        public string Categorie
        {
            get => _categorie;
            set => SetProperty(ref _categorie, value);
        }

        // Le contenu saisi dans le formulaire
        private string _contenu = "";
        public string Contenu
        {
            get => _contenu;
            set => SetProperty(ref _contenu, value);
        }

        // Le texte de recherche
        private string _recherche = "";
        public string Recherche
        {
            get => _recherche;
            set
            {
                if (SetProperty(ref _recherche, value))
                    Filtrer(); // Filtre automatiquement quand le texte change
            }
        }

        // La note selectionnee dans la liste
        private Note? _noteSelectionnee;
        public Note? NoteSelectionnee
        {
            get => _noteSelectionnee;
            set
            {
                if (SetProperty(ref _noteSelectionnee, value) && value != null)
                {
                    // Quand on selectionne une note, remplir le formulaire
                    Titre = value.Titre;
                    Contenu = value.Contenu;
                    Categorie = value.Categorie;
                }
            }
        }

        // Le texte du compteur en bas
        private string _compteur = "";
        public string Compteur
        {
            get => _compteur;
            set => SetProperty(ref _compteur, value);
        }

        // =============================================
        // COMMANDS (remplacent les evenements Click)
        // =============================================

        public ICommand AjouterCommand { get; }
        public ICommand ModifierCommand { get; }
        public ICommand SupprimerCommand { get; }
        public ICommand ViderCommand { get; }

        // =============================================
        // CONSTRUCTEUR
        // =============================================

        public MainViewModel()
        {
            // Initialiser les commands
            // Le 2e parametre (optionnel) desactive le bouton si la condition est fausse
            AjouterCommand = new RelayCommand(
                () => Ajouter(),
                () => !string.IsNullOrWhiteSpace(Titre)  // Bouton grise si titre vide
            );

            ModifierCommand = new RelayCommand(
                () => Modifier(),
                () => NoteSelectionnee != null  // Bouton grise si rien selectionne
            );

            SupprimerCommand = new RelayCommand(
                () => Supprimer(),
                () => NoteSelectionnee != null
            );

            ViderCommand = new RelayCommand(() => ViderFormulaire());

            // Charger les notes au demarrage
            ChargerNotes();
        }

        // =============================================
        // METHODES PRIVEES (la logique metier)
        // =============================================

        private void ChargerNotes()
        {
            Notes.Clear();
            foreach (var note in _repo.GetAll())
                Notes.Add(note);
            MajCompteur();
        }

        private void Ajouter()
        {
            var note = new Note
            {
                Titre = Titre.Trim(),
                Contenu = Contenu.Trim(),
                Categorie = Categorie.Trim(),
            };
            _repo.Add(note);
            Notes.Insert(0, note); // Ajouter en haut de la liste
            ViderFormulaire();
            MajCompteur();
        }

        private void Modifier()
        {
            if (NoteSelectionnee == null) return;
            if (string.IsNullOrWhiteSpace(Titre))
            {
                MessageBox.Show("Le titre est obligatoire !", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NoteSelectionnee.Titre = Titre.Trim();
            NoteSelectionnee.Contenu = Contenu.Trim();
            NoteSelectionnee.Categorie = Categorie.Trim();

            _repo.Update(NoteSelectionnee);
            ChargerNotes(); // Rafraichir la liste
            ViderFormulaire();
        }

        private void Supprimer()
        {
            if (NoteSelectionnee == null) return;

            var result = MessageBox.Show(
                $"Supprimer la note \"{NoteSelectionnee.Titre}\" ?",
                "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _repo.Delete(NoteSelectionnee.Id);
                Notes.Remove(NoteSelectionnee);
                ViderFormulaire();
                MajCompteur();
            }
        }

        private void Filtrer()
        {
            Notes.Clear();
            var resultats = string.IsNullOrEmpty(Recherche)
                ? _repo.GetAll()
                : _repo.Search(Recherche);
            foreach (var note in resultats)
                Notes.Add(note);
            MajCompteur();
        }

        private void ViderFormulaire()
        {
            Titre = "";
            Contenu = "";
            Categorie = "";
            NoteSelectionnee = null;
        }

        private void MajCompteur()
        {
            Compteur = $"{Notes.Count} note(s)";
        }
    }
}
