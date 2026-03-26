import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, CreateQuestionDTO, QuestionDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private baseUrl = 'https://localhost:7220/api/question';

  constructor(private http: HttpClient) {}

  getByQuiz(quizId: number): Observable<ApiResponse<QuestionDTO[]>> {
    return this.http.get<ApiResponse<QuestionDTO[]>>(`${this.baseUrl}/quiz/${quizId}`);
  }

  addQuestion(data: CreateQuestionDTO): Observable<ApiResponse<QuestionDTO>> {
    return this.http.post<ApiResponse<QuestionDTO>>(this.baseUrl, data);
  }

  updateQuestion(id: number, newText: string): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.baseUrl}/${id}`, JSON.stringify(newText), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  deleteQuestion(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.baseUrl}/${id}`);
  }
}
