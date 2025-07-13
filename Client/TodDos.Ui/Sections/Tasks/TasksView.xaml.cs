using System.Windows.Controls;

namespace Todos.Ui.Sections
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
        }

        private void PriorityHeader_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.TasksViewModel viewModel)
            {
                viewModel.SortByPriorityCommand.Execute(null);
            }
        }

        private void CompletedHeader_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.TasksViewModel viewModel)
            {
                viewModel.SortByCompletedCommand.Execute(null);
            }
        }

        private void DueDateHeader_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.TasksViewModel viewModel)
            {
                viewModel.SortByDueDateCommand.Execute(null);
            }
        }
    }
} 