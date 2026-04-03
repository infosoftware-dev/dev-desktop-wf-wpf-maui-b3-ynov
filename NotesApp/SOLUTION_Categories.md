# Solution — Ajouter une Categorie sur les notes

---

## Etape 1 — `Models/Note.cs`

Ajoute la propriete `Categorie` comme `Titre` :

```csharp
public string Categorie { get; set; } = "General";
```

---

## Etape 2 — `ViewModels/MainViewModel.cs`

### 2a — Ajoute la propriete

Copie exactement le meme schema que `Titre` :

```csharp
private string _categorie = "General";
public string Categorie
{
    get => _categorie;
    set => SetProperty(ref _categorie, value);
}
```

### 2b — Remplis le formulaire quand on selectionne une note

Dans le setter de `NoteSelectionnee`, ajoute :

```csharp
Categorie = value.Categorie;
```

### 2c — Inclure dans `Ajouter()`

```csharp
var note = new Note
{
    Titre = Titre.Trim(),
    Contenu = Contenu.Trim(),
    Categorie = Categorie       // ← ajoute
};
```

### 2d — Inclure dans `Modifier()`

```csharp
NoteSelectionnee.Categorie = Categorie;   // ← ajoute
```

### 2e — Reinitialiser dans `ViderFormulaire()`

```csharp
Categorie = "General";   // ← ajoute
```

---

## Etape 3 — `MainWindow.xaml`

Ajoute le champ apres le bloc Contenu, avant les boutons :

```xml
<TextBlock Text="Categorie" FontWeight="SemiBold" Margin="0,0,0,5"/>
<TextBox Text="{Binding Categorie, UpdateSourceTrigger=PropertyChanged}"/>
```

---

## Etape 4 — `Data/NoteRepository.cs`

### 4a — Migration dans `Init()`

Apres la creation de la table, ajoute :

```csharp
try
{
    cmd.CommandText = "ALTER TABLE notes ADD COLUMN categorie TEXT NOT NULL DEFAULT 'General'";
    cmd.ExecuteNonQuery();
}
catch (SqliteException) { /* colonne deja presente */ }
```

### 4b — `GetAll()` : lire la colonne

```csharp
cmd.CommandText = "SELECT id, titre, contenu, date_creation, categorie FROM notes ORDER BY date_creation DESC";

// Dans le while :
Categorie = r.GetString(4)
```

### 4c — `Add()` : sauvegarder la colonne

```csharp
cmd.CommandText = @"INSERT INTO notes (titre, contenu, date_creation, categorie)
                    VALUES ($titre, $contenu, $date, $categorie)";

cmd.Parameters.AddWithValue("$categorie", note.Categorie);
```

### 4d — `Update()` : mettre a jour la colonne

```csharp
cmd.CommandText = "UPDATE notes SET titre=$titre, contenu=$contenu, categorie=$categorie WHERE id=$id";

cmd.Parameters.AddWithValue("$categorie", note.Categorie);
```
