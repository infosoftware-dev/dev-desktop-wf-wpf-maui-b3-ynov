# Correction — Historique des calculs

---

## Résumé des modifications

| Fichier | Ce qui change |
|---------|--------------|
| `Form1.cs` | 2 nouveaux champs, `CreerInterface()` agrandie, `Calculer()` modifiée, 1 nouvelle méthode, case `"C"` mis à jour |

---

## Form1.cs — version complète corrigée

Voici **uniquement les parties modifiées ou ajoutées**, dans l'ordre.

---

### 1. Nouveaux champs (en haut de la classe)

```csharp
private TextBox afficheur = null!;
private string operandePrecedent = "";
private string operateur = "";
private bool nouvelleEntree = true;

// ← AJOUTÉ
private List<string> historique = new();
private FlowLayoutPanel panelHistorique = null!;
```

---

### 2. Constructeur — agrandir la fenêtre

```csharp
Size = new Size(320, 650);  // était 480
```

---

### 3. `CreerInterface()` — ajout du panneau en bas

Ajouter à la **fin** de `CreerInterface()`, avant `Controls.Add(grille)` :

```csharp
// =============================================
// 5. PANNEAU HISTORIQUE (en bas)
// =============================================
var panelWrapper = new Panel
{
    Dock = DockStyle.Bottom,
    Height = 160,
    BackColor = Color.FromArgb(20, 28, 40),
    Padding = new Padding(8, 4, 8, 4)
};

var titre = new Label
{
    Text = "Historique",
    ForeColor = Color.FromArgb(150, 160, 175),
    Font = new Font("Segoe UI", 9, FontStyle.Bold),
    Dock = DockStyle.Top,
    Height = 22
};

panelHistorique = new FlowLayoutPanel
{
    Dock = DockStyle.Fill,
    FlowDirection = FlowDirection.TopDown,
    AutoSize = false,
    BackColor = Color.FromArgb(20, 28, 40)
};

panelWrapper.Controls.Add(panelHistorique);
panelWrapper.Controls.Add(titre);

// L'ordre d'ajout à la Form est important
Controls.Add(panelWrapper);  // ← ajouter AVANT grille et afficheur
Controls.Add(grille);
Controls.Add(afficheur);
```

> **Note :** on ajoute `panelWrapper` en premier car `DockStyle.Bottom` est résolu avant `DockStyle.Fill`.

---

### 4. `Calculer()` — enregistrer dans l'historique

À la fin de `Calculer()`, juste avant `operateur = ""` :

```csharp
// ← AJOUTÉ : enregistrer dans l'historique
string symboleOp = operateur;  // on le récupère avant de le vider
string entree = $"{a} {symboleOp} {b} = {resultat}";
historique.Insert(0, entree);          // insérer au début (le plus récent en premier)
if (historique.Count > 5)
    historique.RemoveAt(5);            // garder au maximum 5 entrées
MettreAJourHistorique();
// fin de l'ajout

operateur = "";
operandePrecedent = "";
nouvelleEntree = true;
```

---

### 5. Nouvelle méthode `MettreAJourHistorique()`

À ajouter dans la classe, après `Calculer()` :

```csharp
/// <summary>
/// Rafraîchit le panneau historique à partir de la liste <see cref="historique"/>.
/// </summary>
private void MettreAJourHistorique()
{
    panelHistorique.Controls.Clear();

    foreach (string entree in historique)
    {
        var lbl = new Label
        {
            Text = entree,
            ForeColor = Color.FromArgb(200, 210, 225),
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Margin = new Padding(2, 1, 2, 1)
        };
        panelHistorique.Controls.Add(lbl);
    }
}
```

---

### 6. `BoutonClick` — vider l'historique sur C

Dans le `case "C"` du `switch` :

```csharp
case "C":
    operandePrecedent = "";
    operateur = "";
    afficheur.Text = "0";
    nouvelleEntree = true;
    // ← AJOUTÉ
    historique.Clear();
    MettreAJourHistorique();
    break;
```

---

## Résultat

Après ces modifications, chaque calcul validé avec `=` apparaît dans le panneau du bas :

```
Historique
  10 ÷ 2 = 5
  5 + 3 = 8
```

Le panneau se vide quand on appuie sur `C`.

---

## Erreurs fréquentes à éviter

| Erreur | Cause | Solution |
|--------|-------|----------|
| L'historique n'apparaît pas | `panelHistorique` est `null` au moment d'appeler `MettreAJourHistorique()` | Vérifier l'ordre d'initialisation dans `CreerInterface()` |
| La grille de boutons disparaît | Mauvais ordre des `Controls.Add(...)` | Ajouter `panelWrapper` AVANT `grille` et `afficheur` |
| Plus de 5 entrées | Oubli du `RemoveAt(5)` | Vérifier que `historique.Count > 5` avant de supprimer |
| Le texte affiché montre trop de décimales | `resultat.ToString()` non formaté | Utiliser `resultat.ToString("G10")` pour limiter |
