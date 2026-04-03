# QCM — Développement Desktop .NET (C#)
## WinForms · WPF · MAUI · SQLite
### Bachelor 3 Informatique — YNOV Campus Nanterre — 2025/2026

> **20 questions** — 1 seule bonne réponse par question  
> Durée recommandée : 20 minutes

---

## PARTIE 1 — L'écosystème .NET et C# (Q1–Q5)

---

**Q1.** Quelle affirmation sur .NET est **correcte** ?

- A) .NET est propriétaire et uniquement disponible sur Windows depuis 2024  
- B) .NET est open source depuis 2016 (.NET Core) et fonctionne sur Windows, Mac et Linux  
- C) .NET ne supporte que le langage C++  
- D) .NET 8 est sorti en 2015  

> ✅ **Réponse : B**

---

**Q2.** En C#, quelle est la traduction correcte de `console.log("Bonjour")` en JavaScript ?

- A) `System.out.println("Bonjour")`  
- B) `print("Bonjour")`  
- C) `Console.WriteLine("Bonjour")`  
- D) `Debug.Log("Bonjour")`  

> ✅ **Réponse : C**

---

**Q3.** Quelle syntaxe C# est équivalente au template littéral JS `` `Bonjour ${nom} !` `` ?

- A) `"Bonjour " + nom + " !"` uniquement  
- B) `$"Bonjour {nom} !"`  
- C) `f"Bonjour {nom} !"`  
- D) `String.format("Bonjour %s !", nom)`  

> ✅ **Réponse : B**

---

**Q4.** Quel framework .NET est **cross-platform** (Windows + Mac + iOS + Android) ?

- A) WPF  
- B) WinForms  
- C) .NET MAUI  
- D) ASP.NET MVC  

> ✅ **Réponse : C**

---

**Q5.** En C#, quelle est la syntaxe correcte pour une propriété avec getter et setter automatiques ?

- A) `public string Nom;`  
- B) `public string Nom { get; set; }`  
- C) `private string Nom { get; set; }`  
- D) `string Nom => new();`  

> ✅ **Réponse : B**

---

## PARTIE 2 — WinForms (Q6–Q9)

---

**Q6.** Dans WinForms, quel fichier contient le code **auto-généré** de l'interface (positions, tailles des contrôles) ?

- A) `Program.cs`  
- B) `Form1.cs`  
- C) `Form1.Designer.cs`  
- D) `App.xaml`  

> ✅ **Réponse : C**

---

**Q7.** Quel contrôle WinForms est l'équivalent de `<table>` en HTML ?

- A) `ListBox`  
- B) `DataGridView`  
- C) `TableLayoutPanel`  
- D) `Panel`  

> ✅ **Réponse : B**

---

**Q8.** Comment brancher un événement Click sur un bouton en WinForms (C#) ?

- A) `btn.addEventListener("click", handler)`  
- B) `btn.OnClick = handler`  
- C) `btn.Click += BoutonClick`  
- D) `btn.Click = new Action(BoutonClick)`  

> ✅ **Réponse : C**

---

**Q9.** Voici un extrait de notre calculatrice. Que fait ce code ?

```csharp
double resultat = operateur switch
{
    "+" => a + b,
    "-" => a - b,
    "×" => a * b,
    "÷" => b != 0 ? a / b : double.NaN,
    _   => 0
};
```

- A) C'est une boucle qui parcourt les opérateurs  
- B) C'est une expression switch C# moderne qui calcule le résultat selon l'opérateur  
- C) C'est une méthode récursive  
- D) C'est une instruction if/else classique déguisée  

> ✅ **Réponse : B**

---

## PARTIE 3 — WPF et XAML (Q10–Q14)

---

**Q10.** En XAML, quel élément est l'équivalent de `<div>` avec `display: flex` (direction colonne) ?

- A) `<Grid>`  
- B) `<DockPanel>`  
- C) `<StackPanel>`  
- D) `<Canvas>`  

> ✅ **Réponse : C**

---

**Q11.** En XAML, quelle propriété correspond à `id="monElement"` en HTML ?

- A) `Name="monElement"`  
- B) `x:Key="monElement"`  
- C) `x:Name="monElement"`  
- D) `Tag="monElement"`  

> ✅ **Réponse : C**

---

**Q12.** Qu'est-ce que le **Data Binding** dans WPF ?

- A) Une technique pour connecter l'application à une base de données  
- B) Un mécanisme qui synchronise automatiquement les données entre le ViewModel et l'interface XAML  
- C) Une bibliothèque externe à installer via NuGet  
- D) Le fait de coder l'interface en C# plutôt qu'en XAML  

> ✅ **Réponse : B**

---

**Q13.** Dans le pattern **MVVM**, quel est le rôle du **ViewModel** ?

- A) Afficher l'interface (comme le JSX en React)  
- B) Stocker les données brutes (comme une table en BDD)  
- C) Faire le lien entre les données (Model) et l'interface (View), gérer la logique  
- D) Gérer les routes de navigation  

> ✅ **Réponse : C**

---

**Q14.** Quelle interface doit implémenter un ViewModel pour que le Data Binding fonctionne (notifications de changement) ?

- A) `IDisposable`  
- B) `INotifyPropertyChanged`  
- C) `IComparable`  
- D) `IEnumerable`  

> ✅ **Réponse : B**

---

## PARTIE 4 — SQLite et sécurité (Q15–Q17)

---

**Q15.** Pourquoi doit-on utiliser des **paramètres SQL** (`$nom`, `$email`) plutôt que la concaténation de chaînes ?

- A) Pour des raisons de performance uniquement  
- B) Pour éviter les injections SQL (SQL Injection)  
- C) Parce que SQLite ne supporte pas les chaînes de caractères  
- D) C'est facultatif, les deux approches sont équivalentes  

> ✅ **Réponse : B**

---

**Q16.** Quelle est la différence entre `List<T>` et `ObservableCollection<T>` en WPF ?

- A) `ObservableCollection<T>` est plus lente que `List<T>`  
- B) `List<T>` notifie l'UI automatiquement, `ObservableCollection<T>` ne le fait pas  
- C) `ObservableCollection<T>` notifie l'UI (DataGrid, ListBox...) automatiquement à chaque ajout/suppression  
- D) Il n'y a aucune différence en WPF  

> ✅ **Réponse : C**

---

**Q17.** Quelle commande NuGet installe le support SQLite en C# ?

- A) `dotnet add package SQLite`  
- B) `dotnet add package Microsoft.Data.Sqlite`  
- C) `npm install better-sqlite3`  
- D) `dotnet add package System.SQLite`  

> ✅ **Réponse : B**

---

## PARTIE 5 — MAUI et déploiement (Q18–Q20)

---

**Q18.** Quelle affirmation sur .NET MAUI est **fausse** ?

- A) MAUI est le successeur de Xamarin.Forms  
- B) MAUI utilise les contrôles natifs de chaque plateforme  
- C) MAUI nécessite d'écrire du code différent pour iOS et Android  
- D) En MAUI, `Entry` correspond à `<TextBox>` en WPF et `<input>` en HTML  

> ✅ **Réponse : C** *(MAUI utilise un seul code C# + XAML pour toutes les plateformes)*

---

**Q19.** Quelle commande publie une application WPF en un seul `.exe` autonome (sans nécessiter l'installation de .NET) ?

- A) `dotnet build -c Release`  
- B) `dotnet run --self-contained`  
- C) `dotnet publish -c Release --self-contained true -r win-x64 -p:PublishSingleFile=true`  
- D) `dotnet pack -o ./dist`  

> ✅ **Réponse : C**

---

**Q20.** Dans le code suivant, que fait le mot-clé `using` ?

```csharp
using var conn = new SqliteConnection(_connectionString);
conn.Open();
```

- A) Il importe un namespace  
- B) Il garantit que la connexion sera fermée automatiquement à la fin du bloc, même en cas d'erreur  
- C) Il crée un alias pour le type `SqliteConnection`  
- D) Il démarre une transaction SQL  

> ✅ **Réponse : B**

---

## Barème

| Score | Appréciation |
|-------|-------------|
| 18–20 | Excellent |
| 15–17 | Bien |
| 12–14 | Assez bien |
| 9–11  | Passable |
| < 9   | Insuffisant |
