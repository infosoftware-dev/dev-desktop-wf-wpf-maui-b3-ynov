# Guide d'installation — Developpement Desktop .NET (C#)

> **YNOV B3 Informatique** — 2025/2026  
> **A faire AVANT le premier jour de cours**  
> Temps estime : 15-20 minutes

---

## Quelle machine utiliser ?

WPF et WinForms sont des frameworks **Windows natifs**. Ils ne fonctionnent que sur Windows.

| Situation | Quoi faire |
|-----------|-----------|
| PC Windows | Parfait, installez tout directement sur Windows |
| PC Windows + WSL | Travaillez sur **Windows** (PowerShell), pas dans WSL. Les fenetres WPF/WinForms ne s'affichent pas correctement dans WSL |
| Mac | Theorie + MAUI OK, mais WPF/WinForms non. Prevoyez une VM Windows ou travaillez en binome avec quelqu'un sur Windows |
| Linux natif | Meme chose que Mac |

---

## Installation sur Windows

### Etape 1 : Installer le .NET 8 SDK

Le SDK contient tout ce qu'il faut pour compiler et executer du C#.

**Option A — avec winget (recommande, une seule commande) :**

Ouvrez un **PowerShell** (pas WSL, pas CMD) et tapez :

```powershell
winget install Microsoft.DotNet.SDK.8
```

**Option B — telechargement direct :**

1. Allez sur **https://dotnet.microsoft.com/download/dotnet/8.0**
2. Dans la section SDK, cliquez sur **Windows x64**
3. Lancez le fichier `.exe` telecharge
4. Suivez l'installateur (suivant, suivant, terminer)

**Apres l'installation, fermez et rouvrez votre terminal PowerShell**, puis verifiez :

```powershell
dotnet --version
```

Vous devez voir :

```
8.0.xxx
```

Si ca affiche une erreur "not found", fermez tous vos terminaux, rouvrez-en un et reessayez. Si ca ne marche toujours pas, redemarrez votre PC.

---

### Etape 2 : Installer VS Code + extensions

**Installer VS Code :**

```powershell
winget install Microsoft.VisualStudioCode
```

<!-- to install winget : 

$progressPreference = 'silentlyContinue'
Write-Host "Installing WinGet PowerShell module from PSGallery..."
Install-PackageProvider -Name NuGet -Force | Out-Null
Install-Module -Name Microsoft.WinGet.Client -Force -Repository PSGallery | Out-Null
Write-Host "Using Repair-WinGetPackageManager cmdlet to bootstrap WinGet..."
Repair-WinGetPackageManager -AllUsers
Write-Host "Done." -->

Ou telechargez-le sur **https://code.visualstudio.com**

**Installer les extensions indispensables :**

Ouvrez VS Code, puis appuyez sur `Ctrl+Shift+X` pour ouvrir le panneau des extensions. Cherchez et installez :

| Extension | Auteur | Role |
|-----------|--------|------|
| **C# Dev Kit** | Microsoft | Autocompletion, debugging, navigation dans le code C# |
| **C#** | Microsoft | Moteur du langage C# (s'installe automatiquement avec C# Dev Kit) |
| **.NET Install Tool** | Microsoft | Gestion des versions .NET |

**Extensions optionnelles mais utiles :**

| Extension | Auteur | Role |
|-----------|--------|------|
| **XAML** | Red Hat | Coloration syntaxique et completion basique dans les fichiers .xaml |
| **SQLite Viewer** | Florian Klampfer | Ouvrir et inspecter les fichiers .db directement dans VS Code |
| **Material Icon Theme** | Philipp Kief | Icones de fichiers plus lisibles (confort visuel) |

---

### Etape 3 : Tester un projet console

Ouvrez un **PowerShell** et tapez :

```powershell
mkdir ~/dev-desktop
cd ~/dev-desktop

dotnet new console -n TestConsole
cd TestConsole
dotnet run
```

Resultat attendu :

```
Hello, World!
```

---

### Etape 4 : Tester un projet WinForms

```powershell
cd ~/dev-desktop

dotnet new winforms -n TestWinForms
cd TestWinForms
dotnet run
```

**Une fenetre vide doit apparaitre.** Fermez-la avec la croix.

---

### Etape 5 : Tester un projet WPF

```powershell
cd ~/dev-desktop

dotnet new wpf -n TestWPF
cd TestWPF
dotnet run
```

**Meme chose : une fenetre vide doit apparaitre.**

Pour ouvrir ce projet dans VS Code :

```powershell
code .
```

VS Code s'ouvre avec le projet. Vous pouvez naviguer dans les fichiers `.cs` et `.xaml`.

---

### Etape 6 : Configurer VS Code pour le debugging

Pour pouvoir lancer et debugger vos apps directement depuis VS Code :

1. Ouvrez un projet WPF ou WinForms dans VS Code (`code .`)
2. Appuyez sur `Ctrl+Shift+D` (panneau Debug)
3. Cliquez sur **"create a launch.json file"**
4. Choisissez **.NET 5+ and .NET Core**
5. Un fichier `.vscode/launch.json` est cree automatiquement

Maintenant vous pouvez lancer l'app avec `F5` (debug) ou `Ctrl+F5` (sans debug) directement depuis VS Code.

**Alternative simple :** ouvrez le terminal integre dans VS Code (`` Ctrl+` ``) et tapez `dotnet run`.

---

### Etape 7 (optionnel) : Installer SQLite en ligne de commande

Pour pouvoir inspecter vos bases de donnees depuis le terminal :

```powershell
winget install SQLite.SQLite
```

Verifiez :

```powershell
sqlite3 --version
```

> Si vous avez installe l'extension **SQLite Viewer** dans VS Code, vous pouvez aussi ouvrir les fichiers `.db` directement dans l'editeur en double-cliquant dessus.

---

## Installation sur macOS

> **Rappel** : WPF et WinForms ne fonctionnent pas sur Mac. Vous pourrez suivre la theorie, coder en C# console, et travailler sur MAUI. Pour les TPs WPF, prevoyez une VM Windows ou un binome sur Windows.

### Etape 1 : Installer le .NET 8 SDK

```bash
# Avec Homebrew (recommande)
brew install dotnet-sdk

# Ou telechargez le .pkg sur :
# https://dotnet.microsoft.com/download/dotnet/8.0
# Choisissez : macOS Arm64 (Apple Silicon) ou x64 (Intel)
```

Verifiez :

```bash
dotnet --version
# 8.0.xxx
```

### Etape 2 : Installer VS Code + extensions

```bash
brew install --cask visual-studio-code
```

Ouvrez VS Code et installez les memes extensions que sur Windows :
`C# Dev Kit`, `.NET Install Tool`, `XAML` (optionnel).

### Etape 3 : Tester

```bash
dotnet new console -n TestConsole
cd TestConsole
dotnet run
# Hello, World!
```

### Pour les TPs WPF : utiliser une VM Windows

| Option | Prix | Recommandation |
|--------|------|---------------|
| **UTM** | Gratuit | Fonctionne bien sur Apple Silicon, telecharger sur https://mac.getutm.app |
| **Parallels Desktop** | Payant (version etudiante dispo) | Le plus simple et performant |
| **VirtualBox** | Gratuit | Fonctionne sur Intel Mac uniquement |

Dans la VM, installez Windows puis suivez les etapes Windows ci-dessus.

---

## Installation sur Linux (Ubuntu / Debian)

> **Meme limitation que Mac** : WPF et WinForms ne tournent pas. Theorie + C# console + MAUI OK.

### Etape 1 : Installer le .NET 8 SDK

```bash
# Ubuntu 24.04 / 22.04
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```

Si le package n'est pas disponible dans les depots :

```bash
# Script d'installation officiel Microsoft
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Ajouter au PATH (mettre dans ~/.bashrc ou ~/.zshrc)
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc
```

> **Evitez snap pour .NET** (`sudo snap install dotnet-sdk`). Ca fonctionne mais ca cause souvent des problemes de permissions et de PATH.

Verifiez :

```bash
dotnet --version
# 8.0.xxx
```

### Etape 2 : Installer VS Code + extensions

```bash
sudo snap install code --classic
```

Memes extensions que sur Windows : `C# Dev Kit`, `.NET Install Tool`.

### Etape 3 : Installer SQLite

```bash
sudo apt install -y sqlite3
sqlite3 --version
```

---

## Checklist finale

Cochez chaque point avant de venir en cours :

```
[ ] dotnet --version affiche 8.0.xxx ou superieur
[ ] VS Code est installe avec l'extension C# Dev Kit
[ ] dotnet new console -n Test && cd Test && dotnet run affiche "Hello, World!"
[ ] (Windows uniquement) dotnet new wpf -n TestWPF && cd TestWPF && dotnet run ouvre une fenetre
[ ] (Windows uniquement) dotnet new winforms -n TestWF && cd TestWF && dotnet run ouvre une fenetre
```

---

## En cas de probleme

| Probleme | Solution |
|----------|----------|
| `dotnet` not found apres installation | Fermer et rouvrir le terminal. Si ca persiste, redemarrer le PC |
| `dotnet` not found dans WSL | Normal si vous avez installe .NET sur Windows. Ouvrez un **PowerShell** Windows |
| `dotnet new wpf` dit "template not found" | Verifiez que vous avez le SDK 8.0+ : `dotnet --list-sdks` |
| VS Code ne reconnait pas le C# | Verifiez que l'extension **C# Dev Kit** est installee et activee |
| Erreur "SDK not found" au build | Executez `dotnet --list-sdks`. Si vide, reinstallez le SDK |
| La fenetre WPF ne s'ouvre pas dans WSL | Normal. Ouvrez PowerShell **Windows** et lancez depuis la |
| `winget` n'est pas reconnu | Installez App Installer depuis le Microsoft Store, ou telechargez manuellement |