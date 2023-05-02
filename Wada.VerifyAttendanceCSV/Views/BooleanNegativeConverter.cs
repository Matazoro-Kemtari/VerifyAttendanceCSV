﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Wada.VerifyAttendanceCSV.Views
{
    /// <summary>
    /// boolを反転するコンバーター
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanNegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }
    }
}
