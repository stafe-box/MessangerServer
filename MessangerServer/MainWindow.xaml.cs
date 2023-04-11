using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessangerServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Messanger _messanger;
        public MainWindow()
        {
            InitializeComponent();
            Info.Users.Add("@all", "");
           
        }

        private void StartBnt_Click(object sender, RoutedEventArgs e)
        {
            _messanger = new Messanger();
            _messanger.Start();
            _messanger.UserState += GetUserState;
            _messanger.Error += GetError;

        }

        private void GetError(string e)
        {
            MessageBoxImage mbi = MessageBoxImage.Error;
            MessageBoxButton mbb = MessageBoxButton.OK;
            MessageBox.Show(e, "Error", mbb, mbi);
        }

        private void GetUserState(string text, Color color)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                TextRange tr = new TextRange(Logs.Document.ContentEnd, Logs.Document.ContentEnd);
                
                tr.Text = text;
                try
                {
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                        new SolidColorBrush(color));
                }
                catch (FormatException) { }
            }));
        }
    }
}
