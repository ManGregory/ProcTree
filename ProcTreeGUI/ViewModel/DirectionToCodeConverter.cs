using System;
using System.Globalization;
using System.Windows.Data;
using ProcTree.Core.ConvertSql;

namespace ProcTreeGUI.ViewModel
{   
    [ValueConversion(typeof(SqlConversionDirection), typeof(string))]
    public class DirectionToCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var direction = (SqlConversionDirection) value;
            return direction == SqlConversionDirection.SqlToCode ? "SQL ⇆ Код" : "Код ⇆ SQL";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
