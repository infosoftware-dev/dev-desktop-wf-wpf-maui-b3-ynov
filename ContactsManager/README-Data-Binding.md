# Gestionnaire de Contacts — WPF

Application de gestion de contacts en WPF (.NET 8), avec SQLite comme base de données.

---

## Comment fonctionne le Data Binding

Le data binding est le mécanisme qui relie automatiquement les **données C#** à **l'affichage XAML**, sans avoir à mettre à jour l'UI manuellement colonne par colonne.

### Les 3 acteurs

```
Models/Contact.cs       MainWindow.xaml.cs        MainWindow.xaml
─────────────────       ──────────────────        ───────────────
  Le modèle               Le code-behind            Le design
  (les données)           (la logique)              (l'affichage)
```

---

### Étape 1 — Le modèle définit les propriétés

**`Models/Contact.cs`**

```csharp
public class Contact
{
    public int    Id        { get; set; }
    public string Nom       { get; set; } = "";
    public string Email     { get; set; } = "";
    public string Telephone { get; set; } = "";
    public string Categorie { get; set; } = "";
}
```

Ces **noms de propriétés** (`Nom`, `Email`, `Telephone`, `Categorie`) sont exactement ceux que le XAML utilisera dans `{Binding ...}`. Ils doivent correspondre à la lettre près.

---

### Étape 2 — Le code-behind branche la liste sur le DataGrid

**`MainWindow.xaml.cs`**

```csharp
// Une liste observable de contacts (expliqué plus bas)
private ObservableCollection<Contact> _contacts = new();

private void ChargerContacts()
{
    var liste = _repo.GetAll(); // Récupère les contacts depuis la BDD
    _contacts = new ObservableCollection<Contact>(liste);

    // ← C'est ici que le binding se crée :
    // On dit au DataGrid "ta source de données, c'est _contacts"
    dgContacts.ItemsSource = _contacts;
}
```

À partir de ce moment, le `DataGrid` sait qu'il doit afficher des objets de type `Contact`.

---

### Étape 3 — Le XAML indique quelle propriété afficher dans chaque colonne

**`MainWindow.xaml`**

```xml
<DataGrid x:Name="dgContacts" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Nom"       Binding="{Binding Nom}"/>
        <DataGridTextColumn Header="Email"     Binding="{Binding Email}"/>
        <DataGridTextColumn Header="Tél"       Binding="{Binding Telephone}"/>
        <DataGridTextColumn Header="Catégorie" Binding="{Binding Categorie}"/>
    </DataGrid.Columns>
</DataGrid>
```

`{Binding Nom}` signifie : *"pour chaque objet `Contact` dans `ItemsSource`, affiche sa propriété `Nom`"*.

Le `Header` est juste le titre de la colonne affiché à l'utilisateur — il n'a aucun lien avec la propriété C#.

---

### Schéma du flux complet

```
Contact.cs                 MainWindow.xaml.cs              MainWindow.xaml
──────────                 ──────────────────              ───────────────

public string Nom    ─┐                                 ┌─ {Binding Nom}
public string Email   │    dgContacts.ItemsSource       │  {Binding Email}
public string Tel     ├──►       = _contacts       ─────┤  {Binding Telephone}
public string Cat    ─┘                                 └─ {Binding Categorie}

       ▲                          │
       │    Les noms des          │
       └──  propriétés doivent ───┘
            correspondre exactement
```

---

### Pourquoi ObservableCollection et pas List ?

```csharp
// ❌ List classique — l'UI ne se rafraîchit PAS automatiquement
private List<Contact> _contacts = new();
_contacts.Add(contact); // Le DataGrid ne voit rien

// ✅ ObservableCollection — l'UI se met à jour toute seule
private ObservableCollection<Contact> _contacts = new();
_contacts.Add(contact);    // Le DataGrid ajoute la ligne
_contacts.Remove(contact); // Le DataGrid supprime la ligne
```

`ObservableCollection` notifie le DataGrid à chaque `Add()` ou `Remove()`. C'est l'équivalent du `state` en React : quand la collection change, l'affichage se recharge automatiquement.

---

### La limite actuelle : INotifyPropertyChanged

`ObservableCollection` gère l'ajout et la suppression d'éléments, mais **pas la modification des propriétés d'un objet existant**.

```csharp
// ❌ Le DataGrid ne voit pas ce changement
_selectedContact.Nom = "Nouveau nom";

// ✅ Solution utilisée ici : recharger toute la liste
ChargerContacts();
```

Pour que la modification soit automatiquement reflétée dans l'UI sans rechargement, il faudrait que `Contact` implémente l'interface `INotifyPropertyChanged` :

```csharp
public class Contact : INotifyPropertyChanged
{
    private string _nom = "";
    public string Nom
    {
        get => _nom;
        set
        {
            _nom = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nom)));
            // ↑ Notifie l'UI que la propriété Nom a changé
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
```

---

### Résumé

| Fichier | Rôle |
|---|---|
| `Models/Contact.cs` | Définit les propriétés (les "colonnes" de données) |
| `MainWindow.xaml.cs` | Crée la liste et l'attache au DataGrid via `ItemsSource` |
| `MainWindow.xaml` | Utilise `{Binding NomPropriété}` pour afficher chaque propriété |
| `ObservableCollection` | Notifie l'UI lors d'ajouts/suppressions |
| `INotifyPropertyChanged` | (optionnel) Notifie l'UI lors de modifications de propriétés |
