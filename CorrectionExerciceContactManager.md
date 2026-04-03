# Correction : Exercice Gestionnaire de Contacts

> **Développement Desktop .NET** — YNOV B3 — 2025/2026  
> **Réservé aux formateurs / Correction complète**

---

## Exercice 1 — Validation de l'email

### Modification dans `MainWindow.xaml.cs`

Ajouter cette méthode privée dans la classe `MainWindow` :

```csharp
/// <summary>
/// Vérifie qu'un email est valide (ou vide, car le champ est facultatif).
/// Utilise MailAddress pour une validation robuste.
/// </summary>
private bool EstEmailValide(string email)
{
    // Email vide = accepté (champ facultatif)
    if (string.IsNullOrWhiteSpace(email))
        return true;

    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}
```

Modifier `BtnAjouter_Click` pour appeler cette validation :

```csharp
private void BtnAjouter_Click(object sender, RoutedEventArgs e)
{
    // Validation nom (déjà présente)
    if (string.IsNullOrWhiteSpace(txtNom.Text))
    {
        MessageBox.Show("Le nom est obligatoire !", "Champ manquant",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        txtNom.Focus();
        return;
    }

    // ✅ NOUVEAU : Validation email
    if (!EstEmailValide(txtEmail.Text.Trim()))
    {
        MessageBox.Show("L'adresse email n'est pas valide !\nExemple correct : alice@gmail.com",
            "Email invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
        txtEmail.Focus();
        return;
    }

    var contact = new Contact
    {
        Nom = txtNom.Text.Trim(),
        Email = txtEmail.Text.Trim(),
        Telephone = txtTel.Text.Trim()
    };

    _repo.Add(contact);
    _contacts.Add(contact);
    ViderFormulaire();
    lblCount.Text = $"{_contacts.Count} contact(s)";
}
```

Faire la **même validation** dans `BtnModifier_Click` (après la vérification du nom).

---

## Exercice 2 — Champ "Catégorie"

### 1. `Models/Contact.cs` — Ajouter la propriété

```csharp
namespace ContactManager.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telephone { get; set; } = "";

        // ✅ NOUVEAU
        public string Categorie { get; set; } = "";
    }
}
```

---

### 2. `Data/ContactRepository.cs` — Mettre à jour toutes les requêtes

```csharp
using Microsoft.Data.Sqlite;
using ContactManager.Models;

namespace ContactManager.Data
{
    public class ContactRepository
    {
        private readonly string _connectionString;

        public ContactRepository(string dbPath = "contacts.db")
        {
            _connectionString = $"Data Source={dbPath}";
            InitialiserBase();
        }

        private void InitialiserBase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS contacts (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nom TEXT NOT NULL,
                    email TEXT NOT NULL DEFAULT '',
                    telephone TEXT NOT NULL DEFAULT '',
                    categorie TEXT NOT NULL DEFAULT ''
                )";
            // ✅ Ajout de la colonne categorie
            cmd.ExecuteNonQuery();
        }

        public List<Contact> GetAll()
        {
            var contacts = new List<Contact>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            // ✅ Ajout de categorie dans le SELECT
            cmd.CommandText = "SELECT id, nom, email, telephone, categorie FROM contacts ORDER BY nom";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(new Contact
                {
                    Id = reader.GetInt32(0),
                    Nom = reader.GetString(1),
                    Email = reader.GetString(2),
                    Telephone = reader.GetString(3),
                    Categorie = reader.GetString(4) // ✅ Nouveau
                });
            }

            return contacts;
        }

        public void Add(Contact contact)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            // ✅ Ajout de categorie dans INSERT
            cmd.CommandText = @"
                INSERT INTO contacts (nom, email, telephone, categorie)
                VALUES ($nom, $email, $tel, $cat)";
            cmd.Parameters.AddWithValue("$nom", contact.Nom);
            cmd.Parameters.AddWithValue("$email", contact.Email);
            cmd.Parameters.AddWithValue("$tel", contact.Telephone);
            cmd.Parameters.AddWithValue("$cat", contact.Categorie); // ✅ Nouveau
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid()";
            contact.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Update(Contact contact)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            // ✅ Ajout de categorie dans UPDATE
            cmd.CommandText = @"
                UPDATE contacts 
                SET nom = $nom, email = $email, telephone = $tel, categorie = $cat
                WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", contact.Id);
            cmd.Parameters.AddWithValue("$nom", contact.Nom);
            cmd.Parameters.AddWithValue("$email", contact.Email);
            cmd.Parameters.AddWithValue("$tel", contact.Telephone);
            cmd.Parameters.AddWithValue("$cat", contact.Categorie); // ✅ Nouveau
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM contacts WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public List<Contact> Search(string query)
        {
            var contacts = new List<Contact>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            // ✅ Ajout de categorie dans SELECT et dans la condition WHERE
            cmd.CommandText = @"
                SELECT id, nom, email, telephone, categorie FROM contacts
                WHERE nom LIKE '%' || $q || '%'
                   OR email LIKE '%' || $q || '%'
                   OR telephone LIKE '%' || $q || '%'
                   OR categorie LIKE '%' || $q || '%'
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
                    Telephone = reader.GetString(3),
                    Categorie = reader.GetString(4) // ✅ Nouveau
                });
            }

            return contacts;
        }
    }
}
```

---

### 3. `MainWindow.xaml` — Ajouter le ComboBox et la colonne

Dans le formulaire (StackPanel gauche), après `txtTel` :

```xml
<!-- Champ CATÉGORIE — à ajouter après le champ Téléphone -->
<TextBlock Text="Catégorie" FontWeight="SemiBold" Margin="0,0,0,5"/>
<ComboBox x:Name="cmbCategorie" 
          Padding="8" FontSize="14" Margin="0,0,0,15"
          SelectedIndex="0">
    <ComboBoxItem Content=""/>
    <ComboBoxItem Content="Famille"/>
    <ComboBoxItem Content="Travail"/>
    <ComboBoxItem Content="Amis"/>
    <ComboBoxItem Content="Autre"/>
</ComboBox>
```

Dans le DataGrid, ajouter une colonne :

```xml
<DataGrid.Columns>
    <DataGridTextColumn Header="Nom" Binding="{Binding Nom}" Width="*"/>
    <DataGridTextColumn Header="Email" Binding="{Binding Email}" Width="*"/>
    <DataGridTextColumn Header="Tél" Binding="{Binding Telephone}" Width="120"/>
    <!-- ✅ Nouvelle colonne -->
    <DataGridTextColumn Header="Catégorie" Binding="{Binding Categorie}" Width="100"/>
</DataGrid.Columns>
```

---

### 4. `MainWindow.xaml.cs` — Lire et remplir le ComboBox

Dans `BtnAjouter_Click`, lire la catégorie :

```csharp
var contact = new Contact
{
    Nom = txtNom.Text.Trim(),
    Email = txtEmail.Text.Trim(),
    Telephone = txtTel.Text.Trim(),
    // ✅ Lire la catégorie du ComboBox
    Categorie = (cmbCategorie.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? ""
};
```

Dans `BtnModifier_Click`, mettre à jour la catégorie :

```csharp
_selectedContact.Nom = txtNom.Text.Trim();
_selectedContact.Email = txtEmail.Text.Trim();
_selectedContact.Telephone = txtTel.Text.Trim();
_selectedContact.Categorie = (cmbCategorie.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? ""; // ✅
```

Dans `DgContacts_SelectionChanged`, remplir le ComboBox :

```csharp
private void DgContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    _selectedContact = dgContacts.SelectedItem as Contact;

    if (_selectedContact != null)
    {
        txtNom.Text = _selectedContact.Nom;
        txtEmail.Text = _selectedContact.Email;
        txtTel.Text = _selectedContact.Telephone;

        // ✅ Sélectionner la catégorie dans le ComboBox
        cmbCategorie.SelectedIndex = 0; // Reset par défaut
        foreach (ComboBoxItem item in cmbCategorie.Items)
        {
            if (item.Content?.ToString() == _selectedContact.Categorie)
            {
                cmbCategorie.SelectedItem = item;
                break;
            }
        }
    }
}
```

Dans `ViderFormulaire()`, réinitialiser le ComboBox :

```csharp
private void ViderFormulaire()
{
    txtNom.Text = "";
    txtEmail.Text = "";
    txtTel.Text = "";
    cmbCategorie.SelectedIndex = 0; // ✅ Reset le ComboBox
    _selectedContact = null;
    dgContacts.SelectedItem = null;
}
```

---

## Exercice 3 — Export CSV

### `MainWindow.xaml` — Ajouter le bouton

Dans le panneau droit, modifier la ligne de titre pour ajouter un bouton à droite :

```xml
<!-- Remplacer le TextBlock titre seul par une Grid avec titre + bouton -->
<Grid Grid.Row="0" Margin="0,0,0,10">
    <TextBlock Text="Contacts"
               FontSize="22" FontWeight="Bold"
               Foreground="#1B2838"/>
    <Button x:Name="btnExportCsv" 
            Content="Exporter CSV"
            Click="BtnExportCsv_Click"
            HorizontalAlignment="Right"
            Padding="12,6"
            Background="#10B981" Foreground="White"
            FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>
</Grid>
```

### `MainWindow.xaml.cs` — Implémenter l'export

Ajouter l'using en haut du fichier :

```csharp
using Microsoft.Win32;
using System.IO;
```

Ajouter la méthode dans la classe :

```csharp
private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
{
    // 1. Ouvrir la boîte de dialogue "Enregistrer sous"
    var dialog = new SaveFileDialog
    {
        Filter = "Fichiers CSV (*.csv)|*.csv",
        DefaultExt = "csv",
        FileName = "contacts_export"
    };

    if (dialog.ShowDialog() != true)
        return; // L'utilisateur a annulé

    try
    {
        // 2. Récupérer tous les contacts
        var contacts = _repo.GetAll();

        // 3. Construire le contenu CSV
        var lignes = new List<string>();
        lignes.Add("Nom,Email,Telephone"); // En-tête

        foreach (var contact in contacts)
        {
            // Guillemets pour gérer les virgules dans les valeurs
            lignes.Add($"\"{contact.Nom}\",\"{contact.Email}\",\"{contact.Telephone}\"");
        }

        // 4. Écrire dans le fichier
        File.WriteAllLines(dialog.FileName, lignes, System.Text.Encoding.UTF8);

        // 5. Confirmer à l'utilisateur
        MessageBox.Show(
            $"{contacts.Count} contact(s) exporté(s) avec succès !\nFichier : {dialog.FileName}",
            "Export réussi",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Erreur lors de l'export : {ex.Message}",
            "Erreur",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
```

---

## Exercice 4 — Compteur par catégorie

> Prérequis : Exercice 2 terminé

### `Data/ContactRepository.cs` — Ajouter la méthode

```csharp
/// <summary>
/// Retourne le nombre de contacts par catégorie.
/// Exemple : { "Famille": 3, "Travail": 5, "": 2 }
/// </summary>
public Dictionary<string, int> GetCountByCategorie()
{
    var result = new Dictionary<string, int>();

    using var conn = new SqliteConnection(_connectionString);
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT categorie, COUNT(*) as nb 
        FROM contacts 
        GROUP BY categorie
        ORDER BY categorie";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        string categorie = reader.GetString(0);
        int nb = reader.GetInt32(1);
        result[categorie] = nb;
    }

    return result;
}
```

### `MainWindow.xaml` — Ajouter le TextBlock de résumé

Dans le panneau droit, remplacer le `TextBlock` du compteur existant :

```xml
<!-- Compteur principal -->
<TextBlock Grid.Row="3" x:Name="lblCount"
           Margin="0,10,0,2" Foreground="#718096" FontSize="12"/>

<!-- ✅ Nouveau : Compteur par catégorie -->
<TextBlock Grid.Row="3" x:Name="lblCountCategorie"
           Margin="0,24,0,0" Foreground="#9CA3AF" FontSize="11"
           FontStyle="Italic"/>
```

> **Note** : Pour empiler les deux TextBlock, tu peux les mettre dans un StackPanel dans la Row="3".

Voici la version propre avec StackPanel :

```xml
<StackPanel Grid.Row="3" Margin="0,10,0,0">
    <TextBlock x:Name="lblCount" Foreground="#718096" FontSize="12"/>
    <TextBlock x:Name="lblCountCategorie" Foreground="#9CA3AF" FontSize="11" FontStyle="Italic"/>
</StackPanel>
```

### `MainWindow.xaml.cs` — Mettre à jour l'affichage

Créer une méthode d'aide pour construire le texte de résumé :

```csharp
/// <summary>
/// Met à jour les deux compteurs (total + par catégorie).
/// </summary>
private void MettreAJourCompteurs()
{
    lblCount.Text = $"{_contacts.Count} contact(s)";

    var stats = _repo.GetCountByCategorie();

    if (stats.Count == 0)
    {
        lblCountCategorie.Text = "";
        return;
    }

    // Construire le résumé : "Famille: 3 | Travail: 5 | Amis: 2 | ..."
    var parties = new List<string>();

    string[] categories = { "Famille", "Travail", "Amis", "Autre" };
    foreach (var cat in categories)
    {
        if (stats.TryGetValue(cat, out int nb))
            parties.Add($"{cat}: {nb}");
    }

    // Contacts sans catégorie (clé vide "")
    if (stats.TryGetValue("", out int sansCat) && sansCat > 0)
        parties.Add($"Sans catégorie: {sansCat}");

    lblCountCategorie.Text = string.Join(" | ", parties);
}
```

Remplacer tous les endroits où `lblCount.Text = ...` est mis à jour par un appel à `MettreAJourCompteurs()` :

```csharp
// Dans ChargerContacts() :
MettreAJourCompteurs(); // Remplace : lblCount.Text = $"...";

// Dans BtnAjouter_Click() :
MettreAJourCompteurs(); // Remplace : lblCount.Text = $"...";

// Dans BtnSupprimer_Click() :
MettreAJourCompteurs(); // Remplace : lblCount.Text = $"...";

// Dans TxtRecherche_TextChanged() :
MettreAJourCompteurs(); // Remplace : lblCount.Text = $"...";
```

---

## Points de vigilance à vérifier avec les étudiants

| Point | Description |
|-------|-------------|
| Injection SQL | Les paramètres SQL (`$nom`, `$cat`...) doivent toujours être utilisés, jamais de concaténation |
| Suppression du .db | Si la structure de la table change (Exercice 2), l'ancien `contacts.db` doit être supprimé |
| Null checks | Vérifier `_selectedContact != null` avant toute opération de modification/suppression |
| Encodage CSV | `Encoding.UTF8` est important pour les caractères accentués |
| ComboBox reset | `ViderFormulaire()` doit bien réinitialiser le ComboBox, sinon l'ancienne valeur persiste |
