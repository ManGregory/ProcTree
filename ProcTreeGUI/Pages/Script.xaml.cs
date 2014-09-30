using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Xml;
using FirebirdSql.Data.FirebirdClient;
using FirstFloor.ModernUI.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Button = System.Windows.Controls.Button;

namespace ProcTreeGUI.Pages
{
    /// <summary>
    /// Interaction logic for Script.xaml
    /// </summary>
    public partial class Script
    {
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
        }

        private void BtnExecuteScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TxtScript.Text))
                {
                    TxtErrors.Clear();
                    ValueContainer.DbConnectionValues.Repository.ExecuteScript(TxtScript.Text);
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
