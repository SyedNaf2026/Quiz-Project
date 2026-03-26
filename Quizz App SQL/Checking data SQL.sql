-- Check users
SELECT Id, FullName, Email, Role FROM Users;
select * from Users

-- Check categories
SELECT * FROM Categories;

-- Check quizzes
SELECT q.Id, q.Title, c.Name AS Category, q.TimeLimit, q.IsActive 
FROM Quizzes q
JOIN Categories c ON c.Id = q.CategoryId;

-- Check questions with option counts
SELECT q.Id, q.QuestionText, COUNT(o.Id) AS OptionCount
FROM Questions q
LEFT JOIN Options o ON o.QuestionId = q.Id
GROUP BY q.Id, q.QuestionText
ORDER BY q.Id;

-- Check all options with correct answer marked
SELECT q.QuestionText, o.OptionText, 
       CASE WHEN o.IsCorrect = 1 THEN '✓ Correct' ELSE '' END AS Answer
FROM Questions q
JOIN Options o ON o.QuestionId = q.Id
ORDER BY q.Id, o.Id;

-- Check results and leaderboard (will be empty until someone takes a quiz)
SELECT * FROM QuizResults;
SELECT * FROM UserAnswers;

---------------------------------------------------------------------------------------

select * from Users

select * from Quizzes 

select * from Categories

select * from Questions

select * from Options

select * from QuizResults

select * from UserAnswers

SELECT Id, Title, CreatedBy FROM Quizzes WHERE Id = 3;

INSERT INTO Questions (QuizId, QuestionText)
VALUES (3, 'What is the capital of India?');

INSERT INTO Options (QuestionId, OptionText, IsCorrect)
VALUES 
(3, 'Mumbai', 0),
(3, 'New Delhi', 1),
(3, 'Chennai', 0),
(3, 'Kolkata', 0);

INSERT INTO Questions (QuizId, QuestionText)
VALUES (4, 'What is the recently released feature in automobile industry');

INSERT INTO Options (QuestionId, OptionText, IsCorrect)
VALUES
(2, 'Electric Vehicle Technology', 1),
(2, 'Steam Engine System', 0),
(2, 'Wooden Chassis Design', 0),
(2, 'Animal Powered Transmission', 0);

INSERT INTO Questions (QuizId, QuestionText)
VALUES (2, 'Which language is used in ASP.NET Core?');

INSERT INTO Options (QuestionId, OptionText, IsCorrect)
VALUES
(3, 'Java', 0),
(3, 'C#', 1),
(3, 'Python', 0),
(3, 'PHP', 0);

INSERT INTO QuizResults 
(UserId, QuizId, Score, TotalQuestions, Percentage, CompletedAt)
VALUES 
(2, 2, 8, 10, 80, GETDATE());

INSERT INTO QuizResults 
(UserId, QuizId, Score, TotalQuestions, Percentage, CompletedAt)
VALUES 
(2, 2, 6, 10, 60, GETDATE());

INSERT INTO UserAnswers
(UserId, QuizId, QuestionId, SelectedOptionId)
VALUES
(2, 2, 1, 5);


select * from Users
delete from  Users where Role = 'QuizTaker'
SELECT * FROM Users WHERE Role = 'QuizTaker';


-- Delete all users except Dayanand (11) and Syed Arshed (14)
DELETE FROM Users WHERE Id NOT IN (11, 14);

