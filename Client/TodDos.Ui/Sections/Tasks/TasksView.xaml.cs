using System.Windows.Controls;
using System.Windows.Input;
using Todos.Ui.ViewModels;
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Threading;

namespace Todos.Ui.Sections
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            this.PreviewKeyDown += TasksView_PreviewKeyDown;
            WeakReferenceMessenger.Default.Register<EndEditTransactionMessage>(this, (r, m) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (TasksDataGrid.IsLoaded && TasksDataGrid.IsVisible)
                    {
                        try
                        {
                            TasksDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                            TasksDataGrid.CancelEdit(DataGridEditingUnit.Row);
                        }
                        catch { /* Optionally log or ignore */ }
                    }
                }), DispatcherPriority.Background);
            });
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