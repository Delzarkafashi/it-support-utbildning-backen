using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using backen_it_support_utbildning.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace backen_it_support_utbildning.Services
{
    public class QuizService
    {
        private readonly string _connectionString;

        public QuizService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateQuizAsync(QuizCreateDto quizDto)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var cmd = new MySqlCommand("INSERT INTO quizzes (name, category) VALUES (@name, @category)", connection, (MySqlTransaction)transaction);
                cmd.Parameters.AddWithValue("@name", quizDto.Name);
                cmd.Parameters.AddWithValue("@category", quizDto.Category);
                await cmd.ExecuteNonQueryAsync();
                int quizId = (int)cmd.LastInsertedId;

                foreach (var q in quizDto.Questions)
                {
                    var qCmd = new MySqlCommand(
                        "INSERT INTO questions (quiz_id, question_text, correct_answer, answers) VALUES (@quizId, @text, @correct, @answers)",
                        connection, (MySqlTransaction)transaction
                    );

                    qCmd.Parameters.AddWithValue("@quizId", quizId);
                    qCmd.Parameters.AddWithValue("@text", q.QuestionText);
                    qCmd.Parameters.AddWithValue("@correct", q.CorrectAnswer);
                    qCmd.Parameters.AddWithValue("@answers", JsonConvert.SerializeObject(q.Answers));

                    await qCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return quizId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task UpdateQuizAsync(int id, QuizCreateDto quizDto)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var cmd = new MySqlCommand(
                    "UPDATE quizzes SET name = @name, category = @category WHERE id = @id",
                    connection, (MySqlTransaction)transaction);
                cmd.Parameters.AddWithValue("@name", quizDto.Name);
                cmd.Parameters.AddWithValue("@category", quizDto.Category);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();

                var delCmd = new MySqlCommand("DELETE FROM questions WHERE quiz_id = @id", connection, (MySqlTransaction)transaction);
                delCmd.Parameters.AddWithValue("@id", id);
                await delCmd.ExecuteNonQueryAsync();

                foreach (var q in quizDto.Questions)
                {
                    var insert = new MySqlCommand(
                        "INSERT INTO questions (quiz_id, question_text, correct_answer, answers) VALUES (@quizId, @text, @correct, @answers)",
                        connection, (MySqlTransaction)transaction);
                    insert.Parameters.AddWithValue("@quizId", id);
                    insert.Parameters.AddWithValue("@text", q.QuestionText);
                    insert.Parameters.AddWithValue("@correct", q.CorrectAnswer);
                    insert.Parameters.AddWithValue("@answers", JsonConvert.SerializeObject(q.Answers));
                    await insert.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateQuizAsync(int id, QuizDto dto)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                var updateCmd = new MySqlCommand("UPDATE quizzes SET name = @name, category = @category WHERE id = @id", conn, (MySqlTransaction)tx);
                updateCmd.Parameters.AddWithValue("@name", dto.Name);
                updateCmd.Parameters.AddWithValue("@category", dto.Category);
                updateCmd.Parameters.AddWithValue("@id", id);
                await updateCmd.ExecuteNonQueryAsync();


                var deleteCmd = new MySqlCommand("DELETE FROM questions WHERE quiz_id = @id", conn, (MySqlTransaction)tx);
                deleteCmd.Parameters.AddWithValue("@id", id);
                await deleteCmd.ExecuteNonQueryAsync();

                foreach (var q in dto.Questions)
                {
                    var insertCmd = new MySqlCommand(
                        "INSERT INTO questions (quiz_id, question_text, correct_answer, answers) VALUES (@quizId, @text, @correct, @answers)",
                        conn, (MySqlTransaction)tx
                    );
                    insertCmd.Parameters.AddWithValue("@quizId", id);
                    insertCmd.Parameters.AddWithValue("@text", q.QuestionText);
                    insertCmd.Parameters.AddWithValue("@correct", q.CorrectAnswer);
                    insertCmd.Parameters.AddWithValue("@answers", JsonConvert.SerializeObject(q.Answers));
                    await insertCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<QuizDto>> GetAllQuizzesAsync()
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var quizzes = new List<QuizDto>();

            var cmd = new MySqlCommand("SELECT * FROM quizzes", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var quizList = new List<(int id, string name, string category)>();
            while (await reader.ReadAsync())
            {
                quizList.Add((reader.GetInt32("id"), reader.GetString("name"), reader.GetString("category")));
            }
            await reader.CloseAsync();

            foreach (var quiz in quizList)
            {
                var questionsCmd = new MySqlCommand("SELECT * FROM questions WHERE quiz_id = @id", conn);
                questionsCmd.Parameters.AddWithValue("@id", quiz.id);
                using var qReader = await questionsCmd.ExecuteReaderAsync();

                var questions = new List<QuestionDto>();
                while (await qReader.ReadAsync())
                {
                    questions.Add(new QuestionDto
                    {
                        QuestionText = qReader.GetString("question_text"),
                        CorrectAnswer = qReader.GetString("correct_answer"),
                        Answers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(qReader.GetString("answers"))
                    });
                }
                await qReader.CloseAsync();

                quizzes.Add(new QuizDto
                {
                    Id = quiz.id,
                    Name = quiz.name,
                    Category = quiz.category,
                    Questions = questions
                });
            }

            return quizzes;
        }
        public async Task DeleteQuizAsync(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("DELETE FROM quizzes WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<QuizDto>> GetQuizzesByCategoryAsync(string category)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var quizzes = new List<QuizDto>();

            var cmd = new MySqlCommand("SELECT * FROM quizzes WHERE category = @category", conn);
            cmd.Parameters.AddWithValue("@category", category);
            using var reader = await cmd.ExecuteReaderAsync();

            var quizList = new List<(int id, string name, string category)>();
            while (await reader.ReadAsync())
            {
                quizList.Add((reader.GetInt32("id"), reader.GetString("name"), reader.GetString("category")));
            }
            await reader.CloseAsync();

            foreach (var quiz in quizList)
            {
                var questionsCmd = new MySqlCommand("SELECT * FROM questions WHERE quiz_id = @id", conn);
                questionsCmd.Parameters.AddWithValue("@id", quiz.id);
                using var qReader = await questionsCmd.ExecuteReaderAsync();

                var questions = new List<QuestionDto>();
                while (await qReader.ReadAsync())
                {
                    questions.Add(new QuestionDto
                    {
                        QuestionText = qReader.GetString("question_text"),
                        CorrectAnswer = qReader.GetString("correct_answer"),
                        Answers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(qReader.GetString("answers"))
                    });
                }
                await qReader.CloseAsync();

                quizzes.Add(new QuizDto
                {
                    Id = quiz.id,
                    Name = quiz.name,
                    Category = quiz.category,
                    Questions = questions
                });
            }

            return quizzes;
        }



    }
}
