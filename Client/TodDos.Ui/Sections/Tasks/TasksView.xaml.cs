using System.Windows.Controls;
using System.Windows.Input;
using Todos.Ui.ViewModels;

namespace Todos.Ui.Sections
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            this.PreviewKeyDown += TasksView_PreviewKeyDown;
        }

        private void TasksView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is TasksViewModel vm && vm.EditingTask != null && vm.EditingTask.IsEditing)
                {
                    if (vm.CancelEditCommand.CanExecute(vm.EditingTask))
                    {
                        vm.CancelEditCommand.Execute(vm.EditingTask);
                        e.Handled = true;
                    }
                }
            }
        }
    }
} 