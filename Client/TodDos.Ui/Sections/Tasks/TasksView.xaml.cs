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

namespace Todos.Ui.Sections
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            this.PreviewKeyDown += TasksView_PreviewKeyDown;

            // Directly ensure any pending edits are committed when needed (if you want to do this on load or expose a method)
            // If you want to expose this as a public method, you can add:
            // public void CommitPendingEdits() { ... }
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