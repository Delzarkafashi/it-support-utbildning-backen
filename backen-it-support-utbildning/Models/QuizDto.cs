using System.Collections.Generic;

namespace backen_it_support_utbildning.Models
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class QuestionDto
    {
        public string QuestionText { get; set; }
        public List<string> Answers { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
