# TUTO 1 : Créer une Calculatrice avec WinForms (C#)

> **Développement Desktop .NET** — YNOV B3 — 2025/2026  
> **Durée estimée** : 1h30  
> **Niveau** : Débutant C#  
> **Prérequis** : .NET 8 SDK installé + un éditeur (Visual Studio ou VS Code)

---

## Objectifs

À la fin de ce tuto, tu sauras :

- Créer un projet WinForms depuis le terminal
- Créer des contrôles par code (Button, TextBox, Label)
- Utiliser un `TableLayoutPanel` (l'équivalent de CSS Grid)
- Gérer des événements `Click`
- Gérer un état interne (opérande, opérateur)
- Gérer les erreurs avec `try/catch` (division par zéro)

---

## Résultat attendu

Une calculatrice fonctionnelle avec :

- Un afficheur qui montre les chiffres
- Les boutons 0 à 9
- Les 4 opérations : +, -, ×, ÷
- Un bouton = pour calculer
- Un bouton C pour tout effacer
- Un bouton ⌫ pour supprimer le dernier chiffre
- Le bouton ± pour changer le signe
- Le bouton % pour calculer un pourcentage

---

## Étape 1 : Créer le projet

### Windows (PowerShell ou CMD)

```bash
mkdir Calculatrice
cd Calculatrice
dotnet new winforms -n Calculatrice
cd Calculatrice
```

### macOS / Linux

> **WinForms ne fonctionne que sur Windows.** Sur Mac/Linux, tu as deux options :
>
> 1. **Utiliser une VM Windows** (VirtualBox, Parallels, UTM)
> 2. **Utiliser WSL2 sur Windows** (si tu es sur un PC avec dual boot)
>
> Si tu es sur Mac/Linux et que tu ne peux pas utiliser Windows, passe directement au **Tuto 2 (WPF)** qui peut être adapté avec Avalonia (cross-platform).

### Vérifier que ça marche

```bash
dotnet run
```

Tu dois voir une fenêtre vide apparaître. Si c'est le cas, tout est bon !

---

## Étape 2 : Comprendre la structure du projet

Ouvre le dossier dans ton éditeur. Tu as ces fichiers :

```
Calculatrice/
├── Form1.cs              ← TON CODE (c'est ici qu'on travaille)
├── Form1.Designer.cs     ← Code auto-généré par le designer (on n'y touche pas)
├── Program.cs            ← Point d'entrée de l'app
├── Calculatrice.csproj   ← Configuration du projet
```

**Le fichier important c'est `Form1.cs`** — c'est là qu'on va écrire tout notre code.

---

## Étape 3 : Configurer la fenêtre

Ouvre `Form1.cs` et **remplace tout le contenu** par le code suivant :

```csharp
// Form1.cs — Calculatrice WinForms
// On importe les bibliothèques nécessaires
using System;
using System.Drawing;          // Pour les couleurs et les positions
using System.Windows.Forms;    // Pour les contrôles WinForms

namespace Calculatrice
{
    public partial class Form1 : Form
    {
        // === ÉTAT DE LA CALCULATRICE ===
        // Ces variables gardent en mémoire ce qui se passe
        private TextBox afficheur = null!;         // L'écran de la calculatrice
        private string operandePrecedent = "";     // Le nombre avant l'opérateur (ex: "5" dans 5+3)
        private string operateur = "";             // L'opérateur choisi (+, -, ×, ÷)
        private bool nouvelleEntree = true;        // Vrai = le prochain chiffre remplace l'afficheur

        public Form1()
        {
            InitializeComponent();

            // Configuration de la fenêtre principale
            Text = "Calculatrice";                                 // Titre de la fenêtre
            Size = new Size(320, 480);                             // Taille : 320 x 480 pixels
            StartPosition = FormStartPosition.CenterScreen;       // Centrer à l'écran
            FormBorderStyle = FormBorderStyle.FixedSingle;        // Empêcher le redimensionnement
            MaximizeBox = false;                                   // Pas de bouton agrandir
            BackColor = Color.FromArgb(30, 40, 56);               // Fond sombre (bleu-gris foncé)

            // Créer tous les éléments de l'interface
            CreerInterface();
        }

        // On va remplir ces méthodes dans les étapes suivantes
        private void CreerInterface() { }
    }
}
```

### Ce que fait ce code

- On crée une classe `Form1` qui hérite de `Form` (la fenêtre Windows)
- On déclare les variables d'état : l'opérande précédent, l'opérateur, et un booléen pour savoir si on commence une nouvelle saisie
- Dans le constructeur, on configure la fenêtre (titre, taille, position, couleur)
- On appelle `CreerInterface()` pour créer les boutons (qu'on va coder juste après)

### Tester

```bash
dotnet run
```

Tu dois voir une fenêtre sombre de 320x480 pixels, centrée à l'écran, sans contenu pour l'instant.

---

## Étape 4 : Créer l'afficheur

L'afficheur c'est le "écran" en haut de la calculatrice qui montre les chiffres.

**Remplace la méthode `CreerInterface()`** par :

```csharp
private void CreerInterface()
{
    // =============================================
    // 1. L'AFFICHEUR (en haut de la fenêtre)
    // =============================================
    // C'est un TextBox en lecture seule, aligné à droite
    // Comme l'écran d'une vraie calculatrice
    afficheur = new TextBox
    {
        Text = "0",                                          // Affiche 0 au démarrage
        Font = new Font("Segoe UI", 32, FontStyle.Bold),    // Grande police
        ForeColor = Color.White,                             // Texte blanc
        BackColor = Color.FromArgb(40, 52, 71),              // Fond un peu plus clair que la fenêtre
        TextAlign = HorizontalAlignment.Right,               // Chiffres alignés à droite
        ReadOnly = true,                                     // L'utilisateur ne peut pas taper dedans
        Dock = DockStyle.Top,                                // Collé en haut de la fenêtre
        Height = 80,                                         // 80 pixels de haut
        BorderStyle = BorderStyle.None                       // Pas de bordure
    };

    // =============================================
    // 2. LA GRILLE DE BOUTONS
    // =============================================
    // TableLayoutPanel = comme CSS Grid !
    // On crée une grille de 4 colonnes × 5 lignes
    var grille = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,      // Remplit tout l'espace restant sous l'afficheur
        ColumnCount = 4,            // 4 colonnes
        RowCount = 5,               // 5 lignes
        Padding = new Padding(5),   // 5px de marge interne
        BackColor = Color.FromArgb(30, 40, 56)  // Même fond que la fenêtre
    };

    // Chaque colonne fait 25% de la largeur (4 × 25% = 100%)
    for (int i = 0; i < 4; i++)
        grille.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

    // Chaque ligne fait 20% de la hauteur (5 × 20% = 100%)
    for (int i = 0; i < 5; i++)
        grille.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

    // =============================================
    // 3. DISPOSITION DES BOUTONS
    // =============================================
    // Tableau 2D qui définit le texte de chaque bouton
    // Ligne par ligne, de haut en bas
    string[,] boutons = {
        { "C",  "±",  "%",  "÷" },    // Ligne 1 : fonctions + division
        { "7",  "8",  "9",  "×" },    // Ligne 2 : 7, 8, 9, multiplication
        { "4",  "5",  "6",  "-" },    // Ligne 3 : 4, 5, 6, soustraction
        { "1",  "2",  "3",  "+" },    // Ligne 4 : 1, 2, 3, addition
        { "0",  ".",  "⌫",  "=" }     // Ligne 5 : 0, point, effacer, égal
    };

    // Parcourir chaque case de la grille et créer un bouton
    for (int row = 0; row < 5; row++)
    {
        for (int col = 0; col < 4; col++)
        {
            string texte = boutons[row, col];
            var btn = CreerBouton(texte);       // Créer le bouton (méthode suivante)
            grille.Controls.Add(btn, col, row); // L'ajouter à la grille à la position (col, row)
        }
    }

    // =============================================
    // 4. AJOUTER À LA FENÊTRE
    // =============================================
    // IMPORTANT : l'ordre d'ajout compte !
    // D'abord la grille (elle se met en Fill), puis l'afficheur (il se met en Top)
    Controls.Add(grille);
    Controls.Add(afficheur);
}
```

> **Pourquoi `Controls.Add(grille)` AVANT `Controls.Add(afficheur)` ?**
>
> Avec `Dock`, l'ordre d'ajout détermine la priorité. L'afficheur (`Dock = Top`) doit être ajouté EN DERNIER pour qu'il soit bien en haut. La grille (`Dock = Fill`) remplit ce qui reste.

---

## Étape 5 : Créer les boutons avec style

Chaque bouton a une couleur différente selon son type. Ajoute cette méthode dans la classe `Form1` :

```csharp
/// <summary>
/// Crée un bouton de la calculatrice avec le bon style
/// </summary>
/// <param name="texte">Le texte affiché sur le bouton (ex: "7", "+", "=")</param>
/// <returns>Le bouton configuré</returns>
private Button CreerBouton(string texte)
{
    var btn = new Button
    {
        Text = texte,
        Dock = DockStyle.Fill,                                // Remplit toute la cellule de la grille
        Font = new Font("Segoe UI", 18, FontStyle.Bold),     // Police assez grande
        FlatStyle = FlatStyle.Flat,                           // Style plat (moderne)
        Margin = new Padding(3),                              // 3px d'espace entre les boutons
        Cursor = Cursors.Hand                                 // Curseur main au survol
    };
    btn.FlatAppearance.BorderSize = 0;  // Pas de bordure

    // === COULEURS SELON LE TYPE DE BOUTON ===
    if (texte == "=")
    {
        // Bouton ÉGAL = vert-teal (couleur d'action principale)
        btn.BackColor = Color.FromArgb(46, 196, 182);   // #2EC4B6
        btn.ForeColor = Color.White;
    }
    else if ("÷×-+".Contains(texte))
    {
        // Boutons OPÉRATEURS = orange
        btn.BackColor = Color.FromArgb(249, 115, 22);   // #F97316
        btn.ForeColor = Color.White;
    }
    else if (texte == "C" || texte == "±" || texte == "%" || texte == "⌫")
    {
        // Boutons FONCTIONS = gris foncé
        btn.BackColor = Color.FromArgb(55, 71, 95);
        btn.ForeColor = Color.White;
    }
    else
    {
        // Boutons CHIFFRES = gris-bleu
        btn.BackColor = Color.FromArgb(70, 90, 120);
        btn.ForeColor = Color.White;
    }

    // Brancher l'événement Click
    // Quand l'utilisateur clique sur CE bouton, la méthode BoutonClick sera appelée
    btn.Click += BoutonClick;

    return btn;
}
```

Il faut aussi ajouter la méthode `BoutonClick` (vide pour l'instant) pour que le code compile :

```csharp
/// <summary>
/// Appelée quand l'utilisateur clique sur n'importe quel bouton
/// </summary>
private void BoutonClick(object? sender, EventArgs e)
{
    // On va remplir ça dans l'étape suivante
}
```

### Tester

```bash
dotnet run
```

Tu dois voir la calculatrice avec tous les boutons colorés. Les chiffres ne fonctionnent pas encore, c'est normal.

---

## Étape 6 : La logique — gérer les clics

C'est le cœur de la calculatrice. **Remplace la méthode `BoutonClick`** par :

```csharp
/// <summary>
/// Gère le clic sur n'importe quel bouton de la calculatrice.
/// On récupère le texte du bouton cliqué et on agit en fonction.
/// </summary>
private void BoutonClick(object? sender, EventArgs e)
{
    // Vérifier que c'est bien un bouton qui a déclenché l'événement
    if (sender is not Button btn) return;

    // Récupérer le texte du bouton (ex: "7", "+", "=", "C"...)
    string texte = btn.Text;

    switch (texte)
    {
        // =============================================
        // BOUTON C : Tout effacer (Clear)
        // =============================================
        case "C":
            operandePrecedent = "";
            operateur = "";
            afficheur.Text = "0";
            nouvelleEntree = true;
            break;

        // =============================================
        // BOUTON ⌫ : Effacer le dernier caractère (Backspace)
        // =============================================
        case "⌫":
            if (afficheur.Text.Length > 1)
            {
                // Enlever le dernier caractère
                // "123" → "12"
                afficheur.Text = afficheur.Text[..^1];
            }
            else
            {
                // Si un seul chiffre, remettre à 0
                afficheur.Text = "0";
            }
            break;

        // =============================================
        // BOUTON ± : Changer le signe (positif ↔ négatif)
        // =============================================
        case "±":
            if (afficheur.Text != "0" && afficheur.Text != "")
            {
                if (afficheur.Text.StartsWith('-'))
                    afficheur.Text = afficheur.Text[1..];     // "-5" → "5"
                else
                    afficheur.Text = "-" + afficheur.Text;    // "5" → "-5"
            }
            break;

        // =============================================
        // BOUTON % : Calculer le pourcentage
        // =============================================
        case "%":
            if (double.TryParse(afficheur.Text, out double pct))
            {
                afficheur.Text = (pct / 100).ToString();
            }
            break;

        // =============================================
        // OPÉRATEURS : +, -, ×, ÷
        // =============================================
        case "+":
        case "-":
        case "×":
        case "÷":
            // Sauvegarder le nombre actuel et l'opérateur
            operandePrecedent = afficheur.Text;
            operateur = texte;
            nouvelleEntree = true;  // Le prochain chiffre remplacera l'afficheur
            break;

        // =============================================
        // BOUTON = : Calculer le résultat
        // =============================================
        case "=":
            Calculer();
            break;

        // =============================================
        // BOUTON . : Point décimal
        // =============================================
        case ".":
            if (nouvelleEntree)
            {
                afficheur.Text = "0.";
                nouvelleEntree = false;
            }
            else if (!afficheur.Text.Contains('.'))
            {
                // N'ajouter le point que s'il n'y en a pas déjà un
                afficheur.Text += ".";
            }
            break;

        // =============================================
        // CHIFFRES : 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        // =============================================
        default:
            if (nouvelleEntree)
            {
                // Nouvelle saisie → remplacer l'afficheur
                afficheur.Text = texte;
                nouvelleEntree = false;
            }
            else
            {
                // Continuer la saisie → ajouter le chiffre
                if (afficheur.Text == "0")
                    afficheur.Text = texte;     // Remplacer le 0 initial
                else
                    afficheur.Text += texte;    // Ajouter à la suite
            }
            break;
    }
}
```

### Ce que fait ce code

Le `switch` agit différemment selon le bouton cliqué :

- **C** : remet tout à zéro
- **⌫** : supprime le dernier caractère (comme la touche Backspace)
- **±** : inverse le signe du nombre
- **%** : divise par 100
- **Opérateurs** (+, -, ×, ÷) : sauvegarde le nombre et l'opérateur, prépare la prochaine saisie
- **=** : appelle la méthode `Calculer()` (étape suivante)
- **.** : ajoute un point décimal (un seul)
- **Chiffres** : ajoute le chiffre à l'afficheur

---

## Étape 7 : La méthode Calculer

Ajoute cette méthode dans la classe `Form1` :

```csharp
/// <summary>
/// Effectue le calcul entre operandePrecedent et l'afficheur,
/// selon l'opérateur choisi.
/// Exemple : si operandePrecedent = "5", operateur = "+", afficheur = "3"
///           → résultat = 8
/// </summary>
private void Calculer()
{
    // Vérifier qu'on a bien un opérateur et un nombre précédent
    if (string.IsNullOrEmpty(operateur) || string.IsNullOrEmpty(operandePrecedent))
        return;

    // Convertir les textes en nombres
    // TryParse retourne false si la conversion échoue (texte invalide)
    if (!double.TryParse(operandePrecedent, out double a) ||
        !double.TryParse(afficheur.Text, out double b))
    {
        afficheur.Text = "Erreur";
        nouvelleEntree = true;
        return;
    }

    // Calculer selon l'opérateur (switch expression C# moderne)
    double resultat = operateur switch
    {
        "+" => a + b,                                  // Addition
        "-" => a - b,                                  // Soustraction
        "×" => a * b,                                  // Multiplication
        "÷" => b != 0 ? a / b : double.NaN,          // Division (attention au zéro !)
        _ => 0                                         // Cas par défaut (ne devrait pas arriver)
    };

    // Vérifier si le résultat est valide
    if (double.IsNaN(resultat) || double.IsInfinity(resultat))
    {
        afficheur.Text = "Erreur";   // Division par zéro ou résultat invalide
    }
    else
    {
        afficheur.Text = resultat.ToString();
    }

    // Réinitialiser pour le prochain calcul
    operateur = "";
    operandePrecedent = "";
    nouvelleEntree = true;
}
```

> **Point important : la division par zéro**
>
> Si l'utilisateur fait `5 ÷ 0`, on ne peut pas diviser par zéro. Au lieu de planter, on retourne `double.NaN` (Not a Number) et on affiche "Erreur".

---

## Étape 8 : Tester la calculatrice

```bash
dotnet run
```

### Scénarios de test

Teste chaque scénario pour vérifier que tout fonctionne :

| Test | Action | Résultat attendu |
|------|--------|-----------------|
| Addition | 5 + 3 = | 8 |
| Soustraction | 10 - 4 = | 6 |
| Multiplication | 6 × 7 = | 42 |
| Division | 15 ÷ 3 = | 5 |
| Division par zéro | 5 ÷ 0 = | Erreur |
| Décimaux | 3.5 + 1.5 = | 5 |
| Négatif | 5 puis ± | -5 |
| Pourcentage | 50 puis % | 0.5 |
| Clear | (n'importe quoi) puis C | 0 |
| Backspace | 123 puis ⌫ | 12 |
| Double point | 3.5 puis . | 3.5 (pas de second point) |

---

## Code complet final

Voici le fichier `Form1.cs` complet, pour vérifier que tu n'as rien oublié :

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Calculatrice
{
    public partial class Form1 : Form
    {
        private TextBox afficheur = null!;
        private string operandePrecedent = "";
        private string operateur = "";
        private bool nouvelleEntree = true;

        public Form1()
        {
            InitializeComponent();
            Text = "Calculatrice";
            Size = new Size(320, 480);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(30, 40, 56);
            CreerInterface();
        }

        private void CreerInterface()
        {
            afficheur = new TextBox
            {
                Text = "0",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 52, 71),
                TextAlign = HorizontalAlignment.Right,
                ReadOnly = true,
                Dock = DockStyle.Top,
                Height = 80,
                BorderStyle = BorderStyle.None
            };

            var grille = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(30, 40, 56)
            };

            for (int i = 0; i < 4; i++)
                grille.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 5; i++)
                grille.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            string[,] boutons = {
                { "C",  "±",  "%",  "÷" },
                { "7",  "8",  "9",  "×" },
                { "4",  "5",  "6",  "-" },
                { "1",  "2",  "3",  "+" },
                { "0",  ".",  "⌫",  "=" }
            };

            for (int row = 0; row < 5; row++)
                for (int col = 0; col < 4; col++)
                    grille.Controls.Add(CreerBouton(boutons[row, col]), col, row);

            Controls.Add(grille);
            Controls.Add(afficheur);
        }

        private Button CreerBouton(string texte)
        {
            var btn = new Button
            {
                Text = texte,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            if (texte == "=")
            {
                btn.BackColor = Color.FromArgb(46, 196, 182);
                btn.ForeColor = Color.White;
            }
            else if ("÷×-+".Contains(texte))
            {
                btn.BackColor = Color.FromArgb(249, 115, 22);
                btn.ForeColor = Color.White;
            }
            else if (texte == "C" || texte == "±" || texte == "%" || texte == "⌫")
            {
                btn.BackColor = Color.FromArgb(55, 71, 95);
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.BackColor = Color.FromArgb(70, 90, 120);
                btn.ForeColor = Color.White;
            }

            btn.Click += BoutonClick;
            return btn;
        }

        private void BoutonClick(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            string texte = btn.Text;

            switch (texte)
            {
                case "C":
                    operandePrecedent = "";
                    operateur = "";
                    afficheur.Text = "0";
                    nouvelleEntree = true;
                    break;

                case "⌫":
                    if (afficheur.Text.Length > 1)
                        afficheur.Text = afficheur.Text[..^1];
                    else
                        afficheur.Text = "0";
                    break;

                case "±":
                    if (afficheur.Text != "0" && afficheur.Text != "")
                    {
                        if (afficheur.Text.StartsWith('-'))
                            afficheur.Text = afficheur.Text[1..];
                        else
                            afficheur.Text = "-" + afficheur.Text;
                    }
                    break;

                case "%":
                    if (double.TryParse(afficheur.Text, out double pct))
                        afficheur.Text = (pct / 100).ToString();
                    break;

                case "+": case "-": case "×": case "÷":
                    operandePrecedent = afficheur.Text;
                    operateur = texte;
                    nouvelleEntree = true;
                    break;

                case "=":
                    Calculer();
                    break;

                case ".":
                    if (nouvelleEntree)
                    {
                        afficheur.Text = "0.";
                        nouvelleEntree = false;
                    }
                    else if (!afficheur.Text.Contains('.'))
                        afficheur.Text += ".";
                    break;

                default:
                    if (nouvelleEntree)
                    {
                        afficheur.Text = texte;
                        nouvelleEntree = false;
                    }
                    else
                    {
                        if (afficheur.Text == "0")
                            afficheur.Text = texte;
                        else
                            afficheur.Text += texte;
                    }
                    break;
            }
        }

        private void Calculer()
        {
            if (string.IsNullOrEmpty(operateur) || string.IsNullOrEmpty(operandePrecedent))
                return;

            if (!double.TryParse(operandePrecedent, out double a) ||
                !double.TryParse(afficheur.Text, out double b))
            {
                afficheur.Text = "Erreur";
                nouvelleEntree = true;
                return;
            }

            double resultat = operateur switch
            {
                "+" => a + b,
                "-" => a - b,
                "×" => a * b,
                "÷" => b != 0 ? a / b : double.NaN,
                _ => 0
            };

            if (double.IsNaN(resultat) || double.IsInfinity(resultat))
                afficheur.Text = "Erreur";
            else
                afficheur.Text = resultat.ToString();

            operateur = "";
            operandePrecedent = "";
            nouvelleEntree = true;
        }
    }
}
```

---

## Ce que tu as appris

- **Créer un projet WinForms** avec `dotnet new winforms`
- **Créer des contrôles par code** : `TextBox`, `Button`, `TableLayoutPanel`
- **Le `TableLayoutPanel`** fonctionne comme CSS Grid (lignes + colonnes en pourcentage)
- **Les événements `Click`** fonctionnent comme `addEventListener('click')` en JavaScript
- **Le pattern `switch` expression** en C# pour des conditions propres
- **`double.TryParse()`** pour convertir du texte en nombre sans planter
- **La gestion d'erreurs** : division par zéro, entrées invalides

---

## Pour aller plus loin (optionnel)

Si tu as fini en avance, essaie d'ajouter :

- Le support du **clavier** (taper les chiffres au clavier au lieu de cliquer)
- Un **historique** des calculs affiché dans une liste
- La gestion des **calculs en chaîne** (5 + 3 + 2 sans appuyer sur = entre chaque)