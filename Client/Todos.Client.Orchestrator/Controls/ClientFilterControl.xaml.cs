using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Todos.Client.Common;

namespace Todos.Client.Orchestrator.Controls
{
    public partial class ClientFilterControl : UserControl
    {
        public static readonly DependencyProperty ClientTypeProperty = DependencyProperty.Register(
            nameof(ClientType), typeof(TypesGlobal.ClientType), typeof(ClientFilterControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterChanged));

        public static readonly DependencyProperty IsAliveProperty = DependencyProperty.Register(
            nameof(IsAlive), typeof(bool), typeof(ClientFilterControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterChanged));

        public static readonly DependencyProperty FilterProcessIdTextProperty = DependencyProperty.Register(
            nameof(FilterProcessIdText), typeof(string), typeof(ClientFilterControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterChanged));

        public static readonly DependencyProperty ClearFiltersCommandProperty = DependencyProperty.Register(
            nameof(ClearFiltersCommand), typeof(ICommand), typeof(ClientFilterControl), new PropertyMetadata(null));

        public TypesGlobal.ClientType ClientType
        {
            get => (TypesGlobal.ClientType)GetValue(ClientTypeProperty);
            set => SetValue(ClientTypeProperty, value);
        }

        public bool IsAlive
        {
            get => (bool)GetValue(IsAliveProperty);
            set => SetValue(IsAliveProperty, value);
        }

        public string FilterProcessIdText
        {
            get => (string)GetValue(FilterProcessIdTextProperty);
            set => SetValue(FilterProcessIdTextProperty, value);
        }

        public ICommand ClearFiltersCommand
        {
            get => (ICommand)GetValue(ClearFiltersCommandProperty);
            set => SetValue(ClearFiltersCommandProperty, value);
        }

        public event EventHandler FilterChanged;

        public ClientFilterControl()
        {
            InitializeComponent();
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ClientFilterControl control)
            {
                control.FilterChanged?.Invoke(control, EventArgs.Empty);
            }
        }
    }
} 