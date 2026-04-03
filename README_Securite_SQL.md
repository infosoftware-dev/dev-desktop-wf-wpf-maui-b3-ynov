# Sécuriser une base de données SQL — Guide complet

> **Contexte** : Application ContactsManager — WPF .NET 8 + SQLite  
> **Question fréquente** : *"Pourquoi on se connecte sans user/password ? C'est pas sécurisé ?"*

---

## 1. Pourquoi il n'y a pas de user/password ici ?

Dans ce projet, on utilise **SQLite**. SQLite est une base de données **embarquée** : elle n'est pas un serveur réseau, c'est juste un **fichier** sur le disque (`contacts.db`).

```
Data Source=contacts.db   ← pas de host, pas de port, pas de user, pas de password
```

La sécurité d'accès au fichier repose entièrement sur le **système de fichiers** (Windows/Linux) :
- Seul l'utilisateur Windows qui possède le fichier peut l'ouvrir
- Aucune connexion réseau n'est possible → pas d'attaque distante

**C'est normal et voulu pour SQLite.** Ce n'est pas un oubli.

---

## 2. SQLite vs bases de données serveur — Tableau comparatif

| Critère | SQLite (ce projet) | MySQL / PostgreSQL / SQL Server |
|---|---|---|
| Type | Fichier local | Serveur réseau |
| User/Password | Non (géré par l'OS) | Oui, obligatoire |
| Connexion réseau | Non | Oui (TCP/IP) |
| Multi-utilisateurs simultanés | Limité | Oui |
| Cas d'usage | App desktop, mobile, embarqué | Site web, API, app d'entreprise |
| Risque d'attaque réseau | Aucun | Elevé si mal configuré |

### Exemple de connection string avec MySQL (pour comparaison)

```
Server=localhost;Database=contacts;User=monuser;Password=monpassword;
```

Avec un serveur SQL, le user/password est **indispensable** car n'importe qui sur le réseau pourrait se connecter sans ça.

---

## 3. Les vraies menaces sur SQLite — et comment s'en protéger

Même sans serveur, SQLite a ses propres risques. Voici les 3 attaques les plus courantes et comment on les évite.

### 3.1 Injection SQL — la menace principale

L'injection SQL consiste à manipuler les requêtes en injectant du SQL malveillant dans les champs de saisie.

#### Exemple d'attaque

Imaginons un champ de recherche. Si on écrit directement la valeur dans la requête :

```csharp
// CODE DANGEREUX — NE JAMAIS FAIRE CA
string recherche = txtRecherche.Text; // L'utilisateur tape : ' OR '1'='1
cmd.CommandText = $"SELECT * FROM contacts WHERE nom = '{recherche}'";

// La requête devient :
// SELECT * FROM contacts WHERE nom = '' OR '1'='1'
// → Retourne TOUS les contacts, même sans connaître de nom !
```

Un attaquant peut aussi supprimer toute la base :

```
Valeur saisie : '; DROP TABLE contacts; --
Requête générée : SELECT * FROM contacts WHERE nom = ''; DROP TABLE contacts; --'
→ La table est détruite !
```

#### Protection : les requêtes paramétrées

Le code du projet utilise **déjà cette protection** dans [Data/ContactRepository.cs](ContactsManager/Data/ContactRepository.cs) :

```csharp
// CORRECT : paramètres $nom, $email, $tel — les valeurs ne sont JAMAIS
// concaténées dans le SQL, SQLite les traite comme de la DATA, pas du code
cmd.CommandText = @"
    INSERT INTO contacts (nom, email, telephone)
    VALUES ($nom, $email, $tel)";
cmd.Parameters.AddWithValue("$nom", contact.Nom);
cmd.Parameters.AddWithValue("$email", contact.Email);
cmd.Parameters.AddWithValue("$tel", contact.Telephone);
```

Avec les paramètres, même si l'utilisateur tape `'; DROP TABLE contacts; --`, SQLite l'interprète comme un **nom de contact** et non comme du code SQL. L'attaque est neutralisée.

> **Règle d'or** : Ne jamais construire une requête SQL par concaténation de chaînes avec des données utilisateur. Toujours utiliser des paramètres.

---

### 3.2 Accès direct au fichier `.db`

Puisque SQLite est un fichier, n'importe qui ayant accès au disque peut l'ouvrir avec un logiciel comme **DB Browser for SQLite** et lire toutes les données.

#### Mesures de protection

**Option A — Emplacement sécurisé (recommandé pour ce projet)**

Au lieu de stocker `contacts.db` à la racine de l'exécutable, le stocker dans le dossier personnel de l'utilisateur :

```csharp
// Dans ContactRepository.cs — utiliser AppData au lieu du répertoire courant
string dossier = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string chemin = Path.Combine(dossier, "ContactManager", "contacts.db");
Directory.CreateDirectory(Path.GetDirectoryName(chemin)!);

var repo = new ContactRepository(chemin);
// → Le fichier sera dans : C:\Users\Alice\AppData\Roaming\ContactManager\contacts.db
// → Inaccessible aux autres utilisateurs Windows
```

**Option B — Chiffrement du fichier SQLite**

SQLite supporte le chiffrement via l'extension `SQLite Encryption Extension (SEE)` ou via la bibliothèque **SQLCipher**. Il faut ajouter le package `Microsoft.Data.Sqlite` avec l'option password :

```csharp
// Connection string avec mot de passe (nécessite SQLCipher)
_connectionString = $"Data Source={dbPath};Password=monMotDePasse;";
```

> Pour ce cours, l'**Option A** est suffisante et ne nécessite pas de librairie supplémentaire.

---

### 3.3 Données sensibles en clair

Si la base contient des mots de passe utilisateurs, il ne faut **jamais** les stocker tels quels.

#### Mauvais — mot de passe en clair

```sql
-- Dans la base : password = "azerty123"  ← si la base fuite, tout est compromis
```

#### Bon — hash avec sel (bcrypt)

```csharp
// Installer le package : dotnet add package BCrypt.Net-Next
string hash = BCrypt.Net.BCrypt.HashPassword("azerty123");
// Stocker le hash en base : "$2a$11$abcdefghijklmnopqrstuuXYZ..."

// Pour vérifier :
bool estValide = BCrypt.Net.BCrypt.Verify("azerty123", hashStocke);
```

> Dans le ContactsManager actuel, il n'y a pas de mot de passe à gérer — cette règle s'applique pour un vrai système d'authentification.

---

## 4. Récapitulatif — Ce qui est déjà sécurisé dans le projet

| Menace | Statut | Où dans le code |
|---|---|---|
| Injection SQL | Protégé | Paramètres `$nom`, `$email`, `$tel` dans `ContactRepository.cs` |
| Connexion réseau exposée | Aucun risque | SQLite = fichier local, pas de serveur |
| Accès fichier non autorisé | A améliorer | Stocker dans `AppData` plutôt qu'à la racine |
| Mots de passe en clair | N/A | Pas de mots de passe dans ce projet |

---

## 5. Pour aller plus loin — Sécurité avec un vrai serveur SQL

Si demain ce projet passait sur **MySQL** ou **SQL Server**, voici les bonnes pratiques essentielles :

### 5.1 Ne jamais mettre les credentials dans le code

```csharp
// DANGEREUX — les credentials sont dans le code source (visible sur GitHub !)
string conn = "Server=localhost;Database=contacts;User=root;Password=1234;";
```

```csharp
// CORRECT — lire depuis une variable d'environnement ou un fichier de config
string conn = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!;
// Ou depuis appsettings.json (avec gitignore sur ce fichier)
```

### 5.2 Principe du moindre privilège

Créer un utilisateur SQL dédié à l'application avec uniquement les droits nécessaires :

```sql
-- Sur MySQL par exemple
CREATE USER 'app_contacts'@'localhost' IDENTIFIED BY 'motDePasseComplexe!';

-- Donner uniquement les droits nécessaires (pas de DROP, pas de CREATE)
GRANT SELECT, INSERT, UPDATE, DELETE ON contacts_db.* TO 'app_contacts'@'localhost';

-- L'utilisateur ne peut PAS supprimer la base, créer des tables, etc.
```

### 5.3 Ne jamais utiliser `root` ou `sa` en production

Le compte `root` (MySQL) ou `sa` (SQL Server) a tous les droits. Une injection SQL avec ce compte peut détruire toute la base. Toujours créer un utilisateur dédié avec des droits limités.

---

## 6. Résumé des règles à retenir

1. **Toujours utiliser des requêtes paramétrées** — jamais de concaténation SQL avec des données utilisateur
2. **SQLite = fichier** — stocker dans `AppData` pour limiter l'accès aux autres utilisateurs
3. **Ne jamais mettre de credentials dans le code source** — utiliser des variables d'environnement
4. **Principe du moindre privilège** — l'utilisateur SQL ne doit avoir que les droits dont il a besoin
5. **Hacher les mots de passe** — jamais en clair, toujours avec bcrypt ou argon2
6. **Ne jamais utiliser root/sa** en production

---

## Références

- [OWASP Top 10 — Injection SQL](https://owasp.org/www-community/attacks/SQL_Injection)
- [Documentation Microsoft.Data.Sqlite](https://learn.microsoft.com/fr-fr/dotnet/standard/data/sqlite/)
- [BCrypt.Net-Next sur NuGet](https://www.nuget.org/packages/BCrypt.Net-Next)
