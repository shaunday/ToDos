using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Todos.Ui.Sections.Tasks
{
    public partial class TaskFilterControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty PriorityProperty =
            DependencyProperty.Register("Priority", typeof(string), typeof(TaskFilterControl), new FrameworkPropertyMetadata("All", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterChanged));

        public static readonly DependencyProperty TagFilterProperty =
            DependencyProperty.Register("TagFilter", typeof(string), typeof(TaskFilterControl), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterChanged));

        public static readonly DependencyProperty CompletedStatusProperty =
            DependencyProperty.Register("CompletedStatus", typeof(string), typeof(TaskFilterControl), new FrameworkPropertyMetadata("All", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterChanged));

        public string Priority
        {
            get => (string)GetValue(PriorityProperty);
            set { SetValue(PriorityProperty, value); OnPropertyChanged(nameof(Priority)); }
        }

        public string TagFilter
        {
            get => (string)GetValue(TagFilterProperty);
            set { SetValue(TagFilterProperty, value); OnPropertyChanged(nameof(TagFilter)); }
        }

        public string CompletedStatus
        {
            get => (string)GetValue(CompletedStatusProperty);
            set { SetValue(CompletedStatusProperty, value); OnPropertyChanged(nameof(CompletedStatus)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskFilterControl ctrl)
                ctrl.OnPropertyChanged(e.Property.Name);
        }
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public TaskFilterControl()
        {
            InitializeComponent();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Priority = "All";
            TagFilter = string.Empty;
            CompletedStatus = "All";
        }

        private void ClearAllFilters_Click(object sender, RoutedEventArgs e)
        {
            Priority = "All";
            TagFilter = string.Empty;
            CompletedStatus = "All";
        }
    }
} 