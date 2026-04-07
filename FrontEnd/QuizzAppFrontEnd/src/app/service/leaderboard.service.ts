import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, LeaderboardDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private baseUrl = 'https://localhost:7220/api/leaderboard';

  constructor(private http: HttpClient) {}

  getLeaderboard(categoryId?: number, fromDate?: string, toDate?: string): Observable<ApiResponse<LeaderboardDTO[]>> {
    const params: string[] = [];
    if (categoryId) params.push(`categoryId=${categoryId}`);
    if (fromDate) params.push(`fromDate=${fromDate}`);
    if (toDate) params.push(`toDate=${toDate}`);
    const url = params.length ? `${this.baseUrl}?${params.join('&')}` : this.baseUrl;
    return this.http.get<ApiResponse<LeaderboardDTO[]>>(url);
  }
}
