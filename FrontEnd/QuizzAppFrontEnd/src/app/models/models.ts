// ===== Auth =====
export interface LoginModel {
  email: string;
  password: string;
  role: string;
}

export interface RegisterModel {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  role: string;
}

// ===== API Wrapper =====
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

// ===== Category =====
export interface CategoryDTO {
  id: number;
  name: string;
  description: string;
}

export interface CreateCategoryDTO {
  name: string;
  description: string;
}

// ===== Quiz =====
export interface QuizDTO {
  id: number;
  title: string;
  description: string;
  categoryName: string;
  timeLimit: number | null;
  isActive: boolean;
  creatorName: string;
  createdAt: string;
  totalQuestions: number;
  difficulty?: string;
}

export interface CreateQuizDTO {
  title: string;
  description: string;
  categoryId: number;
  timeLimit: number | null;
  difficulty?: string;
}

export interface UpdateQuizDTO {
  title: string;
  description: string;
  categoryId: number;
  timeLimit: number | null;
  isActive: boolean;
  difficulty?: string;
}

// ===== Question =====
export interface OptionDTO {
  id: number;
  optionText: string;
  isCorrect: boolean;
}

export interface QuestionDTO {
  id: number;
  quizId: number;
  questionText: string;
  questionType: string; // MultipleChoice | MultipleAnswer | TrueFalse | YesNo
  options: OptionDTO[];
}

export interface CreateOptionDTO {
  optionText: string;
  isCorrect: boolean;
}

export interface CreateQuestionDTO {
  quizId: number;
  questionText: string;
  questionType: string;
  options: CreateOptionDTO[];
}

// ===== Quiz Attempt =====
export interface AnswerDTO {
  questionId: number;
  selectedOptionId: number;          // single-answer types
  selectedOptionIds: number[];       // MultipleAnswer type
}

export interface SubmitQuizDTO {
  quizId: number;
  answers: AnswerDTO[];
}

export interface AnswerResultDTO {
  questionId: number;
  questionText: string;
  questionType: string;
  // single-answer
  selectedOptionId: number;
  selectedOptionText: string;
  correctOptionId: number;
  correctOptionText: string;
  // multi-answer
  selectedOptionIds: number[];
  selectedOptionTexts: string[];
  correctOptionIds: number[];
  correctOptionTexts: string[];
  isCorrect: boolean;
}

export interface QuizResultDTO {
  resultId: number;
  quizId: number;
  quizTitle: string;
  score: number;
  totalQuestions: number;
  percentage: number;
  completedAt: string;
  answerBreakdown: AnswerResultDTO[];
}

// ===== Leaderboard =====
export interface LeaderboardDTO {
  rank: number;
  userName: string;
  quizTitle: string;
  score: number;
  totalQuestions: number;
  percentage: number;
  completedAt: string;
}

// ===== User =====
export interface UserProfileDTO {
  id: number;
  fullName: string;
  email: string;
  role: string;
}

export interface UserStatsDTO {
  totalAttempts: number;
  averageScore: number;
  bestScore: number;
  totalQuizzesTaken: number;
  bestCategory: string;
}

export interface UpdateProfileDTO {
  fullName: string;
  email: string;
}

// ===== Notifications =====
export interface NotificationDTO {
  id: number;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

// ===== Group Manager =====
export interface GroupDTO {
  id: number;
  name: string;
  description: string;
  creatorName: string;
  memberCount: number;
  quizCount: number;
  createdAt: string;
}

export interface CreateGroupDTO {
  name: string;
  description: string;
}

export interface GroupMemberDTO {
  id: number;
  userId: number;
  fullName: string;
  email: string;
  joinedAt: string;
}

export interface GroupQuizDTO {
  groupQuizId: number;
  quizId: number;
  quizTitle: string;
  categoryName: string;
  totalQuestions: number;
  assignedAt: string;
  requiresValidation: boolean;
}

export interface GroupQuizResultDTO {
  id: number;
  userId: number;
  userName: string;
  groupName: string;
  quizTitle: string;
  score: number;
  totalQuestions: number;
  percentage: number;
  validationStatus: string;
  requiresValidation: boolean;
  submittedAt: string;
}

export interface UserSearchDTO {
  id: number;
  fullName: string;
  email: string;
}
