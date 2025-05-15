using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using KnowFlow.Models;
using System.Windows.Controls;
using System.Diagnostics;

namespace KnowFlow.Converters
{
    public class IntToRadioVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToCheckVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QuestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SingleChoiceTemplate { get; set; }
        public DataTemplate MultipleChoiceTemplate { get; set; }
        public DataTemplate TextInputTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Question question)
            {
                return question.QuestionType switch
                {
                    1 => SingleChoiceTemplate,
                    2 => MultipleChoiceTemplate,
                    0 => TextInputTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
