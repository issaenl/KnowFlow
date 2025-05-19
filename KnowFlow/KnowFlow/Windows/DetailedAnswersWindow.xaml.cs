using KnowFlow.Models;
using System.Windows;

namespace KnowFlow.Pages
{
    public partial class DetailedAnswersWindow : Window
    {
        public TestResultDisplay Result { get; }

        public DetailedAnswersWindow(TestResultDisplay result)
        {
            InitializeComponent();
            Result = result;
            DataContext = this;
        }
    }
}