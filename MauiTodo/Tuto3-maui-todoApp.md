# TUTO 3 : .NET MAUI — Application To-Do List cross-platform

> **Developpement Desktop .NET** — YNOV B3 — 2025/2026  
> **Duree estimee** : 2h00  
> **Niveau** : Intermediaire  
> **Prerequis** : avoir fait les Tutos 1 et 2 + .NET 8 SDK + workload MAUI installe

---

## Objectifs

- Comprendre ce qu'est .NET MAUI et comment il se compare a WPF
- Creer un projet MAUI depuis le terminal
- Decouvrir les controles MAUI (Entry, Label, Button, CollectionView...)
- Implementer la navigation avec Shell (comme React Router)
- Gerer une liste de taches avec ObservableCollection
- Persister les donnees avec le fichier JSON local (Preferences)
- Utiliser le Data Binding en MAUI

---

## Compatibilite

| Plateforme | MAUI fonctionne ? | Comment lancer |
|-----------|-------------------|----------------|
| Windows 10/11 | Oui | `dotnet build -t:Run -f net8.0-windows10.0.19041.0` |
| macOS | Oui (avec Xcode installe) | `dotnet build -t:Run -f net8.0-maccatalyst` |
| Linux | Non officiellement | Pas supporte par Microsoft (alternative : Avalonia) |

> **Important** : sur Windows, assurez-vous d'avoir installe le workload MAUI :
> ```powershell
> dotnet workload install maui
> ```
> Cette commande peut prendre quelques minutes.

---

## Rappel : MAUI vs WPF — les differences

| Concept | WPF | MAUI |
|---------|-----|------|
| Fenetre | `<Window>` | `<ContentPage>` |
| Empiler verticalement | `<StackPanel>` | `<VerticalStackLayout>` |
| Empiler horizontalement | `<StackPanel Orientation="Horizontal">` | `<HorizontalStackLayout>` |
| Texte | `<TextBlock>` | `<Label>` |
| Champ de saisie | `<TextBox>` | `<Entry>` |
| Zone de texte multiligne | `<TextBox AcceptsReturn="True">` | `<Editor>` |
| Bouton | `<Button Click="...">` | `<Button Clicked="...">` |
| Liste deroulante | `<ComboBox>` | `<Picker>` |
| Case a cocher | `<CheckBox>` | `<CheckBox>` |
| Liste de donnees | `<DataGrid>` ou `<ListBox>` | `<CollectionView>` |
| Grille | `<Grid>` | `<Grid>` (identique !) |
| Navigation | pas de navigation integree | `Shell` + `GoToAsync` |
| Style | `<Style TargetType="Button">` | identique ! |
| Data Binding | `{Binding Nom}` | identique ! |

**90% des concepts sont les memes.** Les noms changent un peu, mais la logique est identique.

---

## Etape 1 : Creer le projet

```powershell
cd ~/dev-desktop

# Installer le workload MAUI si pas deja fait
dotnet workload install maui

# Creer le projet
dotnet new maui -n MauiTodo
cd MauiTodo
code .
```

### Configurer le projet pour Windows (mode non-packagee)

Par defaut, MAUI sur Windows necessite que l'application soit packagée en MSIX, ce qui
requiert que le **Windows App Runtime** soit enregistre dans le systeme. Pour eviter
cette contrainte en developpement, ajoutez cette ligne dans `MauiTodo.csproj` :

```xml
<WindowsPackageType Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">None</WindowsPackageType>
```

Elle doit etre placee dans le `<PropertyGroup>`, apres les lignes
`SupportedOSPlatformVersion` / `TargetPlatformMinVersion` pour Windows :

```xml
<SupportedOSPlatformVersion Condition="... == 'windows'">10.0.17763.0</SupportedOSPlatformVersion>
<TargetPlatformMinVersion   Condition="... == 'windows'">10.0.17763.0</TargetPlatformMinVersion>
<!-- AJOUT : desactive le packaging MSIX en developpement -->
<WindowsPackageType Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">None</WindowsPackageType>
```

> **Pourquoi ?** Sans cette ligne, MAUI essaie d'initialiser le Windows App SDK via COM
> (`REGDB_E_CLASSNOTREG`). Avec `WindowsPackageType=None`, l'app tourne en mode
> "unpackaged" et charge le runtime dynamiquement, sans avoir besoin d'un MSIX installe.

### Lancer le projet

> **Important** : apres avoir modifie le `.csproj`, supprimez les dossiers `bin` et `obj`
> pour forcer une recompilation complete. Sans ca, MSBuild reutilise l'ancien binaire.

```powershell
# Sur Windows — dans PowerShell (pas de && en PowerShell, on lance les commandes une par une)
Remove-Item -Recurse -Force bin, obj
dotnet build -f net8.0-windows10.0.19041.0
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

```bash
# Sur macOS
dotnet build -t:Run -f net8.0-maccatalyst
```

> **Pourquoi deux etapes sur Windows ?**  
> `dotnet build -f ...` compile le projet et verifie qu'il n'y a pas d'erreur.  
> `dotnet build -t:Run -f ...` compile **et lance** l'application. Si le premier reussit,
> le deuxieme demarre l'app.

> **Attention** : le premier build MAUI prend du temps (1-3 minutes). C'est normal. Les builds suivants seront plus rapides.

Une fenetre avec "Hello, World!" doit apparaitre.

### Structure du projet

```
MauiTodo/
├── App.xaml              ← Config globale (styles, couleurs)
├── App.xaml.cs           ← Demarrage de l'app
├── AppShell.xaml         ← Navigation (comme le router)
├── AppShell.xaml.cs      ← Code de la navigation
├── MainPage.xaml         ← Page d'accueil
├── MainPage.xaml.cs      ← Code de la page d'accueil
├── MauiProgram.cs        ← Config des services (comme Program.cs)
├── Platforms/            ← Code specifique par OS (on n'y touche pas)
├── Resources/            ← Images, polices, styles
└── MauiTodo.csproj       ← Config du projet
```

---

## Etape 2 : Creer le modele TodoItem

Creez un dossier `Models/` et un fichier `TodoItem.cs` :

```powershell
mkdir Models
```

```csharp
// Models/TodoItem.cs
namespace MauiTodo.Models
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
```

> **Pourquoi un `string` pour l'Id ?**  
> On n'utilise pas SQLite dans ce tuto (pour garder les choses simples). On utilise un `Guid` (identifiant unique universel) pour identifier chaque tache. C'est comme un UUID en JavaScript.

---

## Etape 3 : Creer le service de donnees

On va stocker les taches dans un fichier JSON local. Creez un dossier `Services/` :

```powershell
mkdir Services
```

```csharp
// Services/TodoService.cs
using System.Text.Json;
using MauiTodo.Models;

namespace MauiTodo.Services
{
    public class TodoService
    {
        // Chemin du fichier JSON ou on sauvegarde les taches
        // AppDataDirectory = dossier de donnees de l'app (different par OS)
        private readonly string _filePath;

        public TodoService()
        {
            _filePath = Path.Combine(
                FileSystem.AppDataDirectory, "todos.json");
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
```

---

## Etape 4 : La page principale — liste des taches

Ouvrez `MainPage.xaml` et **remplacez tout** :

```xml
<?xml version="1.0" encoding="utf-8" ?>
<!-- MainPage.xaml — La page principale avec la liste des taches -->
<!-- ContentPage = l'equivalent de Window en WPF -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MauiTodo.MainPage"
             Title="Mes Taches"
             BackgroundColor="#ECF0F1">

    <!-- VerticalStackLayout = StackPanel en WPF = Flexbox column -->
    <Grid Padding="20" RowSpacing="15">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>   <!-- Titre -->
            <RowDefinition Height="Auto"/>   <!-- Barre d'ajout -->
            <RowDefinition Height="*"/>      <!-- Liste -->
            <RowDefinition Height="Auto"/>   <!-- Compteur -->
        </Grid.RowDefinitions>

        <!-- TITRE -->
        <Label Grid.Row="0"
               Text="Mes Taches"
               FontSize="28" FontAttributes="Bold"
               TextColor="#1B2838"/>

        <!-- BARRE D'AJOUT : champ + bouton sur la meme ligne -->
        <!-- HorizontalStackLayout = StackPanel Horizontal en WPF -->
        <HorizontalStackLayout Grid.Row="1" Spacing="10">
            <!-- Entry = TextBox en WPF = <input> en HTML -->
            <Entry x:Name="txtNouvelleTache"
                   Placeholder="Nouvelle tache..."
                   FontSize="16"
                   WidthRequest="300"
                   BackgroundColor="White"/>

            <!-- Button : Clicked = Click en WPF -->
            <Button Text="Ajouter"
                    Clicked="BtnAjouter_Clicked"
                    FontAttributes="Bold"
                    BackgroundColor="#2EC4B6"
                    TextColor="White"/>
        </HorizontalStackLayout>

        <!-- LISTE DES TACHES -->
        <!-- CollectionView = l'equivalent de DataGrid/ListBox en WPF -->
        <CollectionView Grid.Row="2"
                        x:Name="listeTaches"
                        SelectionMode="None">

            <!-- ItemTemplate = DataTemplate en WPF = .map() en React -->
            <!-- Definit COMMENT afficher chaque tache -->
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <!-- Frame = Border en WPF (carte avec ombre) -->
                    <Frame BackgroundColor="White"
                           CornerRadius="8"
                           Padding="15"
                           Margin="0,0,0,8"
                           HasShadow="True">

                        <Grid ColumnSpacing="15">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>

                            <!-- CheckBox pour marquer comme terminee -->
                            <CheckBox Grid.Column="0"
                                      IsChecked="{Binding EstTerminee}"
                                      CheckedChanged="CheckBox_Changed"/>

                            <!-- Titre de la tache -->
                            <VerticalStackLayout Grid.Column="1"
                                                 VerticalOptions="Center">
                                <Label Text="{Binding Titre}"
                                       FontSize="16"
                                       FontAttributes="Bold"
                                       TextColor="#1B2838"/>
                                <Label Text="{Binding Description}"
                                       FontSize="13"
                                       TextColor="#718096"/>
                            </VerticalStackLayout>

                            <!-- Bouton supprimer -->
                            <Button Grid.Column="2"
                                    Text="X"
                                    Clicked="BtnSupprimer_Clicked"
                                    CommandParameter="{Binding Id}"
                                    BackgroundColor="#EF4444"
                                    TextColor="White"
                                    FontAttributes="Bold"
                                    WidthRequest="40"
                                    HeightRequest="40"
                                    CornerRadius="20"/>
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>

            <!-- Message quand la liste est vide -->
            <CollectionView.EmptyView>
                <VerticalStackLayout HorizontalOptions="Center"
                                     VerticalOptions="Center"
                                     Padding="40">
                    <Label Text="Aucune tache"
                           FontSize="20"
                           TextColor="#718096"
                           HorizontalOptions="Center"/>
                    <Label Text="Ajoutez votre premiere tache ci-dessus"
                           FontSize="14"
                           TextColor="#A0AEC0"
                           HorizontalOptions="Center"/>
                </VerticalStackLayout>
            </CollectionView.EmptyView>

        </CollectionView>

        <!-- COMPTEUR en bas -->
        <Label Grid.Row="3" x:Name="lblCount"
               FontSize="13" TextColor="#718096"/>

    </Grid>
</ContentPage>
```

### Ce que fait ce XAML

| MAUI | WPF equivalent | HTML equivalent |
|------|---------------|----------------|
| `<ContentPage>` | `<Window>` | `<html>` |
| `<VerticalStackLayout>` | `<StackPanel>` | `<div style="display:flex;flex-direction:column">` |
| `<HorizontalStackLayout>` | `<StackPanel Orientation="Horizontal">` | `<div style="display:flex">` |
| `<Label>` | `<TextBlock>` | `<p>` |
| `<Entry>` | `<TextBox>` | `<input>` |
| `<Button Clicked="...">` | `<Button Click="...">` | `<button onclick="...">` |
| `<CollectionView>` | `<DataGrid>` ou `<ListBox>` | `.map()` en React |
| `<Frame>` | `<Border>` | `<div class="card">` |
| `<CheckBox>` | `<CheckBox>` | `<input type="checkbox">` |
| `{Binding Titre}` | `{Binding Titre}` | identique ! |
| `CommandParameter` | `CommandParameter` | `data-id` en HTML |

---

## Etape 5 : Le code logique de la page principale

Ouvrez `MainPage.xaml.cs` et **remplacez tout** :

```csharp
// MainPage.xaml.cs — Logique de la page principale
using System.Collections.ObjectModel;
using MauiTodo.Models;
using MauiTodo.Services;

namespace MauiTodo
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
```

---

## Etape 6 : Configurer la navigation Shell

Le `AppShell.xaml` est le **routeur** de MAUI. C'est comme React Router ou Vue Router.

Ouvrez `AppShell.xaml` et **remplacez tout** :

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<!-- AppShell.xaml — Le routeur de l'application -->
<!-- Shell = le conteneur de navigation (comme BrowserRouter en React) -->
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:local="clr-namespace:MauiTodo"
       x:Class="MauiTodo.AppShell"
       Shell.FlyoutBehavior="Disabled">

    <!-- ShellContent = une page accessible -->
    <!-- C'est comme <Route path="/" element={<MainPage/>}/> en React -->
    <ShellContent
        Title="Mes Taches"
        ContentTemplate="{DataTemplate local:MainPage}"
        Route="MainPage"/>

</Shell>
```

> **Navigation avancee** : si vous vouliez plusieurs pages avec des onglets :
> ```xml
> <TabBar>
>     <ShellContent Title="Taches" ContentTemplate="{DataTemplate local:MainPage}" Route="main"/>
>     <ShellContent Title="A propos" ContentTemplate="{DataTemplate local:AboutPage}" Route="about"/>
> </TabBar>
> ```
> Et pour naviguer entre les pages :
> ```csharp
> // Comme router.push('/about') en Vue/React
> await Shell.Current.GoToAsync("about");
> // Revenir en arriere
> await Shell.Current.GoToAsync("..");
> ```

---

## Etape 7 : Personnaliser les styles globaux

Ouvrez `App.xaml` et ajoutez/modifiez les styles dans la section `<Application.Resources>` :

```xml
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MauiTodo.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>

            <!-- Vos styles personnalises -->
            <Style TargetType="Entry">
                <Setter Property="FontSize" Value="16"/>
            </Style>

            <Style TargetType="Button">
                <Setter Property="FontSize" Value="15"/>
                <Setter Property="Padding" Value="15,10"/>
            </Style>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

> **Les styles MAUI fonctionnent exactement comme en WPF** : `TargetType`, `Setter`, `Property`, `Value`. Pas besoin de reapprendre !

---

## Etape 8 : Tester l'application

```powershell
# Windows
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

```bash
# macOS
dotnet build -t:Run -f net8.0-maccatalyst
```

### Scenarios de test

| Test | Action | Resultat attendu |
|------|--------|-----------------|
| Ajouter | Taper un titre + "Ajouter" | La tache apparait dans la liste |
| Ajouter vide | Cliquer "Ajouter" sans texte | Message d'alerte |
| Cocher | Cliquer la checkbox | La tache est marquee terminee, compteur mis a jour |
| Decocher | Recliquer la checkbox | La tache redevient "a faire" |
| Supprimer | Cliquer le bouton X rouge | Confirmation puis suppression |
| Persistance | Fermer l'app, la rouvrir | Les taches sont toujours la |
| Liste vide | Supprimer toutes les taches | Message "Aucune tache" s'affiche |

---

## Recapitulatif : la navigation MAUI en detail

| Concept | MAUI | React Router | Vue Router |
|---------|------|-------------|------------|
| Routeur | `<Shell>` | `<BrowserRouter>` | `createRouter()` |
| Route | `<ShellContent Route="...">` | `<Route path="...">` | `{ path: "..." }` |
| Naviguer | `Shell.Current.GoToAsync("page")` | `navigate("/page")` | `router.push("/page")` |
| Avec parametres | `GoToAsync("page?id=123")` | `navigate("/page/123")` | `router.push({ params: { id: 123 } })` |
| Recevoir params | `[QueryProperty("Id","id")]` | `useParams()` | `route.params.id` |
| Retour | `GoToAsync("..")` | `navigate(-1)` | `router.back()` |
| Onglets | `<TabBar>` | Layout avec onglets | Layout avec onglets |

---

## Erreurs frequentes

| Erreur | Cause | Solution |
|--------|-------|----------|
| Build tres long (premiere fois) | Normal, MAUI compile pour la plateforme | Attendre 1-3 min, les builds suivants seront rapides |
| `NETSDK1147: workload maui not installed` | Workload MAUI manquant | `dotnet workload install maui` |
| `Platform version not found` | Mauvais framework cible | Verifier le `-f net8.0-windows10.0.19041.0` |
| `DisplayAlert` ne compile pas | Il faut etre dans une `ContentPage` | Verifier que la classe herite de `ContentPage` |
| Les donnees ne persistent pas | Chemin du fichier incorrect | Utiliser `FileSystem.AppDataDirectory` |
| `REGDB_E_CLASSNOTREG (0x80040154)` | Windows App Runtime non enregistre (pas de MSIX) | Ajouter `<WindowsPackageType>None</WindowsPackageType>` dans le `.csproj` (voir section "Configurer le projet pour Windows") |
| `MauiTodo.exe n'est pas reconnu` apres modif du `.csproj` | MSBuild reutilise l'ancien binaire cache | `Remove-Item -Recurse -Force bin, obj` puis relancer le build |
| `&&` ne fonctionne pas dans le terminal | PowerShell n'accepte pas `&&` comme separateur | Lancer les commandes une par une, ou utiliser `;` (qui continue meme en cas d'erreur) |

---

## Ce que tu as appris

- **MAUI = WPF cross-platform** : meme XAML, memes concepts, noms legerement differents
- **ContentPage** = Window : la base de chaque page
- **VerticalStackLayout** = StackPanel : empile verticalement
- **Entry** = TextBox : champ de saisie
- **CollectionView** = DataGrid/ListBox : affiche une liste de donnees
- **DataTemplate** = ItemTemplate : definit l'affichage de chaque element (comme .map() en React)
- **Shell** = le routeur de MAUI (comme React Router / Vue Router)
- **DisplayAlert** = MessageBox = alert() : boite de dialogue
- **ObservableCollection** : fonctionne exactement comme en WPF
- **Data Binding** `{Binding Prop}` : identique a WPF
- **Styles** : meme syntaxe qu'en WPF
- **FileSystem.AppDataDirectory** : dossier de donnees propre a chaque OS

---

## Pour aller plus loin

- Ajouter une **page de detail** (navigation vers une deuxieme page avec les infos completes)
- Ajouter des **categories/priorites** avec un Picker (equivalent de ComboBox/select)
- Remplacer le fichier JSON par **SQLite** (meme package `Microsoft.Data.Sqlite`)
- Ajouter un **tri** (par date, par statut)
- Deployer l'app comme un **vrai .exe** ou **.app** avec `dotnet publish`