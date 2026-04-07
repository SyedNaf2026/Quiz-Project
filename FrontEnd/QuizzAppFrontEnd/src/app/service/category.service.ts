import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, CategoryDTO, CreateCategoryDTO } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private baseUrl = 'https://localhost:7220/api/category';

  constructor(private http: HttpClient) {}

  getAllCategories(): Observable<ApiResponse<CategoryDTO[]>> {
    return this.http.get<ApiResponse<CategoryDTO[]>>(this.baseUrl);
  }

  createCategory(data: CreateCategoryDTO): Observable<ApiResponse<CategoryDTO>> {
    return this.http.post<ApiResponse<CategoryDTO>>(this.baseUrl, data);
  }

  updateCategory(id: number, data: CreateCategoryDTO): Observable<ApiResponse<CategoryDTO>> {
    return this.http.put<ApiResponse<CategoryDTO>>(`${this.baseUrl}/${id}`, data);
  }

  deleteCategory(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.baseUrl}/${id}`);
  }
}
