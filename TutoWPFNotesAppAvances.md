# TUTO  : WPF Avance — MVVM, Commands, DataTemplates

> **Developpement Desktop .NET** — YNOV B3 — 2025/2026  
> **Duree estimee** : 2h00  
> **Niveau** : Intermediaire-Avance  
> **Prerequis** : avoir fait le Tuto 2 (Partie A + B) — savoir creer un projet WPF avec XAML, Grid, evenements, SQLite

---

## Objectifs

- Comprendre et implementer le pattern **MVVM** (Model-View-ViewModel)
- Creer une classe **BaseViewModel** avec `INotifyPropertyChanged`
- Creer des **RelayCommand** (remplacer les evenements Click par des Commands)
- Utiliser les **DataTemplates** pour personnaliser l'affichage d'une liste
- Comprendre le **DataContext** (le lien entre la View et le ViewModel)
- Refactorer une app WPF "code-behind" vers une architecture MVVM propre

---

## Pourquoi MVVM ?

Dans le Tuto 2, on a mis toute la logique dans `MainWindow.xaml.cs` (le code-behind). Ca fonctionne, mais ca pose des problemes :

| Probleme | Consequence |
|----------|------------|
| Tout est dans un fichier | Difficile a lire quand l'app grandit |
| La logique est collee a l'UI | Impossible de tester sans lancer la fenetre |
| Pas de separation des responsabilites | Un dev qui modifie l'UI risque de casser la logique |

**MVVM** resout ca en separant en 3 couches :

| Couche | Role | Equivalent React |
|--------|------|-----------------|
| **Model** | Les donnees pures (classes C#) | Les types/interfaces TypeScript |
| **View** | L'interface XAML (ce que l'utilisateur voit) | Le JSX |
| **ViewModel** | La logique + l'etat (entre les deux) | Les hooks (useState, useEffect) |

---

## Ce qu'on va construire

On va creer un **gestionnaire de notes** (mini Notion) avec architecture MVVM propre :

- Liste de notes avec titre + contenu + date
- Ajouter, modifier, supprimer des notes
- Affichage personnalise avec DataTemplate (carte avec avatar colore)
- Recherche en temps reel
- Tout en MVVM : zero logique dans le code-behind

---

## Etape 1 : Creer le projet

```powershell
cd ~/dev-desktop
dotnet new wpf -n NotesApp
cd NotesApp
dotnet add package Microsoft.Data.Sqlite
code .
```

### Structure cible du projet

```
NotesApp/
├── Models/
│   └── Note.cs                  ← le modele de donnees
├── Data/
│   └── NoteRepository.cs        ← acces SQLite
├── ViewModels/
│   ├── BaseViewModel.cs         ← classe de base MVVM
│   ├── RelayCommand.cs          ← implementation de ICommand
│   └── MainViewModel.cs         ← le ViewModel principal
├── MainWindow.xaml               ← la View (XAML)
├── MainWindow.xaml.cs            ← code-behind minimal (juste le DataContext)
├── App.xaml
└── NotesApp.csproj
```

Creez les dossiers :

```powershell
mkdir Models
mkdir Data
mkdir ViewModels
```

---

## Etape 2 : Le Model

```powershell
# Creez Models/Note.cs
```

```csharp
// Models/Note.cs
// Le Model = les donnees pures. Pas de logique UI ici.
namespace NotesApp.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Contenu { get; set; } = "";
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
```

---

## Etape 3 : Le Repository SQLite

```csharp
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
        }

        public List<Note> GetAll()
        {
            var notes = new List<Note>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, titre, contenu, date_creation FROM notes ORDER BY date_creation DESC";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                notes.Add(new Note
                {
                    Id = r.GetInt32(0),
                    Titre = r.GetString(1),
                    Contenu = r.GetString(2),
                    DateCreation = DateTime.Parse(r.GetString(3))
                });
            }
            return notes;
        }

        public void Add(Note note)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO notes (titre, contenu, date_creation) 
                                VALUES ($titre, $contenu, $date)";
            cmd.Parameters.AddWithValue("$titre", note.Titre);
            cmd.Parameters.AddWithValue("$contenu", note.Contenu);
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
            cmd.CommandText = "UPDATE notes SET titre=$titre, contenu=$contenu WHERE id=$id";
            cmd.Parameters.AddWithValue("$id", note.Id);
            cmd.Parameters.AddWithValue("$titre", note.Titre);
            cmd.Parameters.AddWithValue("$contenu", note.Contenu);
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
            cmd.CommandText = @"SELECT id, titre, contenu, date_creation FROM notes 
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
                    DateCreation = DateTime.Parse(r.GetString(3))
                });
            }
            return notes;
        }
    }
}
```

---

## Etape 4 : BaseViewModel — le moteur du Data Binding

C'est le coeur de MVVM. Cette classe permet au XAML de detecter automatiquement quand une propriete change.

```csharp
// ViewModels/BaseViewModel.cs
// Classe de base pour tous les ViewModels.
// Implemente INotifyPropertyChanged pour que le Data Binding fonctionne.
//
// ANALOGIE : c'est comme un "useState" en React.
// Quand on appelle SetProperty(), ca notifie l'UI de se re-rendre,
// exactement comme setCount() re-rend le composant React.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NotesApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Cet evenement est ecoute par le XAML.
        // Quand on le declenche, le XAML relit la propriete qui a change.
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifie l'UI qu'une propriete a change.
        /// [CallerMemberName] detecte automatiquement le nom de la propriete appelante.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Met a jour un champ ET notifie l'UI.
        /// C'est le "setter magique" de MVVM.
        /// 
        /// Utilisation :
        ///   private string _nom;
        ///   public string Nom {
        ///       get => _nom;
        ///       set => SetProperty(ref _nom, value);
        ///   }
        /// 
        /// C'est comme :
        ///   const [nom, setNom] = useState("");
        ///   setNom(newValue);  // re-rend le composant
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            // Ne rien faire si la valeur n'a pas change
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
```

---

## Etape 5 : RelayCommand — remplacer les evenements Click

En MVVM, on ne branche plus les boutons avec `Click="..."`. On utilise des **Commands** qu'on lie avec le Data Binding.

```csharp
// ViewModels/RelayCommand.cs
// Permet de lier un bouton XAML a une methode du ViewModel.
//
// AVANT (code-behind) :
//   <Button Click="BtnSave_Click"/>
//   private void BtnSave_Click(...) { ... }
//
// APRES (MVVM) :
//   <Button Command="{Binding SaveCommand}"/>
//   public ICommand SaveCommand { get; }
//   SaveCommand = new RelayCommand(() => Save(), () => CanSave());
//
// Avantage : le bouton se GRISE automatiquement si CanSave() retourne false !

using System.Windows.Input;

namespace NotesApp.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;           // La methode a executer
        private readonly Func<bool>? _canExecute;   // Peut-on executer ? (optionnel)

        /// <summary>
        /// Cree une commande.
        /// </summary>
        /// <param name="execute">La methode a appeler quand le bouton est clique</param>
        /// <param name="canExecute">Retourne true si le bouton doit etre actif (optionnel)</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Le bouton est-il actif ?
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        // Executer la commande
        public void Execute(object? parameter) => _execute();

        // WPF reevalue automatiquement CanExecute quand l'utilisateur interagit
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
```

---

## Etape 6 : Le MainViewModel — toute la logique

C'est ici que toute la logique metier vit. Le code-behind (`MainWindow.xaml.cs`) sera quasi vide.

```csharp
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
                Contenu = Contenu.Trim()
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
            NoteSelectionnee = null;
        }

        private void MajCompteur()
        {
            Compteur = $"{Notes.Count} note(s)";
        }
    }
}
```

### Ce qu'il faut retenir

| Concept | Code-behind (Tuto 2) | MVVM (Tuto 4) |
|---------|----------------------|---------------|
| Lire un champ | `txtNom.Text` | Propriete `Titre` avec `SetProperty` |
| Bouton clic | `Click="BtnAjouter_Click"` | `Command="{Binding AjouterCommand}"` |
| Liste | `dgContacts.ItemsSource = _contacts` | `ItemsSource="{Binding Notes}"` |
| Selection | `dgContacts.SelectedItem as Contact` | Propriete `NoteSelectionnee` avec Binding |
| Bouton desactive | gerer manuellement `IsEnabled` | `CanExecute` dans RelayCommand (automatique !) |

---

## Etape 7 : La View (XAML) — avec Data Binding complet

Remplacez tout `MainWindow.xaml` :

```xml
<Window x:Class="NotesApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Notes App — MVVM" 
        Height="650" Width="950"
        WindowStartupLocation="CenterScreen"
        Background="#ECF0F1">

    <Window.Resources>
        <!-- Styles globaux -->
        <Style TargetType="TextBox">
            <Setter Property="Padding" Value="8"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Margin" Value="0,0,0,10"/>
            <Setter Property="BorderBrush" Value="#CBD5E0"/>
        </Style>
        <Style TargetType="Button">
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="FontWeight" Value="Bold"/>
            <Setter Property="Padding" Value="15,8"/>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Opacity" Value="0.85"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <Grid Margin="20">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="320"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- ==========================================
             FORMULAIRE (gauche)
             ========================================== -->
        <Border Grid.Column="0" Background="White" CornerRadius="8"
                Padding="20" Margin="0,0,10,0">
            <Border.Effect>
                <DropShadowEffect ShadowDepth="2" Opacity="0.1" BlurRadius="8"/>
            </Border.Effect>
            <StackPanel>
                <TextBlock Text="Note" FontSize="22" FontWeight="Bold"
                           Foreground="#1B2838" Margin="0,0,0,15"/>

                <TextBlock Text="Titre" FontWeight="SemiBold" Margin="0,0,0,5"/>
                <!-- BINDING : lie le TextBox a la propriete Titre du ViewModel -->
                <!-- UpdateSourceTrigger=PropertyChanged : met a jour a chaque frappe -->
                <TextBox Text="{Binding Titre, UpdateSourceTrigger=PropertyChanged}"/>

                <TextBlock Text="Contenu" FontWeight="SemiBold" Margin="0,0,0,5"/>
                <!-- AcceptsReturn : permet les retours a la ligne (comme un <textarea>) -->
                <TextBox Text="{Binding Contenu, UpdateSourceTrigger=PropertyChanged}"
                         AcceptsReturn="True" TextWrapping="Wrap"
                         Height="120" VerticalScrollBarVisibility="Auto"/>

                <!-- COMMAND au lieu de Click : le bouton est lie a AjouterCommand -->
                <StackPanel Orientation="Horizontal" Margin="0,5,0,0">
                    <Button Content="Ajouter"
                            Command="{Binding AjouterCommand}"
                            Background="#2EC4B6" Foreground="White" Margin="0,0,8,0"/>
                    <Button Content="Modifier"
                            Command="{Binding ModifierCommand}"
                            Background="#3B82F6" Foreground="White" Margin="0,0,8,0"/>
                    <Button Content="Supprimer"
                            Command="{Binding SupprimerCommand}"
                            Background="#EF4444" Foreground="White"/>
                </StackPanel>

                <Button Content="Vider le formulaire"
                        Command="{Binding ViderCommand}"
                        Background="#718096" Foreground="White"
                        Margin="0,10,0,0"/>
            </StackPanel>
        </Border>

        <!-- ==========================================
             LISTE DES NOTES (droite)
             ========================================== -->
        <Border Grid.Column="1" Background="White" CornerRadius="8" Padding="20">
            <Border.Effect>
                <DropShadowEffect ShadowDepth="2" Opacity="0.1" BlurRadius="8"/>
            </Border.Effect>
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <TextBlock Grid.Row="0" Text="Mes notes" FontSize="22"
                           FontWeight="Bold" Foreground="#1B2838" Margin="0,0,0,10"/>

                <!-- Recherche liee a la propriete Recherche du ViewModel -->
                <TextBox Grid.Row="1"
                         Text="{Binding Recherche, UpdateSourceTrigger=PropertyChanged}"/>

                <!-- ==========================================
                     LISTBOX avec DATATEMPLATE PERSONNALISE
                     Le DataTemplate definit COMMENT afficher chaque note.
                     C'est comme le contenu du .map() en React :
                     
                     {notes.map(note => (
                       <div className="card">
                         <div className="avatar">{note.initiales}</div>
                         <div>
                           <b>{note.titre}</b>
                           <span>{note.dateFormatee}</span>
                         </div>
                       </div>
                     ))}
                     ========================================== -->
                <ListBox Grid.Row="2"
                         ItemsSource="{Binding Notes}"
                         SelectedItem="{Binding NoteSelectionnee}"
                         BorderThickness="0"
                         ScrollViewer.HorizontalScrollBarVisibility="Disabled">

                    <ListBox.ItemTemplate>
                        <DataTemplate>
                            <!-- Chaque note s'affiche comme une carte -->
                            <Border Margin="0,0,0,8" Padding="12"
                                    Background="#F8F9FA" CornerRadius="6">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="45"/>
                                        <ColumnDefinition Width="*"/>
                                    </Grid.ColumnDefinitions>

                                    <!-- Avatar cercle avec les initiales -->
                                    <Border Grid.Column="0"
                                            Width="40" Height="40"
                                            CornerRadius="20"
                                            Background="#2EC4B6"
                                            Margin="0,0,10,0">
                                        <TextBlock Text="{Binding Initiales}"
                                                   Foreground="White"
                                                   FontWeight="Bold" FontSize="14"
                                                   HorizontalAlignment="Center"
                                                   VerticalAlignment="Center"/>
                                    </Border>

                                    <!-- Infos de la note -->
                                    <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                        <TextBlock Text="{Binding Titre}"
                                                   FontSize="15" FontWeight="Bold"
                                                   Foreground="#1B2838"/>
                                        <TextBlock Text="{Binding DateFormatee}"
                                                   FontSize="11" Foreground="#718096"
                                                   Margin="0,2,0,0"/>
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ListBox.ItemTemplate>
                </ListBox>

                <!-- Compteur lie a la propriete Compteur du ViewModel -->
                <TextBlock Grid.Row="3" Text="{Binding Compteur}"
                           Margin="0,10,0,0" Foreground="#718096" FontSize="12"/>
            </Grid>
        </Border>
    </Grid>
</Window>
```

### Les points cles du XAML MVVM

| Avant (code-behind) | Apres (MVVM) |
|---------------------|-------------|
| `x:Name="txtNom"` + `txtNom.Text` en C# | `Text="{Binding Titre}"` — plus besoin de x:Name |
| `Click="BtnSave_Click"` | `Command="{Binding SaveCommand}"` |
| `dgContacts.ItemsSource = _contacts` en C# | `ItemsSource="{Binding Notes}"` dans le XAML |
| `dgContacts.SelectedItem as Contact` en C# | `SelectedItem="{Binding NoteSelectionnee}"` |
| `lblCount.Text = "5 notes"` en C# | `Text="{Binding Compteur}"` |

**Le XAML se lie directement aux proprietes du ViewModel. Zero x:Name, zero code-behind.**

---

## Etape 8 : Le code-behind — quasi vide !

C'est la beaute de MVVM : le code-behind ne fait qu'une seule chose : connecter le ViewModel.

Remplacez tout `MainWindow.xaml.cs` :

```csharp
// MainWindow.xaml.cs — Code-behind MINIMAL
// En MVVM, le code-behind ne fait qu'UNE chose : 
// connecter le ViewModel au DataContext.
// Toute la logique est dans MainViewModel.cs.

using System.Windows;
using NotesApp.ViewModels;

namespace NotesApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DataContext = le lien entre la View et le ViewModel
            // C'est comme passer des props a un composant React :
            //   <MainWindow viewModel={new MainViewModel()} />
            // Toutes les {Binding ...} dans le XAML liront les proprietes
            // de ce MainViewModel.
            DataContext = new MainViewModel();
        }
    }
}
```

**C'est tout.** 3 lignes utiles. Toute la logique est dans `MainViewModel.cs`.

---

## Etape 9 : Tester

```powershell
dotnet run
```

### Scenarios de test

| Test | Resultat attendu |
|------|-----------------|
| Taper un titre + contenu, cliquer "Ajouter" | La note apparait dans la liste avec un avatar colore |
| Titre vide → cliquer "Ajouter" | Le bouton est GRISE (CanExecute = false) |
| Rien selectionne → "Modifier" ou "Supprimer" | Les boutons sont GRISES |
| Cliquer une note dans la liste | Le formulaire se remplit automatiquement |
| Modifier le titre et cliquer "Modifier" | La note est mise a jour |
| Cliquer "Supprimer" | Confirmation puis suppression |
| Taper dans la recherche | Filtre en temps reel |
| Fermer et rouvrir | Les notes sont toujours la (SQLite) |

> **Le point important** : les boutons se grisent automatiquement ! C'est la magie de `CanExecute` dans `RelayCommand`. Vous n'avez rien a gerer manuellement.

---

## Recapitulatif : Code-behind vs MVVM

```
CODE-BEHIND (Tuto 2)              MVVM (Tuto 4)
─────────────────────              ──────────────────
MainWindow.xaml.cs                 MainViewModel.cs
├── _repo                          ├── _repo
├── _contacts                      ├── Notes (ObservableCollection)
├── _selectedContact               ├── NoteSelectionnee (avec SetProperty)
├── BtnAjouter_Click()             ├── AjouterCommand (RelayCommand)
├── BtnModifier_Click()            ├── ModifierCommand
├── BtnSupprimer_Click()           ├── SupprimerCommand
├── TxtRecherche_TextChanged()     ├── Recherche (setter declenche Filtrer)
├── DgContacts_SelectionChanged()  └── NoteSelectionnee (setter remplit le form)
└── ViderFormulaire()

MainWindow.xaml                    MainWindow.xaml
├── x:Name="txtNom"                ├── Text="{Binding Titre}"
├── Click="BtnAjouter_Click"       ├── Command="{Binding AjouterCommand}"
├── TextChanged="TxtRecherche_..." └── Text="{Binding Recherche}"
└── SelectionChanged="DgContacts_..."

MainWindow.xaml.cs                 MainWindow.xaml.cs
= 80+ lignes de logique            = 3 lignes (juste DataContext)
```

---

## Ce que tu as appris

- **INotifyPropertyChanged** : quand une propriete change, l'UI est notifiee automatiquement (comme `useState` en React)
- **SetProperty** : met a jour un champ ET notifie l'UI en une seule ligne
- **RelayCommand** : lie un bouton a une methode du ViewModel, avec desactivation automatique via `CanExecute`
- **DataContext** : connecte le ViewModel a la View (comme passer des props en React)
- **DataTemplate** : personnalise l'affichage de chaque element d'une liste (comme le contenu de `.map()` en React)
- **Zero x:Name** : en MVVM, tout passe par le Binding. Pas besoin de nommer les controles.
- **Code-behind quasi vide** : toute la logique est testable independamment de l'UI

---

## Pour aller plus loin

- Ajouter des **categories** (tags) sur les notes avec un ComboBox lie par Binding
- Implementer **INotifyPropertyChanged** sur le Model `Note` pour que les modifications se reflettent en temps reel dans la liste sans rechargement complet
- Creer un **deuxieme ViewModel** (DetailViewModel) avec navigation entre deux vues
- Utiliser un **framework MVVM** comme CommunityToolkit.Mvvm pour generer le boilerplate automatiquement :
  ```csharp
  // Avec CommunityToolkit.Mvvm, tout ce boilerplate disparait :
  [ObservableProperty] private string _titre = "";
  [RelayCommand] private void Ajouter() { ... }
  ```