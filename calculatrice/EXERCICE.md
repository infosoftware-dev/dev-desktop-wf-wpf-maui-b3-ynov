# Exercice pratique — Historique des calculs

> **Prérequis :** avoir terminé le tutoriel de la calculatrice WinForms.

---

## Objectif

Ajouter un **panneau d'historique** en bas de la calculatrice qui affiche les **5 derniers calculs** effectués, sous la forme :

```
5 + 3 = 8
10 ÷ 2 = 5
7 × 7 = 49
```

---

## Aperçu du résultat attendu

```
┌───────────────────────────────┐
│                           128 │  ← afficheur
├───────────────────────────────┤
│  C    ±    %    ÷            │
│  7    8    9    ×            │
│  4    5    6    -            │
│  1    2    3    +            │
│  0    .    ⌫   =            │
├───────────────────────────────┤
│ Historique                    │  ← nouveau panneau
│  5 + 3 = 8                    │
│  10 ÷ 2 = 5                   │
│  7 × 7 = 49                   │
└───────────────────────────────┘
```

---

## Étapes à suivre

### Étape 1 — Agrandir la fenêtre

Dans le constructeur `Form1()`, changez la taille pour laisser de la place au panneau d'historique :

```csharp
Size = new Size(320, 650);  // était 480, passer à 650
```

---

### Étape 2 — Ajouter les champs nécessaires

En haut de la classe `Form1`, à côté des autres champs (`afficheur`, `operateur`…), ajoutez :

```csharp
private List<string> historique = new();        // stocke les entrées de l'historique
private FlowLayoutPanel panelHistorique = null!; // le panneau visuel
```

---

### Étape 3 — Créer le panneau dans `CreerInterface()`

À la fin de la méthode `CreerInterface()`, **avant** les lignes `Controls.Add(...)`, créez le panneau d'historique :

```csharp
// TODO : créer un Panel en bas de la fenêtre
// - Dock = DockStyle.Bottom
// - Hauteur = 155 pixels
// - Fond couleur : Color.FromArgb(20, 28, 40)
// - Ajouter un Label titre "Historique" en haut du panel
// - Ajouter le panel à Controls
```

**Indices :**
- Utilisez un `FlowLayoutPanel` pour que les lignes s'empilent automatiquement
- `FlowDirection = FlowDirection.TopDown`
- `AutoSize = false`

---

### Étape 4 — Enregistrer chaque calcul dans `Calculer()`

À la fin de la méthode `Calculer()`, juste avant de remettre `operateur = ""`, enregistrez le calcul dans l'historique.

Format de la chaîne à stocker :
```
"5 + 3 = 8"
```

**Contraintes :**
- Garder **au maximum 5 entrées**. Si on en a déjà 5, supprimer la plus ancienne.
- Appeler ensuite une méthode `MettreAJourHistorique()` (à créer à l'étape 5).

---

### Étape 5 — Créer la méthode `MettreAJourHistorique()`

Cette méthode doit :
1. Vider les contrôles du `panelHistorique` (sauf le titre)
2. Parcourir la liste `historique` en partant du plus récent
3. Pour chaque entrée, créer un `Label` avec le texte du calcul et l'ajouter au panel

**Style suggéré pour les labels :**
```csharp
ForeColor = Color.FromArgb(200, 200, 200)
Font = new Font("Segoe UI", 10)
AutoSize = true
```

---

### Étape 6 — Vider l'historique avec le bouton C

Dans le `switch` du gestionnaire `BoutonClick`, dans le `case "C"`, ajoutez le vidage de l'historique et appelez `MettreAJourHistorique()`.

---

## Critères de réussite

- [ ] Les calculs s'affichent dans le panneau après chaque appui sur `=`
- [ ] On ne voit jamais plus de 5 entrées à la fois
- [ ] Le plus récent est affiché en haut
- [ ] Appuyer sur `C` vide l'historique
- [ ] Le style est cohérent avec le reste de la calculatrice

---

## Bonus (facultatif)

- Afficher le calcul **en cours** dans un petit label au-dessus de l'afficheur (ex: `"5 +"` pendant qu'on tape le deuxième nombre)
- Cliquer sur une entrée de l'historique remet son résultat dans l'afficheur

---

*La correction est disponible dans le fichier `CORRECTION.md`.*
