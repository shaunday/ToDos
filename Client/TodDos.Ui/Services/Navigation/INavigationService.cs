using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Ui.ViewModels;

namespace Todos.Ui.Services.Navigation
{
    public interface INavigationService
    {
        ViewModelBase CurrentViewModel { get; set; }

        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    }
}
