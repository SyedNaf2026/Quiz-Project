import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, QuizAttemptStatusDTO, QuizResultDTO, SubmitQuizDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class QuizAttemptService {
  private baseUrl = 'https://localhost:7220/api/quizattempt';

  constructor(private http: HttpClient) {}

  submitQuiz(data: SubmitQuizDTO): Observable<ApiResponse<QuizResultDTO>> {
    return this.http.post<ApiResponse<QuizResultDTO>>(`${this.baseUrl}/submit`, data);
  }

  getMyResults(): Observable<ApiResponse<QuizResultDTO[]>> {
    return this.http.get<ApiResponse<QuizResultDTO[]>>(`${this.baseUrl}/my-results`);
  }

  getAttemptStatus(): Observable<ApiResponse<QuizAttemptStatusDTO[]>> {
    return this.http.get<ApiResponse<QuizAttemptStatusDTO[]>>(`${this.baseUrl}/attempt-status`);
  }

  reviewResult(quizId: number): Observable<ApiResponse<QuizResultDTO>> {
    return this.http.get<ApiResponse<QuizResultDTO>>(`${this.baseUrl}/review/${quizId}`);
  }
}
