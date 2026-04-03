# Développement Desktop .NET (C#)
## WinForms · WPF · MAUI · SQLite

**Bachelor 3 Informatique — YNOV Campus Nanterre — 2025/2026**  
2 jours de formation

---

## Prérequis & Installation

Avant de commencer, suis le guide complet : [Guide_Installation.md](Guide_Installation.md)

En résumé :
- **.NET 8 SDK** — [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Visual Studio 2022 Community** (gratuit) avec les workloads :
  - `.NET desktop development` (WinForms + WPF)
  - `.NET Multi-platform App UI` (MAUI)
- **OU** VS Code + extension [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

Vérifier l'installation :
```bash
dotnet --version   # doit afficher 8.0.xxx
```

---

## Structure du dépôt

```
DesktopDotNet/
│
├── 📁 calculatrice/          # TUTO 1 — Calculatrice WinForms
├── 📁 WinFormsTest/          # Projet test WinForms de départ
│
├── 📁 NotesApp/              # TUTO 2 — Gestionnaire de Notes WPF + MVVM + SQLite
├── 📁 ContactsManager/       # TUTO 2b — Gestionnaire de Contacts WPF + SQLite
├── 📁 TestWPF/               # Projet test WPF de départ
├── 📁 WPFTest/               # Projet test WPF additionnel
│
├── 📁 MauiTodo/              # TUTO 3 — Todo App MAUI (cross-platform)
├── 📁 ToDoApp/               # Variante Todo MAUI
│
├── 📁 TestConsole/           # Projet console de test C#
├── 📁 TestConsole2/          # Projet console de test C# #2
│
├── TutoWinforms_calculatrice.md   # Guide Tuto 1
├── TutoWPFcontactsSqlite.md       # Guide Tuto 2
├── TutoWPFNotesAppAvances.md      # Guide Tuto 2 avancé (MVVM)
├── Tuto3-maui-todoApp.md          # Guide Tuto 3
│
├── ExerciceContactManager.md      # Exercice pratique WPF
├── CorrectionExerciceContactManager.md  # Correction de l'exercice
│
├── GuideSQLiteViewer.md           # Comment visualiser la BDD SQLite
├── README_Securite_SQL.md         # Injection SQL et bonnes pratiques
└── Guide_Installation.md          # Installation de l'environnement
```

---

## Les 3 frameworks Desktop .NET

| | WinForms | WPF | MAUI |
|---|---|---|---|
| **Année** | 2002 | 2006 | 2022 |
| **Interface** | Code / Designer | XAML | XAML |
| **Data Binding** | Non | ✅ Oui (puissant) | ✅ Oui |
| **Plateformes** | Windows | Windows | Win + Mac + iOS + Android |
| **Courbe** | Facile | Moyen | Moyen |
| **Idéal pour** | Outils internes rapides | Apps Windows pro | Apps cross-platform |

---

## TUTO 1 — Calculatrice WinForms

📄 **Guide :** [TutoWinforms_calculatrice.md](TutoWinforms_calculatrice.md)  
📁 **Code :** [calculatrice/](calculatrice/)

### Ce qu'on apprend
- Créer un projet WinForms depuis le terminal
- Construire une interface **entièrement par code C#** (sans le designer)
- `TableLayoutPanel` = l'équivalent de CSS Grid
- Gérer des événements `Click` avec des lambdas
- `switch` expression C# moderne
- Gestion d'état (opérande, opérateur, nouvelle saisie)
- Validation des entrées avec `double.TryParse()`
- Gestion de la division par zéro (`double.NaN`)

### Lancer le projet
```bash
cd calculatrice
dotnet run
```

### Concepts clés

```csharp
// Créer un bouton par code (pas de designer)
var btn = new Button { Text = "7", Dock = DockStyle.Fill };
grille.Controls.Add(btn, col, row);

// Brancher un événement Click
btn.Click += (sender, e) => {
    // logique ici
};

// Switch expression C# moderne
double resultat = operateur switch {
    "+" => a + b,
    "-" => a - b,
    "×" => a * b,
    "÷" => b != 0 ? a / b : double.NaN,
    _   => 0
};
```

---

## TUTO 2 — Gestionnaire de Contacts WPF + SQLite

📄 **Guide :** [TutoWPFcontactsSqlite.md](TutoWPFcontactsSqlite.md)  
📄 **Guide avancé (MVVM) :** [TutoWPFNotesAppAvances.md](TutoWPFNotesAppAvances.md)  
📁 **Code Contacts :** [ContactsManager/](ContactsManager/)  
📁 **Code Notes (MVVM) :** [NotesApp/](NotesApp/)

### Ce qu'on apprend
- Écrire des interfaces en **XAML** (l'équivalent du HTML pour Windows)
- `Grid` (comme CSS Grid), `StackPanel` (comme Flexbox)
- **Data Binding** : lier les données à l'interface automatiquement
- Pattern **MVVM** (Model — ViewModel — View)
- `INotifyPropertyChanged` : notifier l'UI quand une donnée change
- `ObservableCollection<T>` : liste qui met à jour le DataGrid automatiquement
- **SQLite** avec `Microsoft.Data.Sqlite`
- Paramètres SQL pour éviter les injections SQL

### Lancer les projets
```bash
# Gestionnaire de Contacts
cd ContactsManager
dotnet run

# Gestionnaire de Notes (MVVM avancé)
cd NotesApp
dotnet run
```

### Concepts clés

**XAML — Data Binding :**
```xml
<!-- Lier un TextBox à une propriété du ViewModel -->
<TextBox Text="{Binding Nom, UpdateSourceTrigger=PropertyChanged}"/>

<!-- Lier un DataGrid à une ObservableCollection -->
<DataGrid ItemsSource="{Binding Contacts}" SelectedItem="{Binding Selected}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Nom" Binding="{Binding Nom}"/>
    </DataGrid.Columns>
</DataGrid>

<!-- Lier un bouton à une Command -->
<Button Content="Ajouter" Command="{Binding AjouterCommand}"/>
```

**ViewModel avec INotifyPropertyChanged :**
```csharp
public class MainViewModel : BaseViewModel
{
    private string _nom = "";
    public string Nom
    {
        get => _nom;
        set => SetProperty(ref _nom, value); // notifie l'UI automatiquement
    }

    public ObservableCollection<Contact> Contacts { get; } = new();
}
```

**SQLite sécurisé :**
```csharp
// ✅ Toujours utiliser des paramètres
cmd.CommandText = "INSERT INTO contacts (nom) VALUES ($nom)";
cmd.Parameters.AddWithValue("$nom", contact.Nom);

// ❌ Ne JAMAIS concaténer — risque d'injection SQL
// cmd.CommandText = $"INSERT ... VALUES ('{contact.Nom}')";
```

### Architecture MVVM

```
NotesApp/
├── Models/
│   └── Note.cs              ← Les données pures
├── Data/
│   └── NoteRepository.cs    ← Parle à SQLite
├── ViewModels/
│   ├── BaseViewModel.cs     ← INotifyPropertyChanged réutilisable
│   ├── MainViewModel.cs     ← Logique + état + commands
│   └── RelayCommand.cs      ← Implémentation de ICommand
└── MainWindow.xaml          ← Interface (ne contient pas de logique)
```

### Exercice pratique
📄 [ExerciceContactManager.md](ExerciceContactManager.md) — À faire en autonomie  
📄 [CorrectionExerciceContactManager.md](CorrectionExerciceContactManager.md) — La solution

---

## TUTO 3 — Todo App MAUI (Cross-platform)

📄 **Guide :** [Tuto3-maui-todoApp.md](Tuto3-maui-todoApp.md)  
📁 **Code :** [MauiTodo/](MauiTodo/)

### Ce qu'on apprend
- Créer une app qui tourne sur **Windows, Mac, iOS et Android** avec un seul code
- Différences entre MAUI et WPF (contrôles équivalents)
- `VerticalStackLayout` (= StackPanel WPF), `CollectionView` (= ListView)
- Data Binding en MAUI (même principe qu'en WPF)
- Structure d'un projet MAUI (Platforms/, Resources/, AppShell...)

### Lancer le projet (Windows)
```bash
cd MauiTodo
dotnet run
```

### Correspondance des contrôles HTML / WPF / MAUI

| HTML | WPF | MAUI |
|------|-----|------|
| `<div>` flex col | `StackPanel` | `VerticalStackLayout` |
| `<div>` flex row | `StackPanel Horizontal` | `HorizontalStackLayout` |
| `<p>` / `<span>` | `TextBlock` | `Label` |
| `<input>` | `TextBox` | `Entry` |
| `<button>` | `Button` | `Button` |
| `<select>` | `ComboBox` | `Picker` |
| `<ul>` | `ListBox` | `CollectionView` |

---

## Ressources complémentaires

| Fichier | Description |
|---------|-------------|
| [GuideSQLiteViewer.md](GuideSQLiteViewer.md) | Comment ouvrir et inspecter un fichier `.db` SQLite |
| [README_Securite_SQL.md](README_Securite_SQL.md) | Les injections SQL expliquées avec des exemples C# |
| [Guide_Installation.md](Guide_Installation.md) | Installation complète (Visual Studio, VS Code, .NET SDK) |

---

## Commandes utiles

```bash
# Créer un projet
dotnet new winforms -n MonProjet    # WinForms
dotnet new wpf -n MonProjet         # WPF
dotnet new maui -n MonProjet        # MAUI
dotnet new console -n MonProjet     # Console

# Ajouter SQLite
dotnet add package Microsoft.Data.Sqlite

# Lancer
dotnet run

# Publier un .exe autonome (Windows)
dotnet publish -c Release --self-contained true -r win-x64 -p:PublishSingleFile=true
```

---

## Traductions rapides JS/Java → C#

| JavaScript / Java | C# |
|-------------------|----|
| `console.log()` / `System.out.println()` | `Console.WriteLine()` |
| `` `Bonjour ${nom}` `` | `$"Bonjour {nom}"` |
| `addEventListener('click')` | `.Click +=` |
| `alert()` | `MessageBox.Show()` |
| `array.push()` / `ArrayList.add()` | `list.Add()` |
| `useState([])` / `reactive([])` | `ObservableCollection<T>` |
| `v-model` / `useState` + binding | `{Binding ...}` |
| `fetch()` | `HttpClient` |
| `better-sqlite3` | `Microsoft.Data.Sqlite` |
| `extends` (Java) | `: BaseClass` |
| `@Override` | `override` |

---

*Cours réalisé avec ❤️ — YNOV Campus Nanterre — Bachelor 3 Info — 2025/2026*
