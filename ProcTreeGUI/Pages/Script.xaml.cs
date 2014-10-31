using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using ProcTree.Core;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace ProcTreeGUI.Pages
{
    /// <summary>
    /// Interaction logic for Script.xaml
    /// </summary>
    public partial class Script
    {
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public void SetScriptText(string text)
        {
            TxtScript.Text = text;
        }

        public Script()
        {
            InitializeComponent();
            GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtScript);
            _worker.DoWork += (sender, args) =>
            {                
                try
                {
                    ValueContainer.DbConnectionValues.Repository.ExecuteScript(args.Argument.ToString());
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke((Action) delegate {
                        TxtErrors.Text = ex.Message + Environment.NewLine;
                    });
                }                
            };
            _worker.RunWorkerCompleted += (sender, args) => SwitchOverlay(false);
        }

        private void SwitchOverlay(bool isVisible)
        {
            Overlay.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnExecuteScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TxtScript.Text) && !_worker.IsBusy)
                {
                    TxtErrors.Clear();                    
                    if (ChkSaveBackup.IsChecked == true)
                    {
                        ShowSaveBackupDialog();
                    }
                    SwitchOverlay(true);
                    _worker.RunWorkerAsync(TxtScript.Text);
                }
            }
            catch (Exception ex)
            {
                TxtErrors.Text += ex.Message + Environment.NewLine;
            }
        }

        private static void ShowSaveBackupDialog()
        {
            using (var sfd = new SaveFileDialog())
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var scriptToSave = string.Join(Environment.NewLine,
                        new ScriptCreator().CreateAlterProcedureScript(ValueContainer.ScriptValues.UnusedDbObjects));
                    File.WriteAllText(sfd.FileName, scriptToSave);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetScriptText(ValueContainer.ScriptValues.Script);
        }
    }
}
