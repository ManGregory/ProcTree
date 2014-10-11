using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ProcTree.Core;
using Clipboard = System.Windows.Clipboard;

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
            GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtDbObjectSource);
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
                if (args.Result != null)
                {
                    var dbObjectUsages = args.Result as List<DbObjectUsage>;
                    if (dbObjectUsages != null)
                    {
                        /*foreach (var dbObjectUsage in dbObjectUsages)
                        {
                            LstDbObjects.Items.Add(
                                CreateUsageTreeItem(dbObjectUsage)
                            );
                        }*/
                        LstDbObjects.ItemsSource = dbObjectUsages;
                        SwitchOverlay(false);
                    }
                }
            };
        }

        private TreeViewItem CreateUsageTreeItem(DbObjectUsage dbObjectUsage)
        {
            var item = new TreeViewItem();
            item.Header = dbObjectUsage.DbObject.Name;
            var procItem = CreateProcedureTreeItem(dbObjectUsage.DbUsages);
            var sourceItem = CreateSourceTreeItem(dbObjectUsage.DbUsages);
            item.Items.Add(procItem);
            item.Items.Add(sourceItem);
            return item;
        }

        private TreeViewItem CreateSourceTreeItem(IEnumerable<DbObject> dbUsages)
        {
            var item = new TreeViewItem();
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock {Text = "Source files"});
            item.Header = stackPanel;
            return item;
        }

        private TreeViewItem CreateProcedureTreeItem(IEnumerable<DbObject> dbUsages)
        {
            var item = new TreeViewItem();
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = "Procedures" });
            item.Header = stackPanel;
            return item;
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
                if (LstDbObjects.ItemsSource != null)
                {
                    LstDbObjects.ItemsSource = null;
                    TxtDbObjectSource.Clear();
                }
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
                SwitchOverlay(true);
            }
        }

        private void SwitchOverlay(bool isVisible)
        {
            Overlay.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private List<DbObjectUsage> GetUnusedDbObjects(string userName, string userPass, string dataSource, string dbName, IEnumerable<string> folders, IList<string> extensions)
        {
            var dbRepo = new DbObjectRepositoryDependencies(
                userName, userPass, dataSource, dbName
            );
            var dbObjects =
                (dbRepo as IDbObjectRepository).GetDbObjects().ToList();
            var sourceFinder = new SourceFinder();
            sourceFinder.FindObjectUsages(folders, extensions, dbObjects);
            var dbObjectUsages = dbObjects.Select(dbObject => new DbObjectUsage
            {
                DbObject = dbObject,
                DbUsages = DbObjectRepository.GetDbObjectUsages(dbObject, dbObjects),
                SourceFileUsages = sourceFinder.DbObjectUsageFiles.Where(d => d.DbObject == dbObject)
            }).OrderBy(d => d.DbObject.Name).ToList();
            //var unusedDbObjects = DbObjectRepository.GetUnusedDbObjects(dbObjects).ToList();
            /*var usedInSource = objectUsages.Select(u => u.DbObject).GroupBy(d => d.Name).Select(gr => gr.First());
            dbObjects = unusedDbObjects.Except(usedInSource).ToList();
            return
                dbObjects
                    .OrderBy(d => d.Name)
                    .Select(
                    d =>
                        new CheckedDbObject
                        {
                            IsChecked = true,
                            Name = d.Name,
                            LinkedDbOjbects = d.LinkedDbOjbects,
                            Type = d.Type,
                            Source = d.Source
                        }).ToList();*/
            return dbObjectUsages;
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
                ValueContainer.ScriptValues.UnusedDbObjects =
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
                var scriptText = string.Join(Environment.NewLine,
                    new ScriptCreator().CreateDropProcedureScript(ValueContainer.ScriptValues.UnusedDbObjects));
                Clipboard.SetText(scriptText);
                ValueContainer.ScriptValues.Script = scriptText;
                ValueContainer.DbConnectionValues.Repository = new DbObjectRepository(
                    TxtUserName.Text, TxtUserPassword.Password, TxtServerName.Text, TxtDbName.Text);
                NavigationCommands.GoToPage.Execute("/Pages/Script.xaml", this);
            }
        }

        private void LstDbObjects_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var dbObjectUsage = LstDbObjects.SelectedItem as DbObjectUsage;
            if (dbObjectUsage != null)
            {
                TxtDbObjectSource.Text = dbObjectUsage.DbObject.Source;
                return;
            }
            var dbObject = LstDbObjects.SelectedItem as DbObject;
            if (dbObject != null)
            {
                TxtDbObjectSource.Text = dbObject.Source;
                return;
            }
        }
    }

    public class CheckedDbObject : DbObject
    {
        public bool IsChecked { get; set; }
    }
}
