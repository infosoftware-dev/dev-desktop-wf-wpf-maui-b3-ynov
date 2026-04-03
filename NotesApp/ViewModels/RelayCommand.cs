// ViewModels/RelayCommand.cs
// Permet de lier un bouton XAML a une methode du ViewModel.
//
// AVANT (code-behind) :
//   <Button Click="BtnSave_Click"/>
//   private void BtnSave_Click(...) { ... }
//
// APRES (MVVM) :
//   <Button Command="{Binding SaveCommand}"/>
//   public ICommand SaveCommand { get; }
//   SaveCommand = new RelayCommand(() => Save(), () => CanSave());
//
// Avantage : le bouton se GRISE automatiquement si CanSave() retourne false !

using System.Windows.Input;

namespace NotesApp.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;           // La methode a executer
        private readonly Func<bool>? _canExecute;   // Peut-on executer ? (optionnel)

        /// <summary>
        /// Cree une commande.
        /// </summary>
        /// <param name="execute">La methode a appeler quand le bouton est clique</param>
        /// <param name="canExecute">Retourne true si le bouton doit etre actif (optionnel)</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Le bouton est-il actif ?
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        // Executer la commande
        public void Execute(object? parameter) => _execute();

        // WPF reevalue automatiquement CanExecute quand l'utilisateur interagit
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}