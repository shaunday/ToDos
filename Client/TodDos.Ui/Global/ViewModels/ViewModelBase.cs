using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using ToDos.Ui.Services.Navigation;

namespace ToDos.Ui.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        protected readonly IMapper _mapper;
        public INavigationService Navigation { get; }

        public ViewModelBase(INavigationService navigation)
        {
            Navigation = navigation;
        }

        public ViewModelBase(IMapper mapper, INavigationService navigation) : this(navigation)
        {
            _mapper = mapper;
        }
    }

}
