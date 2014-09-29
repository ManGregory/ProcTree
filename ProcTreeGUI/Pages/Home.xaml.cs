using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Clipboard = System.Windows.Clipboard;
using UserControl = System.Windows.Controls.UserControl;

namespace ProcTreeGUI.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public Home()
        {
            InitializeComponent();
            _worker.DoWork += (sender, args) =>
            {
                var arguments = args.Argument as object[];
                if (arguments != null)
                {
                    args.Result = GetUnusedDbObjects(arguments[0].ToString(), arguments[1].ToString(),
                        arguments[2].ToString(), arguments[3].ToString(), arguments[4] as IEnumerable<string>,
                        arguments[5] as IList<string>);
                }
            };
            _worker.RunWorkerCompleted += (sender, args) =>
            {
                var unusedDbObjects = args.Result as List<CheckedDbObject>;
                if (unusedDbObjects != null)
                {
                    LstDbObjects.ItemsSource = unusedDbObjects;
                    LstDbObjects.SelectedIndex = unusedDbObjects.Count > 0 ? 0 : -1;                   
                }
            };
        }        

        private void LstDbObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dbObject = LstDbObjects.SelectedItem as DbObject;
            if (dbObject != null)
                TxtDbObjectSource.Text = dbObject.Source;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_worker.IsBusy)
            {
                var argument = new object[]
                {
                    TxtUserName.Text, 
                    TxtUserPassword.Password, 
                    TxtServerName.Text,
                    TxtDbName.Text, 
                    TxtFolders.Text.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries),
                    new List<string>(new[] {".pas", ".dfm"})
                };
                _worker.RunWorkerAsync(argument);
            }
        }

        private List<CheckedDbObject> GetUnusedDbObjects(string userName, string userPass, string dataSource, string dbName, IEnumerable<string> folders, IList<string> extensions)
        {
            var dbRepo = new DbObjectRepository(
                userName, userPass, dataSource, dbName
            );
            var dbObjects =
                dbRepo.GetDbObjects()
                    .Select(
                        d =>
                            new DbObject
                            {
                                Name = d.Name.ToLowerInvariant(),
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
                folders,
                extensions,
                unusedDbObjects).ToList();
           var usedInSource = objectUsages.Select(u => u.DbObject).GroupBy(d => d.Name).Select(gr => gr.First());
            unusedDbObjects = unusedDbObjects.Except(usedInSource).ToList();
            return
                unusedDbObjects.Select(
                    d =>
                        new CheckedDbObject
                        {
                            IsChecked = true,
                            Name = d.Name,
                            LinkedDbOjbects = d.LinkedDbOjbects,
                            Type = d.Type,
                            Source = d.Source
                        }).ToList();
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

        private void BtnCreateScript_Click(object sender, RoutedEventArgs e)
        {
            if (LstDbObjects.ItemsSource != null)
            {
                IList<DbObject> unusedDbObjects =
                    (LstDbObjects.ItemsSource as List<CheckedDbObject>).Where(d => d.IsChecked)
                        .Select(
                            d =>
                                new DbObject
                                {
                                    LinkedDbOjbects = d.LinkedDbOjbects,
                                    Name = d.Name,
                                    Source = d.Source,
                                    Type = d.Type
                                }).ToList();
                Clipboard.SetText(string.Join(Environment.NewLine,
                    new ScriptCreator().CreateDropProcedureScript(unusedDbObjects)));
            }
        }
    }

    public class CheckedDbObject : DbObject
    {
        public bool IsChecked { get; set; }
    }
}
