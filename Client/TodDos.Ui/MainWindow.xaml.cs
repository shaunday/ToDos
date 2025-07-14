using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Todos.Ui.Models;
using Todos.Ui.Services;
using Todos.Ui.ViewModels;

namespace Todos.Ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UiStateModel _uiState;
        private int? _userId;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWindowState();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowState();
        }

        private void LoadWindowState()
        {
            if (DataContext is MainViewModel mainVm && mainVm.ApplicationViewModel?.CurrentUser != null)
            {
                _userId = mainVm.ApplicationViewModel.CurrentUser.Id;
            }
            else
            {
                _userId = null;
            }
            if (_userId == null)
                return;
            var stateFile = UiStatePersistenceService.GetUserStateFilePath(_userId.Value);
            bool stateExists = System.IO.File.Exists(stateFile);
            _uiState = UiStatePersistenceService.Load(_userId.Value);
            if (stateExists && _uiState != null)
            {
                if (_uiState.WindowState == WindowState.Maximized.ToString())
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    if (_uiState.WindowWidth > 0) Width = _uiState.WindowWidth;
                    if (_uiState.WindowHeight > 0) Height = _uiState.WindowHeight;
                    if (_uiState.WindowTop >= 0) Top = _uiState.WindowTop;
                    if (_uiState.WindowLeft >= 0) Left = _uiState.WindowLeft;
                }
            }
            TasksViewModel.UiState = _uiState;
        }

        private void SaveWindowState()
        {
            if (_userId == null)
                return;
            if (_uiState == null) _uiState = new UiStateModel();
            _uiState.WindowState = WindowState.ToString();
            if (WindowState == WindowState.Normal)
            {
                _uiState.WindowWidth = Width;
                _uiState.WindowHeight = Height;
                _uiState.WindowTop = Top;
                _uiState.WindowLeft = Left;
            }
            UiStatePersistenceService.Save(_userId.Value, _uiState);
        }
    }
}
