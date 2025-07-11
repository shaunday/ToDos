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
            DataContext = new MainWindowViewModel();
            
            // Handle window closing event
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel && viewModel.RunningCount > 0)
            {
                var result = MessageBox.Show(
                    $"You have {viewModel.RunningCount} client(s) running. Do you want to close all clients and exit?",
                    "Confirm Exit",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // Kill all clients and close
                        viewModel.KillAllClientsSilent();
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