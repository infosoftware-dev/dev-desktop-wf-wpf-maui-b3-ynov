// MainWindow.xaml.cs — Code-behind MINIMAL
// En MVVM, le code-behind ne fait qu'UNE chose : 
// connecter le ViewModel au DataContext.
// Toute la logique est dans MainViewModel.cs.

using System.Windows;
using NotesApp.ViewModels;

namespace NotesApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DataContext = le lien entre la View et le ViewModel
            // C'est comme passer des props a un composant React :
            //   <MainWindow viewModel={new MainViewModel()} />
            // Toutes les {Binding ...} dans le XAML liront les proprietes
            // de ce MainViewModel.
            DataContext = new MainViewModel();
        }
    }
}
