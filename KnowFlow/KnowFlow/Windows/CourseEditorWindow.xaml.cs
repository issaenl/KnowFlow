using KnowFlow.Models;
using KnowFlow.Pages.Сlass;
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
using System.Windows.Shapes;

namespace KnowFlow.Windows
{
    public class ColorWrapper
    {
        public Color Color { get; set; }
        public string Name { get; set; }

        public ColorWrapper(Color color, string name)
        {
            Color = color;
            Name = name;
        }
    }

    public partial class CourseEditorWindow : Window
    {
        public Course Course { get; private set; }

        public List<ColorWrapper> AvailableColors { get; } = new List<ColorWrapper>
        {
            new ColorWrapper(Colors.LightBlue, "Голубой"),
            new ColorWrapper(Colors.LightGreen, "Зелёный"),
            new ColorWrapper(Colors.LightSalmon, "Лососевый"),
            new ColorWrapper(Colors.LightGoldenrodYellow, "Золотистый"),
            new ColorWrapper(Colors.LightCoral, "Коралловый"),
            new ColorWrapper(Colors.LightSkyBlue, "Небесно-голубой"),
            new ColorWrapper(Colors.LightSteelBlue, "Стальной голубой"),
            new ColorWrapper(Colors.LightPink, "Розовый"),
            new ColorWrapper(Colors.LightSeaGreen, "Морской волны"),
            new ColorWrapper(Colors.LightGray, "Серый")
        };

        private readonly int _curatorId;
        private readonly UserData _userData = new UserData();

        public CourseEditorWindow(string curatorName)
        {
            InitializeComponent();

            _curatorId = _userData.GetUserIdByUsername(curatorName);
            Course = new Course
            {
                CourseName = string.Empty,
                CourseDescription = string.Empty,
                Color = GetRandomColor().ToString(),
                CuratorId = _curatorId
            };

            DataContext = this;
            CourseNameTextBox.Text = Course.CourseName;
            CourseDescriptionTextBox.Text = Course.CourseDescription;
            colorCube.Background = new SolidColorBrush(GetColorFromString(Course.Color));
        }

        private Color GetRandomColor()
        {
            var random = new Random();
            return AvailableColors[random.Next(AvailableColors.Count)].Color;
        }

        private Color GetColorFromString(string colorString)
        {
            return (Color)ColorConverter.ConvertFromString(colorString);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Course.CourseName = CourseNameTextBox.Text;
            Course.CourseDescription = CourseDescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(CourseNameTextBox.Text))
            {
                MessageBox.Show("Название курса не может быть пустым!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                CourseNameTextBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colorComboBox.SelectedItem is ColorWrapper selectedColor)
            {
                Course.Color = selectedColor.Color.ToString();
                colorCube.Background = new SolidColorBrush(selectedColor.Color);
            }
        }
    }
}
