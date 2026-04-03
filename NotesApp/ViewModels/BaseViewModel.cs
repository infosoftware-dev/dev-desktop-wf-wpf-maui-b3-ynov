
// ViewModels/BaseViewModel.cs
// Classe de base pour tous les ViewModels.
// Implemente INotifyPropertyChanged pour que le Data Binding fonctionne.
//
// ANALOGIE : c'est comme un "useState" en React.
// Quand on appelle SetProperty(), ca notifie l'UI de se re-rendre,
// exactement comme setCount() re-rend le composant React.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NotesApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Cet evenement est ecoute par le XAML.
        // Quand on le declenche, le XAML relit la propriete qui a change.
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifie l'UI qu'une propriete a change.
        /// [CallerMemberName] detecte automatiquement le nom de la propriete appelante.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Met a jour un champ ET notifie l'UI.
        /// C'est le "setter magique" de MVVM.
        /// 
        /// Utilisation :
        ///   private string _nom;
        ///   public string Nom {
        ///       get => _nom;
        ///       set => SetProperty(ref _nom, value);
        ///   }
        /// 
        /// C'est comme :
        ///   const [nom, setNom] = useState("");
        ///   setNom(newValue);  // re-rend le composant
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            // Ne rien faire si la valeur n'a pas change
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
