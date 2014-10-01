﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

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
            using (var reader = new XmlTextReader("avaloneditsql.xml"))
            {
                TxtScript.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
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
                    SwitchOverlay(true);
                    _worker.RunWorkerAsync(TxtScript.Text);
                }
            }
            catch (Exception ex)
            {
                TxtErrors.Text += ex.Message + Environment.NewLine;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetScriptText(ValueContainer.ScriptValues.Script);
        }
    }
}
