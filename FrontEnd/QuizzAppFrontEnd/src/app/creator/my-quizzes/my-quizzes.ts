import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Navbar } from '../../navbar/navbar';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog';
import { QuizService } from '../../service/quiz.service';
import { ToastService } from '../../service/toast.service';
import { QuizDTO } from '../../models/models';

@Component({
  selector: 'app-my-quizzes',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar, ConfirmDialogComponent],
  templateUrl: './my-quizzes.html'
})
export class MyQuizzes implements OnInit {
  quizzes: QuizDTO[] = [];
  loading = true;
  showConfirm = false;
  selectedQuiz: QuizDTO | null = null;

  constructor(
    private quizService: QuizService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.quizService.getMyQuizzes().subscribe({
      next: (res) => {
        this.quizzes = res.data || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
        setTimeout(() => this.toast.error('Failed to load quizzes.'), 0);
      }
    });
  }

  toggle(quiz: QuizDTO): void {
    if (quiz.isActive && !confirm(`Deactivate "${quiz.title}"? QuizTakers will no longer see this quiz.`)) return;
    this.quizService.toggleStatus(quiz.id).subscribe({
      next: () => { setTimeout(() => this.toast.success('Status updated.'), 0); this.load(); },
      error: () => setTimeout(() => this.toast.error('Failed to toggle status.'), 0)
    });
  }

  confirmDelete(quiz: QuizDTO): void {
    this.selectedQuiz = quiz;
    this.showConfirm = true;
  }

  doDelete(): void {
    if (!this.selectedQuiz) return;
    this.quizService.deleteQuiz(this.selectedQuiz.id).subscribe({
      next: () => {
        setTimeout(() => this.toast.success('Quiz deleted.'), 0);
        this.showConfirm = false;
        this.load();
      },
      error: () => setTimeout(() => this.toast.error('Failed to delete quiz.'), 0)
    });
  }
}
