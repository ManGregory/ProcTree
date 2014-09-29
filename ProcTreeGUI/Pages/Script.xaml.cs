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
            SetScriptText(Clipboard.GetText());
        }
    }
}
