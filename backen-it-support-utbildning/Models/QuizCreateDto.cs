using System.Collections.Generic;

namespace backen_it_support_utbildning.Models
{
    public class QuizCreateDto
    {
        public string Name { get; set; }
        public string Category { get; set; } = "Okategoriserad";
        public List<QuestionCreateDto> Questions { get; set; }
    }

    public class QuestionCreateDto
    {
        public string QuestionText { get; set; }
        public List<string> Answers { get; set; }
        public string CorrectAnswer { get; set; } = "";
    }
}
