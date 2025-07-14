using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;

namespace TodDos.Ui.Global.ViewModels
{
    public class SortableCollectionViewModel<T> : ObservableObject
    {
        public ObservableCollection<T> Items { get; private set; }
        public string SortProperty { get; private set; } = string.Empty;
        public ListSortDirection SortDirection { get; private set; } = ListSortDirection.Ascending;

        public IRelayCommand<string> SortCommand { get; }
        public IRelayCommand SortByPriorityCommand { get; }
        public IRelayCommand SortByCompletedCommand { get; }
        public IRelayCommand SortByDueDateCommand { get; }

        public SortableCollectionViewModel()
        {
            Items = new ObservableCollection<T>();
            SortCommand = new RelayCommand<string>(Sort);
            SortByPriorityCommand = new RelayCommand(() => SortBy("Priority"));
            SortByCompletedCommand = new RelayCommand(() => SortBy("IsCompleted"));
            SortByDueDateCommand = new RelayCommand(() => SortBy("DueDate"));
        }

        public SortableCollectionViewModel(IEnumerable<T> items) : this()
        {
            Items = new ObservableCollection<T>(items);
        }

        private void Sort(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            if (SortProperty == propertyName)
            {
                // Toggle direction if same property
                SortDirection = SortDirection == ListSortDirection.Ascending 
                    ? ListSortDirection.Descending 
                    : ListSortDirection.Ascending;
            }
            else
            {
                // New property, default to ascending
                SortProperty = propertyName;
                SortDirection = ListSortDirection.Ascending;
            }

            var sortedItems = SortDirection == ListSortDirection.Ascending
                ? Items.OrderBy(item => GetPropertyValue(item, propertyName)).ToList()
                : Items.OrderByDescending(item => GetPropertyValue(item, propertyName)).ToList();

            // Instead of replacing the collection, update it in place:
            Items.Clear();
            foreach (var item in sortedItems)
                Items.Add(item);
        }

        public void SortBy(string propertyName)
        {
            SortCommand.Execute(propertyName);
        }

        private object GetPropertyValue(T item, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            var value = property?.GetValue(item);

            if (value == null)
            {
                var type = property?.PropertyType;
                if (type == typeof(DateTime?) || type == typeof(DateTime))
                    return DateTime.MinValue; // nulls sort last
                if (type == typeof(int?) || type == typeof(int))
                    return int.MinValue;
                if (type == typeof(bool?) || type == typeof(bool))
                    return false; // or true, depending on desired order
                if (type == typeof(string))
                    return string.Empty;
                // Add more types as needed
                return null;
            }
            return value;
        }

        public void RefreshSort()
        {
            if (!string.IsNullOrEmpty(SortProperty))
            {
                Sort(SortProperty);
            }
        }
    }
} 