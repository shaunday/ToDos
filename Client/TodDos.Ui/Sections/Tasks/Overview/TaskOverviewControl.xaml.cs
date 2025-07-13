using System.Windows.Controls;
using System.Windows;
using Todos.Ui.ViewModels;

namespace Todos.Ui.Sections.Tasks
{
    public partial class TaskOverviewControl : UserControl
    {
        public TaskOverviewControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(TaskOverviewControl), new PropertyMetadata(string.Empty));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty AllProperty =
            DependencyProperty.Register("All", typeof(TasksOverviewModel), typeof(TaskOverviewControl), new PropertyMetadata(null));
        public static readonly DependencyProperty FilteredProperty =
            DependencyProperty.Register("Filtered", typeof(TasksOverviewModel), typeof(TaskOverviewControl), new PropertyMetadata(null));

        public TasksOverviewModel All
        {
            get => (TasksOverviewModel)GetValue(AllProperty);
            set => SetValue(AllProperty, value);
        }
        public TasksOverviewModel Filtered
        {
            get => (TasksOverviewModel)GetValue(FilteredProperty);
            set => SetValue(FilteredProperty, value);
        }
    }
} 