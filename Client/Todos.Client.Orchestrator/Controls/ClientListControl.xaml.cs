using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows;

namespace Todos.Client.Orchestrator.Controls
{
    public partial class ClientListControl : UserControl
    {
        public ClientListControl()
        {
            InitializeComponent();
            this.Loaded += ClientListControl_Loaded;
        }

        private void ClientListControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to FilteredClients changes if DataContext is set
            var vm = DataContext;
            var prop = vm?.GetType().GetProperty("FilteredClients");
            if (prop != null)
            {
                if (prop.GetValue(vm) is INotifyCollectionChanged filteredClients)
                {
                    filteredClients.CollectionChanged += FilteredClients_CollectionChanged;
                }
            }
        }

        private void FilteredClients_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Scroll to the last item if any
            if (ClientListView.Items.Count > 0)
            {
                ClientListView.ScrollIntoView(ClientListView.Items[ClientListView.Items.Count - 1]);
            }
        }
    }
} 