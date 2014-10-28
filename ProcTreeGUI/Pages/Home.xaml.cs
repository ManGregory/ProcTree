using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
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
            GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtSource);
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
                if (args.Error != null)
                {
                    ModernDialog.ShowMessage(args.Error.Message + Environment.NewLine + args.Error.StackTrace, "Ошибка", MessageBoxButton.OK);
                    SwitchOverlay(false);
                    return;
                }
                if (args.Result != null)
                {
                    var dbObjectUsages = args.Result as List<DbObjectUsage>;
                    if (dbObjectUsages != null)
                    {
                        LstDbObjects.ItemsSource = dbObjectUsages;
                        var firstItem = LstDbObjects.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                        if (firstItem != null) firstItem.IsSelected = true;
                        SwitchOverlay(false);
                    }
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_worker.IsBusy)
                {
                    if (LstDbObjects.ItemsSource != null)
                    {
                        LstDbObjects.ItemsSource = null;
                        TxtSource.Clear();
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
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message + Environment.NewLine + ex.StackTrace, "Ошибка", MessageBoxButton.OK);
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
            var dbObjectUsages = dbObjects
                .Where(d => d.Type == DbObjectType.Procedure)
                .Select(dbObject => new DbObjectUsage
                {
                    DbObject = dbObject,
                    DbUsages = DbObjectRepository.GetDbObjectUsages(dbObject, dbObjects),
                    SourceFileUsages = sourceFinder.DbObjectUsageFiles.Where(d => d.DbObject == dbObject)
                })
                .OrderBy(d => d.DbObject.Name)
                .Select(d => new DbObjectUsage
                {
                    DbObject = d.DbObject,
                    DbUsages = d.DbUsages,
                    SourceFileUsages = d.SourceFileUsages,
                    IsUsed = d.DbUsages.Any() || d.SourceFileUsages.Any()
                }).ToList();
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
            try
            {
                if (LstDbObjects.ItemsSource != null)
                {
                    var unusedDbObjects = LstDbObjects.ItemsSource as List<DbObjectUsage>;
                    if (unusedDbObjects != null)
                    {
                        ValueContainer.ScriptValues.UnusedDbObjects =
                            unusedDbObjects.Where(d => d.IsUsed == false).Select(d => d.DbObject).ToList();
                        var scriptText = string.Join(Environment.NewLine,
                            new ScriptCreator().CreateDropProcedureScript(ValueContainer.ScriptValues.UnusedDbObjects));
                        Clipboard.SetText(scriptText);
                        ValueContainer.ScriptValues.Script = scriptText;
                        ValueContainer.DbConnectionValues.Repository = new DbObjectRepository(
                            TxtUserName.Text, TxtUserPassword.Password, TxtServerName.Text, TxtDbName.Text);
                        NavigationCommands.GoToPage.Execute("/Pages/Script.xaml", this);
                    }
                }
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message + Environment.NewLine + ex.StackTrace, "Ошибка", MessageBoxButton.OK);
            }
        }

        private void LstDbObjects_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                LoadSelectedItem();
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message + Environment.NewLine + ex.StackTrace, "Ошибка", MessageBoxButton.OK);
            }
        }

        private void LoadSelectedItem()
        {
            var dbObjectUsage = LstDbObjects.SelectedItem as DbObjectUsage;
            if (dbObjectUsage != null)
            {
                GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtSource);
                TxtSource.ScrollToHome();
                TxtSource.Text = dbObjectUsage.DbObject.Source;
                return;
            }
            var dbObject = LstDbObjects.SelectedItem as DbObjectUsageProcedure;
            if (dbObject != null)
            {
                GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtSource);
                TxtSource.Text = dbObject.DbObject.Source;
                SelectSourceLine(dbObject.LineNumbers.First().LineNumber);
                return;
            }
            var procUsageLine = LstDbObjects.SelectedItem as ProcedureUsageLine;
            if (procUsageLine != null)
            {
                GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtSource);
                TxtSource.Text = procUsageLine.DbObjectUsageProcedure.DbObject.Source;
                SelectSourceLine(procUsageLine.LineNumber);
                return;
            }
            var fileUsage = LstDbObjects.SelectedItem as FileUsage;
            if (fileUsage != null)
            {
                LoadFileSource(fileUsage);
                SelectSourceLine(fileUsage.LineNumbers.First().LineNumber);
                return;
            }
            var fileUsageLine = LstDbObjects.SelectedItem as FileUsageLine;
            if (fileUsageLine != null)
            {
                LoadFileSource(fileUsageLine.FileUsage);
                SelectSourceLine(fileUsageLine.LineNumber);
            }
        }

        private void SelectSourceLine(int lineNumber)
        {
            TxtSource.ScrollToLine(lineNumber);
            var line = TxtSource.Document.GetLineByNumber(lineNumber);
            TxtSource.Select(line.Offset, line.Length);
        }

        private void LoadFileSource(FileUsage fileUsage)
        {
            GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Pascal, TxtSource);
            TxtSource.Text =
                GuiUtils.GetString(Encoding.Convert(Encoding.GetEncoding(1251), Encoding.UTF8,
                    GuiUtils.GetBytes(System.IO.File.ReadAllText(fileUsage.PathToFile, Encoding.GetEncoding(1251)))));
        }
    }
}