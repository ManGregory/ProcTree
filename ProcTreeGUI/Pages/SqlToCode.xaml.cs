using System.Windows;
using System.Windows.Controls;
using ProcTree.Core.ConvertSql;

namespace ProcTreeGUI.Pages
{
    /// <summary>
    /// Логика взаимодействия для SqlToCode.xaml
    /// </summary>
    public partial class SqlToCode
    {
        private readonly ISqlTextConverter _sqlConverter;

        public SqlToCode()
        {
            InitializeComponent();
            GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtSource);
            _sqlConverter = new DelphiConverter(0, "");
        }

        private void BtnConvert_OnClick(object sender, RoutedEventArgs e)
        {
            TxtSource.Text = _sqlConverter.ConvertToProgrammingLanguage(TxtSource.Text);
        }
    }
}
