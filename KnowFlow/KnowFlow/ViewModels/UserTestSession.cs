using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowFlow.ViewModels
{
    public class UserTestSession
    {
        public Dictionary<int, string> TextAnswers { get; } = new Dictionary<int, string>();
        public Dictionary<int, List<int>> SelectedAnswers { get; } = new Dictionary<int, List<int>>();

        public void AddTextAnswer(int questionId, string answer)
        {
            TextAnswers[questionId] = answer;
        }

        public void AddSelectedAnswer(int questionId, int answerId)
        {
            if (!SelectedAnswers.ContainsKey(questionId))
            {
                SelectedAnswers[questionId] = new List<int>();
            }
            SelectedAnswers[questionId].Add(answerId);
        }

        public void RemoveSelectedAnswer(int questionId, int answerId)
        {
            if (SelectedAnswers.ContainsKey(questionId))
            {
                SelectedAnswers[questionId].Remove(answerId);
            }
        }
    }
}
