using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProcTree.Core;

namespace ProcTreeGUI.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
        }

        private void LstDbObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtDbObjectSource.Text = (LstDbObjects.SelectedItem as DbObject).Source;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dbRepo = new DbObjectRepository(
                TxtUserName.Text, TxtUserPassword.Password, TxtServerName.Text, TxtDbName.Text
            );
            var dbObjects =
                dbRepo.GetDbObjects()
                    .Select(
                        d =>
                            new DbObject
                            {
                                Name = d.Name.ToUpperInvariant(),
                                Source = d.Source,
                                Type = d.Type,
                                LinkedDbOjbects = d.LinkedDbOjbects
                            })
                    .Where(d => d.Type == DbObjectType.Procedure)
                    .OrderBy(d => d.Name)
                    .ToList();
            LstDbObjects.ItemsSource = dbObjects;
            LstDbObjects.SelectedIndex = dbObjects.Count > 0 ? 0 : -1;
        }
    }
}
