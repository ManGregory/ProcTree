using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using ProcTree.Core.ConvertSql;
using ProcTreeGUI.Properties;

namespace ProcTreeGUI.Pages
{
    /// <summary>
    /// Логика взаимодействия для SqlToCode.xaml
    /// </summary>
    public partial class SqlToCode : INotifyPropertyChanged
    {
        private readonly ISqlTextConverter _sqlConverter;
        private SqlConversionDirection _conversionDirection;
        public SqlConversionDirection ConversionDirection
        {
            get { return _conversionDirection; }
            set
            {
                _conversionDirection = value;
                OnPropertyChanged();
            }
        }

        public SqlToCode()
        {
            InitializeComponent();
            GuiUtils.LoadAvalonSyntax(SyntaxHighlighting.Sql, TxtSource);
            _sqlConverter = new DelphiConverter(0, "");
            BtnConvert.DataContext = this;
            ConversionDirection = _sqlConverter.GetSqlConversionDirection(TxtSource.Text);
        }

        private void BtnConvert_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtSource.Text =
                    ConversionDirection == SqlConversionDirection.SqlToCode
                        ? _sqlConverter.ConvertToProgrammingLanguage(TxtSource.Text)
                        : _sqlConverter.ConvertFromProgrammingLanguage(TxtSource.Text);
                Clipboard.SetText(TxtSource.Text);
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message + Environment.NewLine + ex.StackTrace, "Ошибка", MessageBoxButton.OK);
            }
        }

        private void TxtSource_OnTextChanged(object sender, EventArgs e)
        {
            ConversionDirection = _sqlConverter.GetSqlConversionDirection(TxtSource.Text);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TxtSource.Text = Clipboard.GetText();
        }
    }
}
