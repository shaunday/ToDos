using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Todos.Client.Orchestrator.ViewModels;

namespace Todos.Client.Orchestrator
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainWindowViewModel();
            DataContext = vm;
            // Removed log viewer event hookups; now handled in MainWindowViewModel
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                var allClientsCount = viewModel.FilteredClientsView.Cast<object>().Count();
                if (allClientsCount > 0)
                {
                    var result = MessageBox.Show(
                        $"You have {allClientsCount} client(s) running. Do you want to close all clients and exit?",
                        "Confirm Exit",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            // Kill all clients and close
                            viewModel.KillAllClientsCommand.Execute(null);
                            break;
                        case MessageBoxResult.No:
                            // Close without killing clients
                            break;
                        case MessageBoxResult.Cancel:
                            // Cancel the closing
                            e.Cancel = true;
                            return;
                    }
                }
            }
        }
    }
}