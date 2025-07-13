using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Client.Common.Interfaces;
using Todos.Ui.Services.Navigation;

namespace Todos.Ui.ViewModels
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
        public INavigationService Navigation { get; }

        public ViewModelBase(INavigationService navigation)
        {
            Navigation = navigation;
        }

        public ViewModelBase(IMapper mapper, INavigationService navigation) : this(navigation)
        {
            _mapper = mapper;
        }

        public ViewModelBase(ITaskSyncClient taskSyncClient, IMapper mapper, INavigationService navigation) : this(navigation)
        {
            _mapper = mapper;
            _taskSyncClient = taskSyncClient;
        }

        public virtual void Init() { }
        public virtual void Cleanup() { }
    }

}
