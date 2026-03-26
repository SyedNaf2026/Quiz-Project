import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, LeaderboardDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private baseUrl = 'https://localhost:7220/api/leaderboard';

  constructor(private http: HttpClient) {}

  getLeaderboard(categoryId?: number): Observable<ApiResponse<LeaderboardDTO[]>> {
    const url = categoryId ? `${this.baseUrl}?categoryId=${categoryId}` : this.baseUrl;
    return this.http.get<ApiResponse<LeaderboardDTO[]>>(url);
  }
}
