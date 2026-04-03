# TUTO 2 : Gestionnaire de Contacts — WPF + SQLite (CRUD complet)

> **Développement Desktop .NET** — YNOV B3 — 2025/2026  
> **Durée estimée** : 2h00  
> **Niveau** : Intermédiaire  
> **Prérequis** : avoir fait le Tuto 1 + .NET 8 SDK

---

## Objectifs

À la fin de ce tuto, tu sauras :

- Créer un projet WPF et structurer l'interface en XAML
- Utiliser le `Grid` (comme CSS Grid) et le `StackPanel` (comme Flexbox)
- Connecter une base de données SQLite pour persister les données
- Implémenter un **CRUD complet** (Create, Read, Update, Delete)
- Afficher des données dans un `DataGrid`
- Ajouter une **recherche en temps réel**
- Utiliser `ObservableCollection<T>` pour rafraîchir l'UI automatiquement

---

## Résultat attendu

Une application de gestion de contacts avec :

- Un formulaire à gauche (nom, email, téléphone)
- Une liste de contacts à droite (DataGrid)
- Les boutons Ajouter, Modifier, Supprimer
- Une barre de recherche qui filtre en temps réel
- Les données persistent après fermeture (SQLite)

---

## Compatibilité

| Plateforme | WPF natif | Alternative |
|-----------|-----------|-------------|
| Windows   | Oui       | —           |
| macOS     | Non       | Avalonia UI (même syntaxe XAML, cross-platform) |
| Linux     | Non       | Avalonia UI |

> **Si tu es sur Mac/Linux** : installe [Avalonia UI](https://avaloniaui.net/). La syntaxe XAML est quasi identique à WPF. Les concepts (Grid, StackPanel, Data Binding) sont les mêmes. Le code C# est identique.

---

## Étape 1 : Créer le projet et installer SQLite

```bash
# Créer le projet WPF
dotnet new wpf -n ContactManager
cd ContactManager

# Installer le package SQLite (base de données locale)
dotnet add package Microsoft.Data.Sqlite
```

### Vérifier que ça fonctionne

```bash
dotnet run
```

Une fenêtre vide apparaît. C'est normal, on n'a encore rien mis dedans.

### Structure du projet

```
ContactManager/
├── App.xaml              ← Configuration globale de l'app
├── App.xaml.cs           ← Code global
├── MainWindow.xaml       ← L'interface (on va la modifier)
├── MainWindow.xaml.cs    ← Le code logique (on va le modifier)
├── ContactManager.csproj ← Configuration du projet + packages
```

On va ajouter :

```
├── Models/
│   └── Contact.cs            ← Le modèle de données
├── Data/
│   └── ContactRepository.cs  ← L'accès à la base de données
```

---

## Étape 2 : Créer le modèle Contact

Le modèle représente un contact dans notre application. C'est une classe C# toute simple.

Crée le dossier et le fichier :

```bash
# Windows (PowerShell)
mkdir Models
New-Item Models/Contact.cs

# macOS / Linux
mkdir Models
touch Models/Contact.cs
```

Remplis `Models/Contact.cs` avec :

```csharp
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
    }
}
```

> **Les propriétés auto `{ get; set; }`**
>
> En C#, on utilise des propriétés au lieu de champs publics. C'est la convention.
> - En Java : `private String nom;` + `getNom()` + `setNom()`
> - En C# : `public string Nom { get; set; }` → le getter et le setter sont générés automatiquement
> - En TypeScript : c'est comme `nom: string` dans une interface

---

## Étape 3 : Créer le Repository SQLite

Le Repository est la couche qui parle à la base de données. Il encapsule toutes les requêtes SQL.

```bash
# Windows
mkdir Data
New-Item Data/ContactRepository.cs

# macOS / Linux
mkdir Data
touch Data/ContactRepository.cs
```

Remplis `Data/ContactRepository.cs` avec :

```csharp
// Data/ContactRepository.cs
// Le Repository gère toutes les opérations sur la base de données.
// C'est l'équivalent d'un fichier "contactService.js" ou "contactDAO.java".
// Il parle à SQLite et retourne des objets Contact.

using Microsoft.Data.Sqlite;
using ContactManager.Models;

namespace ContactManager.Data
{
    public class ContactRepository
    {
        // La chaîne de connexion indique le chemin du fichier SQLite
        // "Data Source=contacts.db" → crée/ouvre le fichier contacts.db
        private readonly string _connectionString;

        /// <summary>
        /// Constructeur : initialise la connexion et crée la table si elle n'existe pas
        /// </summary>
        /// <param name="dbPath">Chemin du fichier de base de données (par défaut : contacts.db)</param>
        public ContactRepository(string dbPath = "contacts.db")
        {
            _connectionString = $"Data Source={dbPath}";
            InitialiserBase();
        }

        /// <summary>
        /// Crée la table "contacts" si elle n'existe pas encore.
        /// C'est comme un "CREATE TABLE IF NOT EXISTS" en SQL classique.
        /// </summary>
        private void InitialiserBase()
        {
            // "using" ferme automatiquement la connexion à la fin du bloc
            // C'est comme un try/finally en Java, ou un "defer" en Go
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS contacts (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nom TEXT NOT NULL,
                    email TEXT NOT NULL DEFAULT '',
                    telephone TEXT NOT NULL DEFAULT ''
                )";
            cmd.ExecuteNonQuery();
        }

        // =============================================
        // READ : Lire tous les contacts
        // =============================================
        /// <summary>
        /// Récupère tous les contacts de la base, triés par nom.
        /// C'est comme un SELECT * FROM contacts ORDER BY nom.
        /// </summary>
        public List<Contact> GetAll()
        {
            var contacts = new List<Contact>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nom, email, telephone FROM contacts ORDER BY nom";

            // ExecuteReader() exécute la requête et retourne un "curseur"
            // On lit ligne par ligne avec reader.Read()
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(new Contact
                {
                    Id = reader.GetInt32(0),          // Colonne 0 = id
                    Nom = reader.GetString(1),        // Colonne 1 = nom
                    Email = reader.GetString(2),      // Colonne 2 = email
                    Telephone = reader.GetString(3)   // Colonne 3 = telephone
                });
            }

            return contacts;
        }

        // =============================================
        // CREATE : Ajouter un contact
        // =============================================
        /// <summary>
        /// Ajoute un nouveau contact dans la base de données.
        /// Utilise des paramètres SQL ($nom, $email...) pour éviter l'injection SQL.
        /// </summary>
        public void Add(Contact contact)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();

            // IMPORTANT : on utilise des PARAMÈTRES ($nom, $email, $tel)
            // et JAMAIS de la concaténation de chaînes !
            // Mauvais : $"INSERT ... VALUES ('{contact.Nom}')"  ← INJECTION SQL !
            // Bon     : "INSERT ... VALUES ($nom)" + AddWithValue  ← SÉCURISÉ
            cmd.CommandText = @"
                INSERT INTO contacts (nom, email, telephone)
                VALUES ($nom, $email, $tel)";
            cmd.Parameters.AddWithValue("$nom", contact.Nom);
            cmd.Parameters.AddWithValue("$email", contact.Email);
            cmd.Parameters.AddWithValue("$tel", contact.Telephone);
            cmd.ExecuteNonQuery();

            // Récupérer l'ID auto-généré par SQLite
            cmd.CommandText = "SELECT last_insert_rowid()";
            contact.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        // =============================================
        // UPDATE : Modifier un contact existant
        // =============================================
        /// <summary>
        /// Met à jour un contact existant dans la base de données.
        /// On identifie le contact par son Id.
        /// </summary>
        public void Update(Contact contact)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE contacts 
                SET nom = $nom, email = $email, telephone = $tel
                WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", contact.Id);
            cmd.Parameters.AddWithValue("$nom", contact.Nom);
            cmd.Parameters.AddWithValue("$email", contact.Email);
            cmd.Parameters.AddWithValue("$tel", contact.Telephone);
            cmd.ExecuteNonQuery();
        }

        // =============================================
        // DELETE : Supprimer un contact
        // =============================================
        /// <summary>
        /// Supprime un contact de la base de données par son Id.
        /// </summary>
        public void Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM contacts WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // =============================================
        // SEARCH : Rechercher des contacts
        // =============================================
        /// <summary>
        /// Recherche des contacts par nom, email ou téléphone.
        /// Utilise LIKE pour une recherche partielle (contient).
        /// </summary>
        public List<Contact> Search(string query)
        {
            var contacts = new List<Contact>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            // LIKE '%query%' → cherche "query" n'importe où dans le texte
            // Le || est l'opérateur de concaténation en SQL (pas le OR logique !)
            cmd.CommandText = @"
                SELECT id, nom, email, telephone FROM contacts
                WHERE nom LIKE '%' || $q || '%'
                   OR email LIKE '%' || $q || '%'
                   OR telephone LIKE '%' || $q || '%'
                ORDER BY nom";
            cmd.Parameters.AddWithValue("$q", query);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(new Contact
                {
                    Id = reader.GetInt32(0),
                    Nom = reader.GetString(1),
                    Email = reader.GetString(2),
                    Telephone = reader.GetString(3)
                });
            }

            return contacts;
        }
    }
}
```

---

## Étape 4 : Créer l'interface XAML

Maintenant, on crée l'interface utilisateur. Ouvre `MainWindow.xaml` et **remplace tout le contenu** par :

```xml
<!-- MainWindow.xaml — L'interface de notre gestionnaire de contacts -->
<!-- Le XAML, c'est comme du HTML : on décrit la structure et le style de l'UI -->
<Window x:Class="ContactManager.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Gestionnaire de Contacts" 
        Height="600" Width="950"
        WindowStartupLocation="CenterScreen"
        Background="#ECF0F1">

    <!-- ==============================================
         LAYOUT PRINCIPAL : Grid à 2 colonnes
         Colonne gauche = formulaire (300px fixe)
         Colonne droite = liste des contacts (le reste)
         C'est comme : grid-template-columns: 300px 1fr;
         ============================================== -->
    <Grid Margin="20">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="300"/>  <!-- Formulaire (largeur fixe) -->
            <ColumnDefinition Width="*"/>    <!-- Liste (prend le reste = 1fr) -->
        </Grid.ColumnDefinitions>

        <!-- ==============================================
             PANNEAU GAUCHE : Formulaire de saisie
             ============================================== -->
        <Border Grid.Column="0" 
                Background="White" 
                CornerRadius="8" 
                Padding="20"
                Margin="0,0,10,0">
            <!-- Ombre portée (comme box-shadow en CSS) -->
            <Border.Effect>
                <DropShadowEffect ShadowDepth="2" Opacity="0.1" BlurRadius="8"/>
            </Border.Effect>

            <!-- StackPanel = Flexbox direction: column -->
            <StackPanel>
                <!-- Titre du panneau -->
                <TextBlock Text="Contact" 
                           FontSize="22" FontWeight="Bold"
                           Foreground="#1B2838" Margin="0,0,0,15"/>

                <!-- Champ NOM -->
                <TextBlock Text="Nom" FontWeight="SemiBold" Margin="0,0,0,5"/>
                <TextBox x:Name="txtNom" Padding="8" FontSize="14" Margin="0,0,0,10"/>

                <!-- Champ EMAIL -->
                <TextBlock Text="Email" FontWeight="SemiBold" Margin="0,0,0,5"/>
                <TextBox x:Name="txtEmail" Padding="8" FontSize="14" Margin="0,0,0,10"/>

                <!-- Champ TÉLÉPHONE -->
                <TextBlock Text="Téléphone" FontWeight="SemiBold" Margin="0,0,0,5"/>
                <TextBox x:Name="txtTel" Padding="8" FontSize="14" Margin="0,0,0,15"/>

                <!-- Boutons d'action (en ligne = Horizontal) -->
                <StackPanel Orientation="Horizontal">
                    <Button x:Name="btnAjouter" Content="Ajouter"
                            Click="BtnAjouter_Click"
                            Padding="15,8" Margin="0,0,8,0"
                            Background="#2EC4B6" Foreground="White"
                            FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>

                    <Button x:Name="btnModifier" Content="Modifier"
                            Click="BtnModifier_Click"
                            Padding="15,8" Margin="0,0,8,0"
                            Background="#3B82F6" Foreground="White"
                            FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>

                    <Button x:Name="btnSupprimer" Content="Supprimer"
                            Click="BtnSupprimer_Click"
                            Padding="15,8"
                            Background="#EF4444" Foreground="White"
                            FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>
                </StackPanel>

                <!-- Bouton vider le formulaire -->
                <Button x:Name="btnVider" Content="Vider le formulaire"
                        Click="BtnVider_Click"
                        Padding="10,6" Margin="0,10,0,0"
                        Background="#718096" Foreground="White"
                        BorderThickness="0" Cursor="Hand"/>
            </StackPanel>
        </Border>

        <!-- ==============================================
             PANNEAU DROIT : Liste des contacts
             ============================================== -->
        <Border Grid.Column="1" 
                Background="White" 
                CornerRadius="8" 
                Padding="20">
            <Border.Effect>
                <DropShadowEffect ShadowDepth="2" Opacity="0.1" BlurRadius="8"/>
            </Border.Effect>

            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>  <!-- Titre -->
                    <RowDefinition Height="Auto"/>  <!-- Recherche -->
                    <RowDefinition Height="*"/>     <!-- DataGrid (prend le reste) -->
                    <RowDefinition Height="Auto"/>  <!-- Compteur -->
                </Grid.RowDefinitions>

                <!-- Titre -->
                <TextBlock Grid.Row="0" Text="Contacts"
                           FontSize="22" FontWeight="Bold"
                           Foreground="#1B2838" Margin="0,0,0,10"/>

                <!-- Barre de recherche -->
                <TextBox Grid.Row="1" x:Name="txtRecherche"
                         Padding="8" FontSize="14" Margin="0,0,0,10"
                         TextChanged="TxtRecherche_TextChanged"/>

                <!-- DataGrid = Tableau de données (comme <table> en HTML) -->
                <DataGrid Grid.Row="2" x:Name="dgContacts"
                          AutoGenerateColumns="False"
                          IsReadOnly="True"
                          SelectionChanged="DgContacts_SelectionChanged"
                          HeadersVisibility="Column"
                          GridLinesVisibility="Horizontal"
                          BorderThickness="0"
                          Background="White"
                          RowBackground="White"
                          AlternatingRowBackground="#F8F9FA"
                          SelectionMode="Single">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Nom" 
                                            Binding="{Binding Nom}" Width="*"/>
                        <DataGridTextColumn Header="Email" 
                                            Binding="{Binding Email}" Width="*"/>
                        <DataGridTextColumn Header="Tél" 
                                            Binding="{Binding Telephone}" Width="120"/>
                    </DataGrid.Columns>
                </DataGrid>

                <!-- Compteur de contacts -->
                <TextBlock Grid.Row="3" x:Name="lblCount"
                           Margin="0,10,0,0" Foreground="#718096" FontSize="12"/>
            </Grid>
        </Border>
    </Grid>
</Window>
```

### Ce que fait ce XAML

- **Grid à 2 colonnes** : formulaire (300px) + liste (le reste). C'est comme `grid-template-columns: 300px 1fr` en CSS.
- **Border avec CornerRadius et DropShadowEffect** : c'est comme `border-radius` + `box-shadow` en CSS.
- **StackPanel** : empile les éléments verticalement, comme `display: flex; flex-direction: column`.
- **DataGrid** : affiche les contacts dans un tableau. `Binding="{Binding Nom}"` lie la colonne à la propriété `Nom` de l'objet `Contact`.
- **x:Name** : donne un nom au contrôle pour y accéder dans le code C# (comme `id` en HTML).
- **Click="BtnAjouter_Click"** : branche l'événement (comme `onclick` en HTML).

---

## Étape 5 : Le code logique (MainWindow.xaml.cs)

Ouvre `MainWindow.xaml.cs` et **remplace tout le contenu** par :

```csharp
// MainWindow.xaml.cs — Le code logique de notre gestionnaire de contacts
// C'est le "code-behind" du XAML (comme le <script> dans un fichier HTML/Vue)

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ContactManager.Data;
using ContactManager.Models;

namespace ContactManager
{
    public partial class MainWindow : Window
    {
        // === DÉPENDANCES ===
        // Le repository gère l'accès à la base de données
        private readonly ContactRepository _repo = new();

        // ObservableCollection = une liste qui NOTIFIE L'UI quand on ajoute/supprime
        // C'est comme un state React : quand ça change, le rendu se met à jour
        private ObservableCollection<Contact> _contacts = new();

        // Le contact actuellement sélectionné dans le DataGrid
        private Contact? _selectedContact;

        // =============================================
        // CONSTRUCTEUR : s'exécute au démarrage
        // =============================================
        public MainWindow()
        {
            InitializeComponent();  // Charge le XAML
            ChargerContacts();      // Charge les contacts depuis la BDD
        }

        // =============================================
        // CHARGER : Récupérer tous les contacts depuis la BDD
        // =============================================
        private void ChargerContacts()
        {
            var liste = _repo.GetAll();
            _contacts = new ObservableCollection<Contact>(liste);

            // Lier la liste au DataGrid (comme itemsSource={contacts} en React)
            dgContacts.ItemsSource = _contacts;

            // Mettre à jour le compteur
            lblCount.Text = $"{_contacts.Count} contact(s)";
        }

        // =============================================
        // AJOUTER : Créer un nouveau contact
        // =============================================
        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            // Validation : le nom est obligatoire
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show(
                    "Le nom est obligatoire !",
                    "Champ manquant",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtNom.Focus();  // Mettre le curseur dans le champ nom
                return;
            }

            // Créer l'objet Contact
            var contact = new Contact
            {
                Nom = txtNom.Text.Trim(),        // Trim() enlève les espaces avant/après
                Email = txtEmail.Text.Trim(),
                Telephone = txtTel.Text.Trim()
            };

            // Sauvegarder dans la base de données
            _repo.Add(contact);

            // Ajouter à la liste affichée (le DataGrid se met à jour automatiquement)
            _contacts.Add(contact);

            // Vider le formulaire et mettre à jour le compteur
            ViderFormulaire();
            lblCount.Text = $"{_contacts.Count} contact(s)";
        }

        // =============================================
        // MODIFIER : Mettre à jour un contact existant
        // =============================================
        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier qu'un contact est sélectionné
            if (_selectedContact == null)
            {
                MessageBox.Show(
                    "Sélectionne un contact dans la liste d'abord !",
                    "Aucune sélection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("Le nom est obligatoire !", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Mettre à jour les propriétés du contact sélectionné
            _selectedContact.Nom = txtNom.Text.Trim();
            _selectedContact.Email = txtEmail.Text.Trim();
            _selectedContact.Telephone = txtTel.Text.Trim();

            // Sauvegarder dans la BDD
            _repo.Update(_selectedContact);

            // Rafraîchir la liste (nécessaire car Contact n'implémente pas INotifyPropertyChanged)
            ChargerContacts();
            ViderFormulaire();
        }

        // =============================================
        // SUPPRIMER : Effacer un contact
        // =============================================
        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContact == null)
            {
                MessageBox.Show(
                    "Sélectionne un contact dans la liste d'abord !",
                    "Aucune sélection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Demander confirmation (comme confirm() en JavaScript)
            var result = MessageBox.Show(
                $"Supprimer le contact \"{_selectedContact.Nom}\" ?",
                "Confirmer la suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Supprimer de la BDD
                _repo.Delete(_selectedContact.Id);

                // Supprimer de la liste affichée (le DataGrid se met à jour)
                _contacts.Remove(_selectedContact);

                ViderFormulaire();
                lblCount.Text = $"{_contacts.Count} contact(s)";
            }
        }

        // =============================================
        // SÉLECTION : Quand l'utilisateur clique sur un contact dans le DataGrid
        // =============================================
        private void DgContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Récupérer le contact sélectionné
            _selectedContact = dgContacts.SelectedItem as Contact;

            if (_selectedContact != null)
            {
                // Remplir le formulaire avec les données du contact sélectionné
                txtNom.Text = _selectedContact.Nom;
                txtEmail.Text = _selectedContact.Email;
                txtTel.Text = _selectedContact.Telephone;
            }
        }

        // =============================================
        // RECHERCHE : Filtrer les contacts en temps réel
        // =============================================
        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = txtRecherche.Text.Trim();

            // Si la recherche est vide, afficher tous les contacts
            // Sinon, filtrer avec la méthode Search du repository
            var resultats = string.IsNullOrEmpty(query)
                ? _repo.GetAll()
                : _repo.Search(query);

            _contacts = new ObservableCollection<Contact>(resultats);
            dgContacts.ItemsSource = _contacts;
            lblCount.Text = $"{_contacts.Count} contact(s)";
        }

        // =============================================
        // VIDER : Remettre le formulaire à zéro
        // =============================================
        private void BtnVider_Click(object sender, RoutedEventArgs e)
        {
            ViderFormulaire();
        }

        /// <summary>
        /// Vide tous les champs du formulaire et désélectionne le contact.
        /// </summary>
        private void ViderFormulaire()
        {
            txtNom.Text = "";
            txtEmail.Text = "";
            txtTel.Text = "";
            _selectedContact = null;
            dgContacts.SelectedItem = null;
        }
    }
}
```

---

## Étape 6 : Tester l'application

```bash
dotnet run
```

### Scénarios de test

| Test | Action | Résultat attendu |
|------|--------|-----------------|
| Ajouter | Remplir le formulaire + cliquer "Ajouter" | Le contact apparaît dans la liste |
| Ajouter sans nom | Laisser le nom vide + "Ajouter" | Message d'erreur "Le nom est obligatoire" |
| Sélectionner | Cliquer sur un contact dans la liste | Le formulaire se remplit |
| Modifier | Sélectionner → changer le nom → "Modifier" | Le nom change dans la liste |
| Supprimer | Sélectionner → "Supprimer" | Confirmation demandée → contact supprimé |
| Recherche | Taper dans la barre de recherche | La liste se filtre en temps réel |
| Persistance | Fermer l'app → la rouvrir | Les contacts sont toujours là |

### Vérifier la base de données

Si tu veux voir les données brutes dans SQLite, tu peux utiliser :

```bash
# Installer sqlite3 si nécessaire
# Windows : choco install sqlite
# macOS : brew install sqlite3
# Linux : sudo apt install sqlite3

# Ouvrir la base de données
sqlite3 contacts.db

# Voir tous les contacts
SELECT * FROM contacts;

# Quitter
.quit
```

---

## Ce que tu as appris

- **XAML = HTML de Windows** : `Grid` = CSS Grid, `StackPanel` = Flexbox, `TextBox` = `<input>`, `Button` = `<button>`
- **`x:Name`** = `id` en HTML : donne un nom pour accéder au contrôle en C#
- **`Click="..."`** = `onclick` en HTML : branche un événement
- **`{Binding Nom}`** = Data Binding : lie une colonne du DataGrid à une propriété C#
- **`ObservableCollection<T>`** : comme un `state` React, notifie l'UI quand la liste change
- **SQLite** : base de données dans un seul fichier, pas besoin de serveur
- **Paramètres SQL** (`$nom`) : protègent contre l'injection SQL
- **CRUD** : les 4 opérations de base sur les données (Create, Read, Update, Delete)
- **`MessageBox`** = `alert()` / `confirm()` en JavaScript
- **`using`** : ferme automatiquement les ressources (connexions, fichiers...)

---

## Erreurs fréquentes et solutions

| Erreur | Cause probable | Solution |
|--------|---------------|----------|
| `SqliteException: no such table` | La base n'a pas été initialisée | Supprimer `contacts.db` et relancer l'app |
| Le DataGrid ne se met pas à jour | `ItemsSource` pas reassigné | Appeler `ChargerContacts()` après chaque modification |
| `NullReferenceException` sur `_selectedContact` | Aucun contact sélectionné | Vérifier `_selectedContact != null` avant d'agir |
| Le formulaire ne se vide pas | `ViderFormulaire()` pas appelé | L'appeler après chaque opération (ajouter, modifier, supprimer) |

---

## Pour aller plus loin (optionnel)

Si tu as fini en avance :

- Ajouter une **validation de l'email** (vérifier qu'il contient un @)
- Ajouter un **tri** dans le DataGrid (cliquer sur l'en-tête d'une colonne)
- Implémenter **INotifyPropertyChanged** sur le modèle Contact pour que les modifications s'affichent sans rechargement complet
- Ajouter un **export CSV** des contacts
- Ajouter un **compteur par catégorie** (nombre de contacts avec email, sans email, etc.)