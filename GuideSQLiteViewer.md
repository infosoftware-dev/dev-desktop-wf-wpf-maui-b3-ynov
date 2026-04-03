# Guide : Visualiser la base SQLite dans VS Code

> **Développement Desktop .NET** — YNOV B3 — 2025/2026

---

## Pourquoi visualiser la base de données ?

Quand tu développes ton application, tu as besoin de **voir ce qu'il y a dans ta base de données** :
- Vérifier que tes données ont bien été sauvegardées
- Debugger un problème (pourquoi mon contact n'apparaît pas ?)
- Exécuter des requêtes SQL pour tester

L'extension **SQLite Viewer** dans VS Code te permet de faire ça directement depuis ton éditeur.

---

## Étape 1 : Installer l'extension SQLite Viewer

1. Ouvre **VS Code**
2. Va dans l'onglet **Extensions** (icône carrés à gauche, ou `Ctrl+Shift+X`)
3. Dans la barre de recherche, tape : **`SQLite Viewer`**
4. Installe l'extension de **Florian Klampfer** (c'est la plus populaire)

![Extension SQLite Viewer](https://i.imgur.com/placeholder.png)

> Il existe aussi **SQLite** de **alexcvzz** qui permet d'exécuter des requêtes SQL. Tu peux installer les deux !

---

## Étape 2 : Trouver le fichier contacts.db

Quand tu lances `dotnet run`, SQLite crée un fichier `contacts.db` dans le dossier de l'exécutable.

### Où se trouve le fichier ?

```
ContactManager/
├── bin/
│   └── Debug/
│       └── net8.0-windows/
│           └── contacts.db   ← C'est ici !
```

Le chemin complet depuis la racine du projet :
```
bin/Debug/net8.0-windows/contacts.db
```

> **Astuce** : Lance l'application au moins une fois (`dotnet run`) pour que le fichier soit créé !

---

## Étape 3 : Ouvrir le fichier dans SQLite Viewer

### Méthode 1 : Depuis l'explorateur VS Code (recommandée)

1. Dans VS Code, ouvre le panneau **Explorateur de fichiers** (`Ctrl+Shift+E`)
2. Navigue jusqu'à `bin/Debug/net8.0-windows/`
3. **Double-clique** sur `contacts.db`
4. L'extension SQLite Viewer s'ouvre automatiquement

### Méthode 2 : Glisser-déposer

1. Trouve le fichier `contacts.db` dans l'explorateur Windows
2. Fais-le glisser directement dans VS Code

---

## Étape 4 : Naviguer dans SQLite Viewer

Une fois le fichier ouvert, tu vois une interface avec :

```
┌─────────────────────────────────────────────────────┐
│  contacts.db                                        │
├─────────────────┬───────────────────────────────────┤
│  TABLES         │  TABLE: contacts                  │
│  ▼ contacts     │                                   │
│                 │  id │ nom      │ email    │ tel    │
│                 │  1  │ Alice    │ a@b.com  │ 06...  │
│                 │  2  │ Bob      │ b@c.com  │ 07...  │
└─────────────────┴───────────────────────────────────┘
```

### Les fonctionnalités principales

| Fonctionnalité | Comment faire |
|---------------|---------------|
| Voir toutes les tables | Panneau gauche |
| Voir le contenu d'une table | Cliquer sur le nom de la table |
| Trier par colonne | Cliquer sur l'en-tête de colonne |
| Filtrer les données | Utiliser la barre de recherche en haut |
| Rafraîchir les données | Icône rafraîchir ou fermer/rouvrir le fichier |

---

## Étape 5 : Rafraîchir après modification

> **Important** : SQLite Viewer affiche un **snapshot** du fichier. Si ton application modifie la base, tu dois rafraîchir manuellement.

### Comment rafraîchir

- Clique sur l'icône **rafraîchir** (🔄) dans l'onglet SQLite Viewer
- Ou : ferme l'onglet et rouvre le fichier `contacts.db`

---

## Bonus : Exécuter des requêtes SQL

Si tu as installé l'extension **SQLite** (alexcvzz), tu peux exécuter des requêtes SQL directement dans VS Code.

### Ouvrir la console SQL

1. `Ctrl+Shift+P` → tape **"SQLite: Open Database"**
2. Sélectionne `contacts.db`
3. En bas de VS Code, un panneau **SQLITE EXPLORER** apparaît
4. Fais un clic droit sur la table → **"New Query"**

### Exemples de requêtes utiles

```sql
-- Voir tous les contacts
SELECT * FROM contacts;

-- Compter le nombre de contacts
SELECT COUNT(*) AS total FROM contacts;

-- Rechercher par nom
SELECT * FROM contacts WHERE nom LIKE '%Alice%';

-- Voir les contacts sans téléphone
SELECT * FROM contacts WHERE telephone = '';

-- Trier par email
SELECT * FROM contacts ORDER BY email;

-- Voir la structure de la table
PRAGMA table_info(contacts);
```

### Exécuter une requête

- Écris ta requête dans l'éditeur SQL
- Appuie sur `Ctrl+Shift+Q` ou clique sur **"Run Query"**
- Les résultats s'affichent dans un tableau en dessous

---

## Récapitulatif

| Étape | Action |
|-------|--------|
| 1 | Installer l'extension **SQLite Viewer** dans VS Code |
| 2 | Lancer l'app au moins une fois (`dotnet run`) |
| 3 | Ouvrir `bin/Debug/net8.0-windows/contacts.db` dans VS Code |
| 4 | Double-cliquer sur la table `contacts` pour voir les données |
| 5 | Rafraîchir après chaque modification de l'app |

---

## En cas de problème

| Problème | Solution |
|----------|----------|
| Le fichier `contacts.db` n'existe pas | Lancer l'app au moins une fois avec `dotnet run` |
| Les données ne s'affichent pas | Rafraîchir l'onglet SQLite Viewer |
| L'extension ne s'ouvre pas | Vérifier que l'extension est bien installée et activée |
| Données non mises à jour | Refermer et rouvrir le fichier `.db` |
