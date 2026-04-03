# Exercice : Améliorer le Gestionnaire de Contacts

> **Développement Desktop .NET** — YNOV B3 — 2025/2026  
> **Durée estimée** : 1h30  
> **Niveau** : Intermédiaire  
> **Prérequis** : avoir terminé le Tuto 2 (application ContactManager fonctionnelle)

---

## Contexte

Tu as maintenant une application de gestion de contacts fonctionnelle avec un CRUD complet et une recherche.  
Dans cet exercice, tu vas **étendre cette application** en ajoutant de nouvelles fonctionnalités.

> **Travail individuel** — chaque fonctionnalité est indépendante. Tu peux les faire dans l'ordre que tu veux.

---

## Exercice 1 — Validation de l'email (Facile ⭐)

### Objectif

Actuellement, l'application accepte n'importe quelle valeur dans le champ email, même une chaîne vide ou une valeur invalide comme `"pas-un-email"`.

### Ce qui est demandé

Dans `MainWindow.xaml.cs`, modifie la méthode `BtnAjouter_Click` (et aussi `BtnModifier_Click`) pour **valider l'email** si l'utilisateur en a saisi un.

**Règles de validation :**
- Le champ email est **facultatif** (on peut laisser vide)
- Si un email est saisi, il doit **contenir exactement un `@`** et **un `.` après le `@`**
- Si l'email est invalide, afficher un `MessageBox` d'avertissement et stopper l'ajout

### Exemple de comportement attendu

| Valeur saisie | Résultat |
|--------------|----------|
| *(vide)* | Accepté |
| `alice@gmail.com` | Accepté |
| `alice@` | Refusé — message d'erreur |
| `alicegmail.com` | Refusé — message d'erreur |
| `alice@gmail` | Refusé — message d'erreur |

### Indice

En C#, la classe `System.Net.Mail.MailAddress` permet de valider un email proprement :

```csharp
// Méthode pour valider un email
private bool EstEmailValide(string email)
{
    if (string.IsNullOrWhiteSpace(email)) 
        return true; // Email facultatif

    try
    {
        // Si la construction réussit, l'email est valide
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}
```

---

## Exercice 2 — Champ "Catégorie" (Intermédiaire ⭐⭐)

### Objectif

Ajouter un champ **Catégorie** au contact pour permettre de classer les contacts (Famille, Travail, Amis, Autre).

### Ce qui est demandé

**1. Modifier le modèle** (`Models/Contact.cs`)  
Ajouter une propriété `Categorie` de type `string`.

**2. Modifier la base de données** (`Data/ContactRepository.cs`)  
- Ajouter la colonne `categorie` dans le `CREATE TABLE`
- Mettre à jour les requêtes `GetAll`, `Add`, `Update`, `Search` pour inclure cette colonne

**3. Modifier l'interface** (`MainWindow.xaml`)  
- Ajouter un `ComboBox` dans le formulaire (sous le champ Téléphone) avec les options : `Famille`, `Travail`, `Amis`, `Autre`
- Ajouter une colonne "Catégorie" dans le `DataGrid`

**4. Modifier le code logique** (`MainWindow.xaml.cs`)  
- Lire la valeur sélectionnée du `ComboBox` lors de l'ajout/modification
- Remplir le `ComboBox` lors de la sélection d'un contact dans le DataGrid
- Réinitialiser le `ComboBox` dans `ViderFormulaire()`

### Indice XAML pour le ComboBox

```xml
<!-- Dans le StackPanel du formulaire, après le champ Téléphone -->
<TextBlock Text="Catégorie" FontWeight="SemiBold" Margin="0,0,0,5"/>
<ComboBox x:Name="cmbCategorie" Padding="8" FontSize="14" Margin="0,0,0,15">
    <ComboBoxItem Content="Famille"/>
    <ComboBoxItem Content="Travail"/>
    <ComboBoxItem Content="Amis"/>
    <ComboBoxItem Content="Autre"/>
</ComboBox>
```

### Indice C# pour lire la valeur du ComboBox

```csharp
// Lire la catégorie sélectionnée
string categorie = (cmbCategorie.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

// Sélectionner un item par son texte
foreach (ComboBoxItem item in cmbCategorie.Items)
{
    if (item.Content.ToString() == contact.Categorie)
    {
        cmbCategorie.SelectedItem = item;
        break;
    }
}
```

> **Attention** : Si tu as déjà un fichier `contacts.db` existant, supprime-le pour que la nouvelle colonne soit créée. Les données existantes seront perdues.

---

## Exercice 3 — Export CSV (Intermédiaire ⭐⭐)

### Objectif

Ajouter un bouton **"Exporter en CSV"** qui sauvegarde tous les contacts dans un fichier `.csv` que l'on peut ouvrir avec Excel.

### Ce qui est demandé

**1. Ajouter un bouton** dans `MainWindow.xaml`  
Dans le panneau droit (liste des contacts), ajouter un bouton "Exporter CSV" à côté du titre.

**2. Implémenter la méthode** dans `MainWindow.xaml.cs`  
- Récupérer tous les contacts depuis le repository
- Créer un fichier CSV avec une ligne d'en-tête (`Nom,Email,Telephone`)
- Puis une ligne par contact
- Utiliser `SaveFileDialog` pour laisser l'utilisateur choisir où sauvegarder le fichier
- Afficher un message de confirmation une fois l'export terminé

### Format CSV attendu

```
Nom,Email,Telephone
Alice Dupont,alice@example.com,0612345678
Bob Martin,bob@example.com,0698765432
```

### Indice — SaveFileDialog

```csharp
using Microsoft.Win32;

// Ouvrir la boîte de dialogue "Enregistrer sous"
var dialog = new SaveFileDialog
{
    Filter = "Fichiers CSV (*.csv)|*.csv",
    DefaultExt = "csv",
    FileName = "contacts"
};

if (dialog.ShowDialog() == true)
{
    // dialog.FileName contient le chemin choisi par l'utilisateur
    string chemin = dialog.FileName;
    // ... écrire le fichier
}
```

### Indice — Écrire un fichier texte

```csharp
// Créer le contenu CSV
var lignes = new List<string>();
lignes.Add("Nom,Email,Telephone"); // En-tête

foreach (var contact in contacts)
{
    // Échapper les virgules dans les valeurs avec des guillemets
    lignes.Add($"\"{contact.Nom}\",\"{contact.Email}\",\"{contact.Telephone}\"");
}

// Écrire dans le fichier (File.WriteAllLines crée le fichier automatiquement)
File.WriteAllLines(chemin, lignes, System.Text.Encoding.UTF8);
```

---

## Exercice 4 — Compteur de contacts par catégorie (Difficile ⭐⭐⭐)

> **Prérequis** : avoir fait l'Exercice 2 (champ Catégorie)

### Objectif

Afficher un résumé en bas de l'application avec le nombre de contacts par catégorie.

### Ce qui est demandé

**1. Ajouter une méthode dans le Repository** (`Data/ContactRepository.cs`)  
Créer une méthode `GetCountByCategorie()` qui retourne un dictionnaire `Dictionary<string, int>` avec le nombre de contacts par catégorie.

```csharp
// Exemple de résultat :
// { "Famille": 3, "Travail": 5, "Amis": 2, "Autre": 1 }
public Dictionary<string, int> GetCountByCategorie()
{
    // À implémenter
}
```

**2. Ajouter un `TextBlock`** dans `MainWindow.xaml`  
En bas du panneau droit, afficher le résumé après le compteur total.

**3. Mettre à jour l'affichage** dans `MainWindow.xaml.cs`  
Appeler `GetCountByCategorie()` et construire un texte comme :
```
12 contact(s) — Famille: 3 | Travail: 5 | Amis: 2 | Autre: 1 | Sans catégorie: 1
```

### Indice SQL

```sql
SELECT categorie, COUNT(*) as nb 
FROM contacts 
GROUP BY categorie
ORDER BY categorie
```

---

## Rendu

- Vérifie que ton application compile et fonctionne avec `dotnet run`
- Ouvre `contacts.db` dans SQLite Viewer et vérifie que tes données sont bien sauvegardées
- Montre le résultat à ton formateur

---

## Récapitulatif des exercices

| # | Exercice | Difficulté | Fichiers à modifier |
|---|----------|-----------|-------------------|
| 1 | Validation email | ⭐ | `MainWindow.xaml.cs` |
| 2 | Champ Catégorie | ⭐⭐ | `Contact.cs`, `ContactRepository.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs` |
| 3 | Export CSV | ⭐⭐ | `MainWindow.xaml`, `MainWindow.xaml.cs` |
| 4 | Compteur par catégorie | ⭐⭐⭐ | `ContactRepository.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs` |
