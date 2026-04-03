# Exercice — Ajouter une Categorie sur les notes

## Objectif

Ajoute un champ **Categorie** sur chaque note, exactement comme le champ **Titre**.

---

## A faire

- Dans `Note.cs` : ajoute une propriete `Categorie`
- Dans `MainViewModel.cs` : ajoute la propriete liee `Categorie` (meme schema que `Titre`)
- Dans `MainWindow.xaml` : ajoute un `TextBox` lie a `Categorie` dans le formulaire
- Dans `NoteRepository.cs` : sauvegarde et lis la categorie en base

---

## Criteres de validation

- [ ] Le champ Categorie s'affiche dans le formulaire
- [ ] Selectionner une note charge sa categorie
- [ ] Ajouter / Modifier sauvegarde la categorie en base
