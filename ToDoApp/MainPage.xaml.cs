// MainPage.xaml.cs — Logique de la page principale
using System.Collections.ObjectModel;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp
{
    public partial class MainPage : ContentPage
    {
        // Le service gere la lecture/ecriture des donnees
        private readonly TodoService _service = new();

        // ObservableCollection = meme chose qu'en WPF
        // Notifie l'UI quand on ajoute/supprime
        private ObservableCollection<TodoItem> _todos = new();

        public MainPage()
        {
            InitializeComponent();
            ChargerTaches();
        }

        /// <summary>
        /// Charge toutes les taches depuis le fichier JSON
        /// et les affiche dans la CollectionView
        /// </summary>
        private void ChargerTaches()
        {
            var liste = _service.GetAll();
            _todos = new ObservableCollection<TodoItem>(liste);

            // ItemsSource = equivalent de ItemsSource en WPF
            listeTaches.ItemsSource = _todos;

            MettreAJourCompteur();
        }

        /// <summary>
        /// Ajouter une nouvelle tache
        /// </summary>
        private void BtnAjouter_Clicked(object? sender, EventArgs e)
        {
            string titre = txtNouvelleTache.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(titre))
            {
                // DisplayAlert = MessageBox en WPF = alert() en JS
                DisplayAlert("Champ vide", "Entrez un titre pour la tache", "OK");
                return;
            }

            var item = new TodoItem { Titre = titre };

            _service.Add(item);
            _todos.Add(item);

            // Vider le champ de saisie
            txtNouvelleTache.Text = "";

            MettreAJourCompteur();
        }

        /// <summary>
        /// Supprimer une tache (le bouton X rouge)
        /// CommandParameter contient l'Id de la tache
        /// </summary>
        private async void BtnSupprimer_Clicked(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            string? id = btn.CommandParameter as string;
            if (id == null) return;

            // Demander confirmation
            bool confirm = await DisplayAlert(
                "Confirmer",
                "Supprimer cette tache ?",
                "Oui", "Non");

            if (!confirm) return;

            _service.Delete(id);

            var item = _todos.FirstOrDefault(t => t.Id == id);
            if (item != null) _todos.Remove(item);

            MettreAJourCompteur();
        }

        /// <summary>
        /// Quand une checkbox change (tache terminee ou pas)
        /// </summary>
        private void CheckBox_Changed(object? sender, CheckedChangedEventArgs e)
        {
            if (sender is not CheckBox cb) return;

            // Recuperer le TodoItem lie a cette checkbox
            // via le BindingContext (= le DataContext en WPF)
            if (cb.BindingContext is not TodoItem item) return;

            item.EstTerminee = cb.IsChecked;
            _service.Update(item);

            MettreAJourCompteur();
        }

        /// <summary>
        /// Met a jour le compteur en bas de la page
        /// </summary>
        private void MettreAJourCompteur()
        {
            int total = _todos.Count;
            int terminees = _todos.Count(t => t.EstTerminee);
            lblCount.Text = $"{terminees}/{total} tache(s) terminee(s)";
        }
    }
}