import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApiResponse, CreateGroupDTO, GroupDTO, GroupMemberDTO,
  GroupQuizDTO, GroupQuizResultDTO, UserSearchDTO
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class GroupService {
  private base = 'https://localhost:7220/api/group';

  constructor(private http: HttpClient) {}

  // Groups
  createGroup(dto: CreateGroupDTO): Observable<ApiResponse<GroupDTO>> {
    return this.http.post<ApiResponse<GroupDTO>>(this.base, dto);
  }

  getMyGroups(): Observable<ApiResponse<GroupDTO[]>> {
    return this.http.get<ApiResponse<GroupDTO[]>>(this.base);
  }

  getGroupById(id: number): Observable<ApiResponse<GroupDTO>> {
    return this.http.get<ApiResponse<GroupDTO>>(`${this.base}/${id}`);
  }

  deleteGroup(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.base}/${id}`);
  }

  // Members
  searchUsers(query: string): Observable<ApiResponse<UserSearchDTO[]>> {
    return this.http.get<ApiResponse<UserSearchDTO[]>>(`${this.base}/search-users?query=${encodeURIComponent(query)}`);
  }

  getMembers(groupId: number): Observable<ApiResponse<GroupMemberDTO[]>> {
    return this.http.get<ApiResponse<GroupMemberDTO[]>>(`${this.base}/${groupId}/members`);
  }

  addMember(groupId: number, userId: number): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.base}/${groupId}/members/${userId}`, {});
  }

  removeMember(groupId: number, userId: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.base}/${groupId}/members/${userId}`);
  }

  // Quizzes
  getGroupQuizzes(groupId: number): Observable<ApiResponse<GroupQuizDTO[]>> {
    return this.http.get<ApiResponse<GroupQuizDTO[]>>(`${this.base}/${groupId}/quizzes`);
  }

  assignQuiz(groupId: number, quizId: number): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.base}/${groupId}/quizzes/${quizId}`, {});
  }

  removeQuiz(groupId: number, quizId: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.base}/${groupId}/quizzes/${quizId}`);
  }

  // Submissions
  getGroupSubmissions(groupId: number): Observable<ApiResponse<GroupQuizResultDTO[]>> {
    return this.http.get<ApiResponse<GroupQuizResultDTO[]>>(`${this.base}/${groupId}/submissions`);
  }

  validateSubmission(resultId: number, status: string): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.base}/submissions/${resultId}/validate`, { status });
  }

  setRequiresValidation(groupId: number, quizId: number): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.base}/${groupId}/quizzes/${quizId}/require-validation`, {});
  }

  // For QuizTaker
  getMyGroupResults(): Observable<ApiResponse<GroupQuizResultDTO[]>> {
    return this.http.get<ApiResponse<GroupQuizResultDTO[]>>(`${this.base}/my-results`);
  }

  getMyGroupQuizzes(): Observable<ApiResponse<GroupQuizDTO[]>> {
    return this.http.get<ApiResponse<GroupQuizDTO[]>>(`${this.base}/my-quizzes`);
  }

  getCompletedGroupQuizIds(): Observable<ApiResponse<number[]>> {
    return this.http.get<ApiResponse<number[]>>(`${this.base}/completed-quiz-ids`);
  }
}
