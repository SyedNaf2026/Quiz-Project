import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse, CreateQuizDTO, QuizDTO, UpdateQuizDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class QuizService {
  private baseUrl = 'https://localhost:7220/api/quiz';

  constructor(private http: HttpClient) {}

  getActiveQuizzes(categoryId?: number): Observable<ApiResponse<QuizDTO[]>> {
    const url = categoryId ? `${this.baseUrl}?categoryId=${categoryId}` : this.baseUrl;
    return this.http.get<ApiResponse<QuizDTO[]>>(url);
  }

  getQuizById(id: number): Observable<ApiResponse<QuizDTO>> {
    return this.http.get<ApiResponse<QuizDTO>>(`${this.baseUrl}/${id}`);
  }

  getMyQuizzes(): Observable<ApiResponse<QuizDTO[]>> {
    return this.http.get<ApiResponse<QuizDTO[]>>(`${this.baseUrl}/my-quizzes`);
  }

  createQuiz(data: CreateQuizDTO): Observable<ApiResponse<QuizDTO>> {
    return this.http.post<ApiResponse<QuizDTO>>(this.baseUrl, data);
  }

  updateQuiz(id: number, data: UpdateQuizDTO): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.baseUrl}/${id}`, data);
  }

  deleteQuiz(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.baseUrl}/${id}`);
  }

  toggleStatus(id: number): Observable<ApiResponse<string>> {
    return this.http.patch<ApiResponse<string>>(`${this.baseUrl}/${id}/toggle-status`, {});
  }

  getStats(id: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/${id}/stats`);
  }
}
