-- ============================================================
-- QUICK FIX SCRIPT — Run in SQL Server Management Studio
-- 10 categories, 25 quizzes, 125 questions, 500 options
-- Register 2 QuizCreator accounts in the app FIRST
-- ============================================================

-- Step 1: Add missing columns if they don't exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Questions') AND name = 'QuestionType')
    ALTER TABLE Questions ADD QuestionType NVARCHAR(50) NOT NULL DEFAULT 'MultipleChoice';
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quizzes') AND name = 'Difficulty')
    ALTER TABLE Quizzes ADD Difficulty NVARCHAR(20) NULL;
GO

-- Step 2: Clear all data (Users are preserved)
DELETE FROM UserAnswers;
DELETE FROM QuizResults;
DELETE FROM Options;
DELETE FROM Questions;
DELETE FROM Quizzes;
DELETE FROM Categories;

-- Step 3: Reset identity seeds
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('Quizzes',    RESEED, 0);
DBCC CHECKIDENT ('Questions',  RESEED, 0);
DBCC CHECKIDENT ('Options',    RESEED, 0);

-- Step 4: Pick 2 QuizCreators — register them in the app first
-- If only 1 exists, both variables point to the same person
DECLARE @Creator1 INT = (SELECT TOP 1 Id FROM Users WHERE Role = 'QuizCreator' ORDER BY Id ASC);
DECLARE @Creator2 INT = (SELECT TOP 1 Id FROM Users WHERE Role = 'QuizCreator' ORDER BY Id DESC);

-- Step 5: Categories
INSERT INTO Categories (Name, Description) VALUES
('General Knowledge', 'Test your everyday general knowledge'),
('Technology',        'Questions on computers, software and the internet'),
('Science',           'Physics, Chemistry, Biology and more'),
('Sports',            'Cricket, Football, Olympics and world sports'),
('Geography',         'Countries, capitals, rivers and world geography'),
('History',           'Ancient, medieval and modern world history'),
('Movies & TV',       'Hollywood, Bollywood and popular TV shows'),
('Mathematics',       'Numbers, algebra, geometry and logic'),
('Health & Fitness',  'Nutrition, exercise, medicine and wellness'),
('Space & Astronomy', 'Planets, stars, galaxies and space exploration');

-- Step 6: Quizzes — Creator1 gets 1-13, Creator2 gets 14-25
INSERT INTO Quizzes (Title, Description, CategoryId, CreatedBy, TimeLimit, IsActive, CreatedAt, Difficulty) VALUES
('GK Basics',             'Basic general knowledge for everyone',              1, @Creator1, 10, 1, GETDATE(), 'Easy'),
('World Facts',           'Interesting facts about the world',                 1, @Creator1, 10, 1, GETDATE(), 'Medium'),
('Tech Fundamentals',     'Core concepts in technology and computing',         2, @Creator1, 10, 1, GETDATE(), 'Easy'),
('Web Development',       'HTML, CSS, JavaScript and frameworks',              2, @Creator1, 10, 1, GETDATE(), 'Medium');

INSERT INTO Quizzes (Title, Description, CategoryId, CreatedBy, TimeLimit, IsActive, CreatedAt, Difficulty) VALUES
('Science Basics',        'Fundamental science questions',                     3, @Creator1, 10, 1, GETDATE(), 'Easy'),
('Human Body',            'Questions about the human body and biology',        3, @Creator1, 10, 1, GETDATE(), 'Medium'),
('Cricket Quiz',          'All about cricket - the gentleman''s game',         4, @Creator1, 10, 1, GETDATE(), 'Medium'),
('Football Quiz',         'World football and FIFA trivia',                    4, @Creator1, 10, 1, GETDATE(), 'Hard'),
('Indian Geography',      'Rivers, states, capitals of India',                 5, @Creator1, 10, 1, GETDATE(), 'Easy'),
('World Geography',       'Countries, capitals and continents',                5, @Creator1, 10, 1, GETDATE(), 'Hard'),
('Ancient Civilizations', 'Egypt, Rome, Greece and ancient empires',           6, @Creator1, 10, 1, GETDATE(), 'Medium'),
('Indian History',        'Mughal empire, freedom struggle and more',          6, @Creator1, 10, 1, GETDATE(), 'Medium'),
('World War Trivia',      'Events and facts from WW1 and WW2',                 6, @Creator1, 10, 1, GETDATE(), 'Hard');

INSERT INTO Quizzes (Title, Description, CategoryId, CreatedBy, TimeLimit, IsActive, CreatedAt, Difficulty) VALUES
('Bollywood Blockbusters','Famous Hindi movies and actors',                    7, @Creator2, 10, 1, GETDATE(), 'Easy'),
('Hollywood Hits',        'Popular English movies and directors',              7, @Creator2, 10, 1, GETDATE(), 'Medium'),
('Basic Maths',           'Arithmetic, fractions and simple algebra',          8, @Creator2, 10, 1, GETDATE(), 'Easy'),
('Logic & Puzzles',       'Number patterns, sequences and brain teasers',      8, @Creator2, 10, 1, GETDATE(), 'Hard'),
('Nutrition Facts',       'Vitamins, minerals and healthy eating',             9, @Creator2, 10, 1, GETDATE(), 'Easy'),
('Medical Basics',        'Common diseases, symptoms and treatments',          9, @Creator2, 10, 1, GETDATE(), 'Medium'),
('Solar System',          'Planets, moons and our solar neighbourhood',       10, @Creator2, 10, 1, GETDATE(), 'Easy'),
('Deep Space',            'Stars, galaxies, black holes and the universe',    10, @Creator2, 10, 1, GETDATE(), 'Hard'),
('Olympics Trivia',       'Summer and Winter Olympics facts',                  4, @Creator2, 10, 1, GETDATE(), 'Medium'),
('Famous Inventions',     'Who invented what and when',                        6, @Creator2, 10, 1, GETDATE(), 'Medium'),
('Coding Concepts',       'Programming logic, data structures and algorithms', 2, @Creator2, 10, 1, GETDATE(), 'Hard'),
('Animal Kingdom',        'Facts about animals, birds and marine life',        3, @Creator2, 10, 1, GETDATE(), 'Easy');

-- ============================================================
-- Step 7: Questions (5 per quiz = 125 questions)
-- ============================================================

INSERT INTO Questions (QuizId, QuestionText, QuestionType) VALUES
(1,'Who wrote the national anthem of India?','MultipleChoice'),
(1,'How many days are there in a leap year?','MultipleChoice'),
(1,'What is the largest planet in our solar system?','MultipleChoice'),
(1,'Which metal is liquid at room temperature?','MultipleChoice'),
(1,'What is the capital of France?','MultipleChoice'),
(2,'Which is the longest river in the world?','MultipleChoice'),
(2,'Which country has the largest population?','MultipleChoice'),
(2,'What is the smallest country in the world?','MultipleChoice'),
(2,'Which ocean is the largest?','MultipleChoice'),
(2,'Which country has the most land area?','MultipleChoice'),
(3,'What does CPU stand for?','MultipleChoice'),
(3,'Which language is known as the mother of all languages?','MultipleChoice'),
(3,'What does RAM stand for?','MultipleChoice'),
(3,'Which company developed Windows?','MultipleChoice'),
(3,'What does GPU stand for?','MultipleChoice'),
(4,'What does HTML stand for?','MultipleChoice'),
(4,'Which language is used for styling web pages?','MultipleChoice'),
(4,'What does API stand for?','MultipleChoice'),
(4,'Which JS framework is developed by Google?','MultipleChoice'),
(4,'What does DOM stand for?','MultipleChoice'),
(5,'What is the chemical symbol for water?','MultipleChoice'),
(5,'How many bones are in the adult human body?','MultipleChoice'),
(5,'What is the speed of light (approx)?','MultipleChoice'),
(5,'Which gas do plants absorb from the atmosphere?','MultipleChoice'),
(5,'What is the atomic number of Carbon?','MultipleChoice');

INSERT INTO Questions (QuizId, QuestionText, QuestionType) VALUES
(6,'Which is the largest organ in the human body?','MultipleChoice'),
(6,'How many chambers does the human heart have?','MultipleChoice'),
(6,'Which blood group is the universal donor?','MultipleChoice'),
(6,'What is the powerhouse of the cell?','MultipleChoice'),
(6,'How many teeth does an adult human have?','MultipleChoice'),
(7,'How many players are in a cricket team?','MultipleChoice'),
(7,'Which country won the first Cricket World Cup in 1975?','MultipleChoice'),
(7,'Max overs in a One Day International?','MultipleChoice'),
(7,'Who holds the highest individual score in Test cricket?','MultipleChoice'),
(7,'What is the length of a cricket pitch in yards?','MultipleChoice'),
(8,'Which country won FIFA World Cup 2022?','MultipleChoice'),
(8,'How many players per team on the field?','MultipleChoice'),
(8,'Who has won the most Ballon d''Or awards?','MultipleChoice'),
(8,'How long is a standard football match?','MultipleChoice'),
(8,'Which country has won the most FIFA World Cups?','MultipleChoice'),
(9,'What is the capital of India?','MultipleChoice'),
(9,'Which is the longest river in India?','MultipleChoice'),
(9,'How many states are there in India?','MultipleChoice'),
(9,'Which is the largest state in India by area?','MultipleChoice'),
(9,'Which is the smallest state in India by area?','MultipleChoice'),
(10,'What is the capital of Australia?','MultipleChoice'),
(10,'Which is the largest continent?','MultipleChoice'),
(10,'Which country has the most natural lakes?','MultipleChoice'),
(10,'What is the tallest mountain in the world?','MultipleChoice'),
(10,'Which is the longest mountain range in the world?','MultipleChoice'),
(11,'Which ancient wonder was located in Alexandria?','MultipleChoice'),
(11,'Who was the first emperor of Rome?','MultipleChoice'),
(11,'Which civilization built Machu Picchu?','MultipleChoice'),
(11,'What was the currency of ancient Rome called?','MultipleChoice'),
(11,'Which ancient civilization built the pyramids?','MultipleChoice'),
(12,'Who was the first Prime Minister of India?','MultipleChoice'),
(12,'In which year did India gain independence?','MultipleChoice'),
(12,'Who founded the Mughal Empire in India?','MultipleChoice'),
(12,'Which movement was launched by Gandhi in 1942?','MultipleChoice'),
(12,'Who was the first President of India?','MultipleChoice'),
(13,'In which year did World War 1 begin?','MultipleChoice'),
(13,'Which country was NOT part of the Allied Powers in WW2?','MultipleChoice'),
(13,'What was the name of the first atomic bomb dropped?','MultipleChoice'),
(13,'Which treaty ended World War 1?','MultipleChoice'),
(13,'In which year did World War 2 end?','MultipleChoice'),
(14,'Which film won the first Filmfare Best Film Award?','MultipleChoice'),
(14,'Who is known as the King of Bollywood?','MultipleChoice'),
(14,'Which movie features the song Jai Ho?','MultipleChoice'),
(14,'Who directed the movie Lagaan?','MultipleChoice'),
(14,'Which Bollywood movie was India''s first Oscar submission?','MultipleChoice'),
(15,'Who directed the movie Titanic?','MultipleChoice'),
(15,'Which film won the most Academy Awards ever?','MultipleChoice'),
(15,'Who played Iron Man in the Marvel movies?','MultipleChoice'),
(15,'Which movie features the quote "I''ll be back"?','MultipleChoice'),
(15,'Which studio produced the Avengers movies?','MultipleChoice');

INSERT INTO Questions (QuizId, QuestionText, QuestionType) VALUES
(16,'What is the square root of 144?','MultipleChoice'),
(16,'What is 15% of 200?','MultipleChoice'),
(16,'How many sides does a hexagon have?','MultipleChoice'),
(16,'What is the value of Pi (approx)?','MultipleChoice'),
(16,'What is 2 to the power of 10?','MultipleChoice'),
(17,'What comes next: 2, 4, 8, 16, ?','MultipleChoice'),
(17,'If all roses are flowers and some flowers fade, what can we conclude?','MultipleChoice'),
(17,'What is the next prime number after 7?','MultipleChoice'),
(17,'A clock shows 3:15. What is the angle between the hands?','MultipleChoice'),
(17,'What comes next: 1, 1, 2, 3, 5, 8, ?','MultipleChoice'),
(18,'Which vitamin is produced by sunlight?','MultipleChoice'),
(18,'Which food is the richest source of Vitamin C?','MultipleChoice'),
(18,'How many calories are in 1 gram of fat?','MultipleChoice'),
(18,'Which mineral is essential for strong bones?','MultipleChoice'),
(18,'Which nutrient provides the most energy per gram?','MultipleChoice'),
(19,'What is the normal human body temperature in Celsius?','MultipleChoice'),
(19,'Which organ produces insulin?','MultipleChoice'),
(19,'What does ECG stand for?','MultipleChoice'),
(19,'Which vitamin deficiency causes Rickets?','MultipleChoice'),
(19,'What is the normal resting heart rate for adults?','MultipleChoice'),
(20,'Which is the closest planet to the Sun?','MultipleChoice'),
(20,'How many moons does Mars have?','MultipleChoice'),
(20,'Which planet has the most rings?','MultipleChoice'),
(20,'What is the name of Earth''s natural satellite?','MultipleChoice'),
(20,'Which is the hottest planet in the solar system?','MultipleChoice'),
(21,'What is a light year a measure of?','MultipleChoice'),
(21,'Which is the nearest star to Earth (after the Sun)?','MultipleChoice'),
(21,'What is the name of the galaxy we live in?','MultipleChoice'),
(21,'What is the term for a star that has exploded?','MultipleChoice'),
(21,'What force holds galaxies together?','MultipleChoice'),
(22,'In which city were the 2020 Summer Olympics held?','MultipleChoice'),
(22,'How many rings are on the Olympic flag?','MultipleChoice'),
(22,'Which country has won the most Olympic gold medals overall?','MultipleChoice'),
(22,'How often are the Summer Olympics held?','MultipleChoice'),
(22,'In which country did the ancient Olympics originate?','MultipleChoice'),
(23,'Who invented the telephone?','MultipleChoice'),
(23,'In which year was the World Wide Web invented?','MultipleChoice'),
(23,'Who invented the light bulb?','MultipleChoice'),
(23,'Which country invented paper?','MultipleChoice'),
(23,'Who invented the steam engine?','MultipleChoice'),
(24,'What does OOP stand for?','MultipleChoice'),
(24,'Which data structure uses LIFO order?','MultipleChoice'),
(24,'What is the time complexity of binary search?','MultipleChoice'),
(24,'Which sorting algorithm has the best average case complexity?','MultipleChoice'),
(24,'What does SQL stand for?','MultipleChoice'),
(25,'Which is the fastest land animal?','MultipleChoice'),
(25,'How many hearts does an octopus have?','MultipleChoice'),
(25,'Which bird cannot fly?','MultipleChoice'),
(25,'What is the largest mammal in the world?','MultipleChoice'),
(25,'Which animal has the longest lifespan?','MultipleChoice');

-- ============================================================
-- Step 8: Options (4 per question = 500 options)
-- ============================================================

-- Q1-5 (Quiz 1: GK Basics)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(1,'Rabindranath Tagore',1),(1,'Mahatma Gandhi',0),(1,'Jawaharlal Nehru',0),(1,'Subhas Chandra Bose',0),
(2,'366',1),(2,'365',0),(2,'364',0),(2,'367',0),
(3,'Jupiter',1),(3,'Saturn',0),(3,'Neptune',0),(3,'Mars',0),
(4,'Mercury',1),(4,'Iron',0),(4,'Gold',0),(4,'Silver',0),
(5,'Paris',1),(5,'London',0),(5,'Berlin',0),(5,'Rome',0);

-- Q6-10 (Quiz 2: World Facts)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(6,'Nile',1),(6,'Amazon',0),(6,'Yangtze',0),(6,'Mississippi',0),
(7,'India',1),(7,'China',0),(7,'USA',0),(7,'Indonesia',0),
(8,'Vatican City',1),(8,'Monaco',0),(8,'San Marino',0),(8,'Liechtenstein',0),
(9,'Pacific Ocean',1),(9,'Atlantic Ocean',0),(9,'Indian Ocean',0),(9,'Arctic Ocean',0),
(10,'Russia',1),(10,'Canada',0),(10,'USA',0),(10,'China',0);

-- Q11-15 (Quiz 3: Tech Fundamentals)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(11,'Central Processing Unit',1),(11,'Central Program Unit',0),(11,'Computer Processing Unit',0),(11,'Core Processing Unit',0),
(12,'C',1),(12,'Python',0),(12,'Java',0),(12,'COBOL',0),
(13,'Random Access Memory',1),(13,'Read Access Memory',0),(13,'Run Access Memory',0),(13,'Rapid Access Memory',0),
(14,'Microsoft',1),(14,'Apple',0),(14,'Google',0),(14,'IBM',0),
(15,'Graphics Processing Unit',1),(15,'General Processing Unit',0),(15,'Graphical Program Unit',0),(15,'Global Processing Unit',0);

-- Q16-20 (Quiz 4: Web Development)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(16,'HyperText Markup Language',1),(16,'HighText Machine Language',0),(16,'HyperText Machine Language',0),(16,'HyperTool Markup Language',0),
(17,'CSS',1),(17,'JavaScript',0),(17,'Python',0),(17,'XML',0),
(18,'Application Programming Interface',1),(18,'Applied Program Interface',0),(18,'Application Process Interface',0),(18,'Automated Program Interface',0),
(19,'Angular',1),(19,'React',0),(19,'Vue',0),(19,'Svelte',0),
(20,'Document Object Model',1),(20,'Data Object Model',0),(20,'Document Oriented Model',0),(20,'Dynamic Object Model',0);

-- Q21-25 (Quiz 5: Science Basics)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(21,'H2O',1),(21,'CO2',0),(21,'O2',0),(21,'H2',0),
(22,'206',1),(22,'208',0),(22,'200',0),(22,'212',0),
(23,'3 x 10^8 m/s',1),(23,'3 x 10^6 m/s',0),(23,'3 x 10^10 m/s',0),(23,'3 x 10^4 m/s',0),
(24,'Carbon Dioxide',1),(24,'Oxygen',0),(24,'Nitrogen',0),(24,'Hydrogen',0),
(25,'6',1),(25,'8',0),(25,'12',0),(25,'4',0);

-- Q26-30 (Quiz 6: Human Body)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(26,'Skin',1),(26,'Liver',0),(26,'Brain',0),(26,'Lungs',0),
(27,'4',1),(27,'2',0),(27,'3',0),(27,'6',0),
(28,'O negative',1),(28,'A positive',0),(28,'B negative',0),(28,'AB positive',0),
(29,'Mitochondria',1),(29,'Nucleus',0),(29,'Ribosome',0),(29,'Vacuole',0),
(30,'32',1),(30,'28',0),(30,'30',0),(30,'36',0);

-- Q31-35 (Quiz 7: Cricket)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(31,'11',1),(31,'9',0),(31,'12',0),(31,'10',0),
(32,'West Indies',1),(32,'Australia',0),(32,'England',0),(32,'India',0),
(33,'50',1),(33,'40',0),(33,'60',0),(33,'45',0),
(34,'Brian Lara',1),(34,'Sachin Tendulkar',0),(34,'Ricky Ponting',0),(34,'Don Bradman',0),
(35,'22',1),(35,'20',0),(35,'24',0),(35,'18',0);

-- Q36-40 (Quiz 8: Football)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(36,'Argentina',1),(36,'France',0),(36,'Brazil',0),(36,'Germany',0),
(37,'11',1),(37,'10',0),(37,'12',0),(37,'9',0),
(38,'Lionel Messi',1),(38,'Cristiano Ronaldo',0),(38,'Ronaldinho',0),(38,'Zinedine Zidane',0),
(39,'90 minutes',1),(39,'80 minutes',0),(39,'100 minutes',0),(39,'75 minutes',0),
(40,'Brazil',1),(40,'Germany',0),(40,'Italy',0),(40,'Argentina',0);

-- Q41-45 (Quiz 9: Indian Geography)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(41,'New Delhi',1),(41,'Mumbai',0),(41,'Kolkata',0),(41,'Chennai',0),
(42,'Ganga',1),(42,'Yamuna',0),(42,'Godavari',0),(42,'Brahmaputra',0),
(43,'28',1),(43,'29',0),(43,'27',0),(43,'30',0),
(44,'Rajasthan',1),(44,'Madhya Pradesh',0),(44,'Maharashtra',0),(44,'Uttar Pradesh',0),
(45,'Goa',1),(45,'Sikkim',0),(45,'Tripura',0),(45,'Manipur',0);

-- Q46-50 (Quiz 10: World Geography)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(46,'Canberra',1),(46,'Sydney',0),(46,'Melbourne',0),(46,'Brisbane',0),
(47,'Asia',1),(47,'Africa',0),(47,'Europe',0),(47,'North America',0),
(48,'Canada',1),(48,'Russia',0),(48,'USA',0),(48,'Finland',0),
(49,'Mount Everest',1),(49,'K2',0),(49,'Kangchenjunga',0),(49,'Lhotse',0),
(50,'Andes',1),(50,'Himalayas',0),(50,'Rockies',0),(50,'Alps',0);

-- Q51-55 (Quiz 11: Ancient Civilizations)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(51,'Lighthouse of Alexandria',1),(51,'Colosseum',0),(51,'Parthenon',0),(51,'Sphinx',0),
(52,'Augustus',1),(52,'Julius Caesar',0),(52,'Nero',0),(52,'Caligula',0),
(53,'Inca',1),(53,'Aztec',0),(53,'Maya',0),(53,'Olmec',0),
(54,'Denarius',1),(54,'Drachma',0),(54,'Talent',0),(54,'Shekel',0),
(55,'Egyptians',1),(55,'Romans',0),(55,'Greeks',0),(55,'Persians',0);

-- Q56-60 (Quiz 12: Indian History)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(56,'Jawaharlal Nehru',1),(56,'Mahatma Gandhi',0),(56,'Sardar Patel',0),(56,'B.R. Ambedkar',0),
(57,'1947',1),(57,'1945',0),(57,'1950',0),(57,'1942',0),
(58,'Babur',1),(58,'Akbar',0),(58,'Humayun',0),(58,'Shah Jahan',0),
(59,'Quit India Movement',1),(59,'Non-Cooperation Movement',0),(59,'Civil Disobedience',0),(59,'Swadeshi Movement',0),
(60,'Rajendra Prasad',1),(60,'Sardar Patel',0),(60,'B.R. Ambedkar',0),(60,'S. Radhakrishnan',0);

-- Q61-65 (Quiz 13: World War Trivia)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(61,'1914',1),(61,'1916',0),(61,'1912',0),(61,'1918',0),
(62,'Japan',1),(62,'France',0),(62,'USA',0),(62,'UK',0),
(63,'Little Boy',1),(63,'Fat Man',0),(63,'Big Bang',0),(63,'Thunder',0),
(64,'Treaty of Versailles',1),(64,'Treaty of Paris',0),(64,'Treaty of Berlin',0),(64,'Treaty of Rome',0),
(65,'1945',1),(65,'1943',0),(65,'1944',0),(65,'1946',0);

-- Q66-70 (Quiz 14: Bollywood)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(66,'Aan',1),(66,'Mother India',0),(66,'Mughal-E-Azam',0),(66,'Sholay',0),
(67,'Shah Rukh Khan',1),(67,'Salman Khan',0),(67,'Aamir Khan',0),(67,'Amitabh Bachchan',0),
(68,'Slumdog Millionaire',1),(68,'Lagaan',0),(68,'Dil Chahta Hai',0),(68,'Rang De Basanti',0),
(69,'Ashutosh Gowariker',1),(69,'Karan Johar',0),(69,'Sanjay Leela Bhansali',0),(69,'Rajkumar Hirani',0),
(70,'Mother India',1),(70,'Lagaan',0),(70,'Taare Zameen Par',0),(70,'3 Idiots',0);

-- Q71-75 (Quiz 15: Hollywood)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(71,'James Cameron',1),(71,'Steven Spielberg',0),(71,'Christopher Nolan',0),(71,'Ridley Scott',0),
(72,'Ben-Hur',1),(72,'Titanic',0),(72,'The Lord of the Rings',0),(72,'Gone with the Wind',0),
(73,'Robert Downey Jr.',1),(73,'Chris Evans',0),(73,'Chris Hemsworth',0),(73,'Mark Ruffalo',0),
(74,'The Terminator',1),(74,'Predator',0),(74,'Total Recall',0),(74,'Commando',0),
(75,'Marvel Studios',1),(75,'Warner Bros',0),(75,'Disney',0),(75,'Universal',0);

-- Q76-80 (Quiz 16: Basic Maths)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(76,'12',1),(76,'11',0),(76,'13',0),(76,'14',0),
(77,'30',1),(77,'25',0),(77,'35',0),(77,'20',0),
(78,'6',1),(78,'5',0),(78,'7',0),(78,'8',0),
(79,'3.14159',1),(79,'3.14169',0),(79,'3.14150',0),(79,'3.14199',0),
(80,'1024',1),(80,'512',0),(80,'2048',0),(80,'256',0);

-- Q81-85 (Quiz 17: Logic & Puzzles)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(81,'32',1),(81,'24',0),(81,'28',0),(81,'30',0),
(82,'Some roses may fade',1),(82,'All roses fade',0),(82,'No roses fade',0),(82,'Roses never fade',0),
(83,'11',1),(83,'9',0),(83,'13',0),(83,'10',0),
(84,'7.5 degrees',1),(84,'0 degrees',0),(84,'15 degrees',0),(84,'22.5 degrees',0),
(85,'13',1),(85,'11',0),(85,'15',0),(85,'12',0);

-- Q86-90 (Quiz 18: Nutrition Facts)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(86,'Vitamin D',1),(86,'Vitamin A',0),(86,'Vitamin C',0),(86,'Vitamin B12',0),
(87,'Guava',1),(87,'Orange',0),(87,'Lemon',0),(87,'Apple',0),
(88,'9',1),(88,'4',0),(88,'7',0),(88,'6',0),
(89,'Calcium',1),(89,'Iron',0),(89,'Zinc',0),(89,'Magnesium',0),
(90,'Fat',1),(90,'Protein',0),(90,'Carbohydrates',0),(90,'Fiber',0);

-- Q91-95 (Quiz 19: Medical Basics)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(91,'37',1),(91,'36',0),(91,'38',0),(91,'35',0),
(92,'Pancreas',1),(92,'Liver',0),(92,'Kidney',0),(92,'Spleen',0),
(93,'Electrocardiogram',1),(93,'Electro Cardiac Graph',0),(93,'Electronic Cardio Gram',0),(93,'Electro Cardio Gate',0),
(94,'Vitamin D',1),(94,'Vitamin C',0),(94,'Vitamin A',0),(94,'Vitamin B',0),
(95,'60-100 bpm',1),(95,'40-60 bpm',0),(95,'100-120 bpm',0),(95,'120-140 bpm',0);

-- Q96-100 (Quiz 20: Solar System)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(96,'Mercury',1),(96,'Venus',0),(96,'Earth',0),(96,'Mars',0),
(97,'2',1),(97,'0',0),(97,'1',0),(97,'3',0),
(98,'Saturn',1),(98,'Jupiter',0),(98,'Uranus',0),(98,'Neptune',0),
(99,'Moon',1),(99,'Titan',0),(99,'Europa',0),(99,'Phobos',0),
(100,'Venus',1),(100,'Mercury',0),(100,'Mars',0),(100,'Jupiter',0);

-- Q101-105 (Quiz 21: Deep Space)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(101,'Distance',1),(101,'Time',0),(101,'Speed',0),(101,'Mass',0),
(102,'Proxima Centauri',1),(102,'Sirius',0),(102,'Betelgeuse',0),(102,'Vega',0),
(103,'Milky Way',1),(103,'Andromeda',0),(103,'Triangulum',0),(103,'Whirlpool',0),
(104,'Supernova',1),(104,'Pulsar',0),(104,'Quasar',0),(104,'Nebula',0),
(105,'Gravity',1),(105,'Dark Matter',0),(105,'Magnetism',0),(105,'Nuclear Force',0);

-- Q106-110 (Quiz 22: Olympics)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(106,'Tokyo',1),(106,'Paris',0),(106,'London',0),(106,'Rio',0),
(107,'5',1),(107,'4',0),(107,'6',0),(107,'3',0),
(108,'USA',1),(108,'China',0),(108,'Russia',0),(108,'Germany',0),
(109,'Every 4 years',1),(109,'Every 2 years',0),(109,'Every 3 years',0),(109,'Every 5 years',0),
(110,'Greece',1),(110,'Italy',0),(110,'Egypt',0),(110,'China',0);

-- Q111-115 (Quiz 23: Famous Inventions)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(111,'Alexander Graham Bell',1),(111,'Thomas Edison',0),(111,'Nikola Tesla',0),(111,'Guglielmo Marconi',0),
(112,'1991',1),(112,'1989',0),(112,'1995',0),(112,'1985',0),
(113,'Thomas Edison',1),(113,'Nikola Tesla',0),(113,'Benjamin Franklin',0),(113,'James Watt',0),
(114,'China',1),(114,'Egypt',0),(114,'India',0),(114,'Greece',0),
(115,'James Watt',1),(115,'Thomas Edison',0),(115,'Nikola Tesla',0),(115,'George Stephenson',0);

-- Q116-120 (Quiz 24: Coding Concepts)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(116,'Object-Oriented Programming',1),(116,'Object-Oriented Process',0),(116,'Ordered Object Programming',0),(116,'Open Object Protocol',0),
(117,'Stack',1),(117,'Queue',0),(117,'Array',0),(117,'Tree',0),
(118,'O(log n)',1),(118,'O(n)',0),(118,'O(n^2)',0),(118,'O(1)',0),
(119,'Merge Sort',1),(119,'Bubble Sort',0),(119,'Selection Sort',0),(119,'Insertion Sort',0),
(120,'Structured Query Language',1),(120,'Simple Query Language',0),(120,'Standard Query Logic',0),(120,'Sequential Query Language',0);

-- Q121-125 (Quiz 25: Animal Kingdom)
INSERT INTO Options (QuestionId, OptionText, IsCorrect) VALUES
(121,'Cheetah',1),(121,'Lion',0),(121,'Horse',0),(121,'Leopard',0),
(122,'3',1),(122,'1',0),(122,'2',0),(122,'4',0),
(123,'Penguin',1),(123,'Parrot',0),(123,'Eagle',0),(123,'Flamingo',0),
(124,'Blue Whale',1),(124,'Elephant',0),(124,'Giraffe',0),(124,'Hippopotamus',0),
(125,'Greenland Shark',1),(125,'Tortoise',0),(125,'Elephant',0),(125,'Parrot',0);

-- ============================================================
-- Step 9: Verify counts
-- ============================================================
SELECT 'Users'      AS [Table], COUNT(*) AS Total FROM Users
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL SELECT 'Quizzes',    COUNT(*) FROM Quizzes
UNION ALL SELECT 'Questions',  COUNT(*) FROM Questions
UNION ALL SELECT 'Options',    COUNT(*) FROM Options;
