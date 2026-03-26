import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Navbar } from '../../navbar/navbar';
import { QuizService } from '../../service/quiz.service';
import { CategoryService } from '../../service/category.service';
import { LeaderboardService } from '../../service/leaderboard.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar],
  templateUrl: './admin-dashboard.html'
})
export class AdminDashboard implements OnInit {
  totalQuizzes = 0;
  totalCategories = 0;
  topScores: any[] = [];
  loading = true;

  constructor(
    private quizService: QuizService,
    private categoryService: CategoryService,
    private leaderboardService: LeaderboardService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.quizService.getActiveQuizzes().subscribe({
      next: (res) => {
        this.totalQuizzes = res.data?.length || 0;
        this.cdr.detectChanges();
      }
    });
    this.categoryService.getAllCategories().subscribe({
      next: (res) => {
        this.totalCategories = res.data?.length || 0;
        this.cdr.detectChanges();
      }
    });
    this.leaderboardService.getLeaderboard().subscribe({
      next: (res) => {
        this.topScores = res.data?.slice(0, 5) || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
