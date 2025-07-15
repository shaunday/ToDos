using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using Todos.Ui.Services.Navigation;
using Serilog;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace TodDos.Ui.Global.ViewModels
{
    public interface IInitializable
    {
        void Init();
    }
    public interface ICleanable
    {
        void Cleanup();
    }

    public class ViewModelBase : ObservableObject, IInitializable, ICleanable
    {
        protected readonly IMapper _mapper;
        protected readonly ITaskSyncClient _taskSyncClient;
        protected readonly ILogger _logger;
        public INavigationService Navigation { get; }

        public ViewModelBase(INavigationService navigation, ILogger logger = null)
        {
            Navigation = navigation;
            _logger = logger;
        }

        public virtual void Init() { }
        public virtual void Cleanup() { }

        /// <summary>
        /// Runs an async action with try-catch, logging, and optional Snackbar error notification.
        /// </summary>
        protected async Task RunWithErrorHandlingAsync(
            Func<Task> action,
            string errorMessage,
            SnackbarMessageQueue snackbar = null,
            Func<Task> recovery = null)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _ = Application.Current.Dispatcher.Invoke(async () =>
                {
                    _logger?.Error(ex, errorMessage);
                    snackbar?.Enqueue(errorMessage);
                    if (recovery != null)
                    {
                        await recovery();
                    }
                });
            }
        }

        /// <summary>
        /// Runs a sync action with try-catch, logging, and optional Snackbar error notification.
        /// This wraps the async version for consistency.
        /// </summary>
        protected void RunWithErrorHandling(Action action, string errorMessage, SnackbarMessageQueue snackbar = null)
        {
            RunWithErrorHandlingAsync(() => { action(); return Task.CompletedTask; }, errorMessage, snackbar).GetAwaiter().GetResult();
        }
    }

}
