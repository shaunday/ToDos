using System.Windows.Controls;
using Todos.Ui.ViewModels;
using Unity;

namespace Todos.Ui.Panels
{
    /// <summary>
    /// Interaction logic for TopPanel.xaml
    /// </summary>
    public partial class TopPanel : UserControl
    {
        public TopPanel()
        {
            InitializeComponent();
            DataContext = App.Container.Resolve<TopPanelViewModel>();
        }
    }
} 