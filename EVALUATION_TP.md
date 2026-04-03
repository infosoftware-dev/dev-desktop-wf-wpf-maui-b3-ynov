# TP FINAL — Développement Desktop .NET (C#)
## Gestionnaire de Livres — WinForms · WPF · MAUI · SQLite

**Bachelor 3 Informatique — YNOV Campus Nanterre — 2025/2026**

---

> **Durée : 14h00 → 16h45 (2h45)**  
> **Rendu :** un dossier nommé `TP_NOM_Prenom` contenant :
> - Votre code source (dossier projet)
> - Un fichier Word **OU** un sous-dossier `captures/` avec toutes les captures d'écran
>
> **Important :** toutes les captures doivent montrer **l'écran entier** (pas seulement la fenêtre).  
> Utilisez `Touche Windows + Maj + S` ou `Impr. Écran` → collez dans Word ou enregistrez en PNG.

---

## Contexte

Vous devez réaliser une **application de gestion de livres** en .NET C#.  
L'application permettra à un utilisateur de gérer une bibliothèque personnelle.

Chaque livre possède les informations suivantes :
- **Titre** (obligatoire)
- **Auteur** (obligatoire)
- **Année** (nombre entier)
- **Lu** (oui / non — booléen)

---

## PARTIE 1 — WinForms : Interface de saisie (5 points)

### Objectif
Créer une application WinForms simple pour **ajouter et afficher des livres**.

### Étapes

**1.1 — Créer le projet WinForms**

```bash
dotnet new winforms -n GestionnaireLivres
cd GestionnaireLivres
dotnet run
```

**1.2 — Construire l'interface**

Votre formulaire `Form1` doit contenir (créés **par code C#**, pas par le designer) :
- Un `Label` "Titre :"
- Un `TextBox` pour saisir le titre
- Un `Label` "Auteur :"
- Un `TextBox` pour saisir l'auteur
- Un `Label` "Année :"
- Un `TextBox` pour saisir l'année
- Une `CheckBox` "Lu ?"
- Un `Button` "Ajouter"
- Une `ListBox` qui affiche les livres ajoutés sous la forme : `Titre — Auteur (2023) ✓`

**1.3 — Brancher l'événement Click**

Quand l'utilisateur clique sur "Ajouter" :
1. Récupérer les valeurs des champs
2. Vérifier que Titre et Auteur ne sont pas vides (sinon : `MessageBox.Show("...")`)
3. Formater et ajouter l'entrée dans la `ListBox`
4. Vider les champs

### Captures d'écran à fournir

| # | Ce que doit montrer la capture |
|---|-------------------------------|
| **C1** | L'application lancée avec au moins **3 livres dans la ListBox** |
| **C2** | La `MessageBox` d'erreur quand on clique "Ajouter" avec le titre vide |

---

## PARTIE 2 — WPF + SQLite : Application complète MVVM (10 points)

### Objectif
Créer une application WPF avec **persistance SQLite** et le pattern **MVVM** pour gérer les livres.

### Étapes

**2.1 — Créer le projet WPF**

```bash
dotnet new wpf -n GestionnaireLivresWPF
cd GestionnaireLivresWPF
dotnet add package Microsoft.Data.Sqlite
dotnet run
```

**2.2 — Créer le Model**

Créer `Models/Livre.cs` :

```csharp
namespace GestionnaireLivresWPF.Models
{
    public class Livre
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Auteur { get; set; } = "";
        public int Annee { get; set; }
        public bool Lu { get; set; }
    }
}
```

**2.3 — Créer le Repository SQLite**

Créer `Data/LivreRepository.cs` avec les méthodes :
- `InitialiserBase()` — crée la table `livres` si elle n'existe pas
- `GetAll()` — retourne `List<Livre>`
- `Add(Livre livre)` — insère un livre
- `Delete(int id)` — supprime par id
- `Update(Livre livre)` — met à jour

> ⚠️ **Obligatoire :** utiliser des paramètres SQL (`$titre`, `$auteur`...) et **jamais** de concaténation de chaînes.

Exemple de structure de table :
```sql
CREATE TABLE IF NOT EXISTS livres (
    id      INTEGER PRIMARY KEY AUTOINCREMENT,
    titre   TEXT NOT NULL,
    auteur  TEXT NOT NULL,
    annee   INTEGER,
    lu      INTEGER NOT NULL DEFAULT 0
)
```

**2.4 — Créer le ViewModel**

Créer `ViewModels/BaseViewModel.cs` (avec `INotifyPropertyChanged`) et `ViewModels/MainViewModel.cs` contenant :

- `ObservableCollection<Livre> Livres` — liée au DataGrid
- `string Titre`, `string Auteur`, `int Annee`, `bool Lu` — liés aux champs de saisie
- `Livre? LivreSelectionne` — lié à `SelectedItem` du DataGrid
- `ICommand AjouterCommand`
- `ICommand SupprimerCommand`
- Méthode `ChargerLivres()` appelée au démarrage

**2.5 — Créer la View XAML**

`MainWindow.xaml` doit contenir :
- Un `Grid` avec **2 colonnes** (panneau formulaire gauche | liste droite)
- Colonne gauche : `StackPanel` avec les champs `TextBox`, une `CheckBox` "Lu ?", les boutons "Ajouter" et "Supprimer"
- Colonne droite : un `DataGrid` lié à `{Binding Livres}` avec les colonnes Titre, Auteur, Année, Lu
- Un `TextBlock` en bas affichant le nombre de livres (ex: "3 livre(s)")

**2.6 — Lier le DataContext**

Dans `MainWindow.xaml.cs` :
```csharp
DataContext = new MainViewModel();
```

### Captures d'écran à fournir

| # | Ce que doit montrer la capture |
|---|-------------------------------|
| **C3** | L'application WPF lancée avec **au moins 4 livres dans le DataGrid** (dont au moins 1 marqué "Lu") |
| **C4** | La boîte de dialogue de confirmation avant suppression d'un livre |
| **C5** | L'application après suppression : le livre n'apparaît plus dans le DataGrid |
| **C6** | L'application après **redémarrage** : les livres sont toujours là (preuve de la persistance SQLite) |

---

## PARTIE 3 — MAUI : Affichage cross-platform (5 points)

### Objectif
Créer une application MAUI qui affiche une **liste statique** de livres recommandés.

### Étapes

**3.1 — Créer le projet MAUI**

```bash
dotnet new maui -n LivresMAUI
cd LivresMAUI
dotnet run
```

**3.2 — Modifier `MainPage.xaml`**

Remplacer le contenu par une interface contenant :

- Un `Label` titre : "Mes livres recommandés" (grand, gras, centré)
- Une `CollectionView` ou plusieurs `Frame` affichant **au moins 3 livres** codés en dur dans le XAML, chacun avec :
  - Le titre en gras
  - L'auteur en italique
  - Un badge coloré "Lu" ou "Non lu" (un simple `Label` avec `BackgroundColor`)

Exemple de structure XAML pour un livre :
```xml
<Frame Margin="10" CornerRadius="8" BackgroundColor="#1E293B">
    <VerticalStackLayout>
        <Label Text="Le Nom de la Rose" FontSize="16" FontAttributes="Bold" TextColor="White"/>
        <Label Text="Umberto Eco" FontSize="13" FontAttributes="Italic" TextColor="#94A3B8"/>
        <Label Text="✓ Lu" BackgroundColor="#22C55E" TextColor="White" Padding="5,2"/>
    </VerticalStackLayout>
</Frame>
```

**3.3 — Modifier `MainPage.xaml.cs`** (optionnel — bonus)

Ajouter un bouton "Ajouter un livre" qui affiche une `DisplayAlert` avec le titre "Fonctionnalité à venir".

### Captures d'écran à fournir

| # | Ce que doit montrer la capture |
|---|-------------------------------|
| **C7** | L'application MAUI lancée sur **Windows** avec la liste des 3 livres visible |

---

## PARTIE 4 — Question de code à trou (bonus — 2 points)

Complétez les `___` dans cet extrait de ViewModel WPF :

```csharp
public class MainViewModel : ___
{
    private string _titre = "";
    public string Titre
    {
        get => _titre;
        set => ___(ref _titre, value);
    }

    public ObservableCollection<Livre> Livres { get; } = new();

    private Livre? _livreSelectionne;
    public Livre? LivreSelectionne
    {
        get => _livreSelectionne;
        set => SetProperty(ref _livreSelectionne, ___);
    }
}
```

> **Réponse attendue :** `BaseViewModel` · `SetProperty` · `value`

---

## Barème détaillé

| Partie | Critère | Points |
|--------|---------|--------|
| **Partie 1 — WinForms** | Projet qui se lance | 1 |
| | Interface créée par code (pas le designer) | 1 |
| | Événement Click branché et logique correcte | 1.5 |
| | Validation (MessageBox si champ vide) | 0.5 |
| | Captures C1 et C2 présentes et correctes | 1 |
| **Partie 2 — WPF + SQLite** | Model `Livre` correct | 0.5 |
| | Repository avec paramètres SQL (pas de concat) | 2 |
| | ViewModel avec INPC et ObservableCollection | 2 |
| | Interface XAML (Grid 2 colonnes, DataGrid lié) | 2 |
| | Persistance prouvée (C6 — redémarrage) | 1 |
| | Captures C3, C4, C5, C6 présentes | 1.5 |
| | Suppression avec confirmation | 1 |
| **Partie 3 — MAUI** | Projet qui se lance | 1 |
| | Interface XAML avec 3 livres | 2.5 |
| | Badge "Lu/Non lu" coloré | 1 |
| | Capture C7 présente | 0.5 |
| **Bonus** | Question de code à trou correcte | 2 |
| | **TOTAL** | **20 + 2** |

---

## Conseils

- Commencez par **Partie 1** (la plus rapide), puis **Partie 2**, puis **Partie 3**.
- Faites vos captures d'écran **au fur et à mesure**, pas à la fin.
- Si vous bloquez sur la Partie 2, passez à la Partie 3 et revenez.
- Un projet qui **se lance sans erreur** avec une interface incomplète vaut plus que du code qui ne compile pas.

---

## Rappels techniques utiles

**Créer un projet :**
```bash
dotnet new winforms -n MonProjet   # WinForms
dotnet new wpf -n MonProjet        # WPF
dotnet new maui -n MonProjet       # MAUI
dotnet add package Microsoft.Data.Sqlite  # SQLite
dotnet run                          # Lancer
```

**Data Binding XAML :**
```xml
<TextBox Text="{Binding Titre, UpdateSourceTrigger=PropertyChanged}"/>
<DataGrid ItemsSource="{Binding Livres}" SelectedItem="{Binding LivreSelectionne}"/>
<Button Content="Ajouter" Command="{Binding AjouterCommand}"/>
```

**Paramètres SQL sécurisés :**
```csharp
cmd.CommandText = "INSERT INTO livres (titre) VALUES ($titre)";
cmd.Parameters.AddWithValue("$titre", livre.Titre);  // ✅ Sécurisé
// JAMAIS : $"INSERT ... VALUES ('{livre.Titre}')"  ❌ Injection SQL
```

**Capture d'écran Windows :**
- `Windows + Maj + S` → sélectionner et coller dans Word
- `Impr. Écran` → coller dans Word ou Paint pour sauvegarder en PNG
- ⚠️ L'écran entier doit être visible (barre des tâches incluse)

---

*Bon courage ! Vous avez toutes les ressources nécessaires dans le dépôt GitHub du cours.*
