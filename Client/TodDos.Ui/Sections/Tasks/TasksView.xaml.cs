using System.Windows.Controls;
using System.Windows.Input;
using Todos.Ui.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Data;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Todos.Ui.Services;
using Todos.Ui.Sections.Tasks;

namespace Todos.Ui.Sections
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            this.PreviewKeyDown += TasksView_PreviewKeyDown;
            WeakReferenceMessenger.Default.Register<CommitAndClearFocusMessage>(this, (r, m) => CommitAndClearFocus());
            this.Unloaded += (s, e) => WeakReferenceMessenger.Default.Unregister<CommitAndClearFocusMessage>(this);
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

        private void CommitAndClearFocus()
        {
            TasksDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            TasksDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

            Keyboard.ClearFocus();
            TasksDataGrid.Focus();
        }
    }
} 