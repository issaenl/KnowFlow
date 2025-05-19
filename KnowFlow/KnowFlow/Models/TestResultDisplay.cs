using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.Models
{
    public class TestResultDisplay
    {
        public string Username { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public int TotalPoints { get; set; }
        public ObservableCollection<QuestionResult> QuestionResults { get; set; }
    }
}
