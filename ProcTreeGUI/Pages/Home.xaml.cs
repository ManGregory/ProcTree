using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProcTree.Core;
using UserControl = System.Windows.Controls.UserControl;

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
            var dbObject = LstDbObjects.SelectedItem as DbObject;
            if (dbObject != null)
                TxtDbObjectSource.Text = dbObject.Source;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var unusedDbObjects = GetUnusedDbObjects();
            LstDbObjects.ItemsSource = unusedDbObjects;
            LstDbObjects.SelectedIndex = unusedDbObjects.Count > 0 ? 0 : -1;
        }

        private List<DbObject> GetUnusedDbObjects()
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
            DbObjectRepository.MakeLinks(dbObjects);
            var unusedDbObjects = DbObjectRepository.GetUnusedDbObjects(dbObjects).ToList();
            var objectUsages = SourceFinder.GetObjectUsages(
                TxtFolders.Text.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries),
                new List<string>(new[] {".pas", ".dfm"}),
                unusedDbObjects).ToList();
            var usedInSource = objectUsages.Select(u => u.DbObject).GroupBy(d => d.Name).Select(gr => gr.First());
            unusedDbObjects = unusedDbObjects.Except(usedInSource).ToList();
            return unusedDbObjects;
        }

        private void BtnSelectFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    TxtFolders.Text += string.Format("{0};", folderDialog.SelectedPath);
                }
            }
        }
    }
}
