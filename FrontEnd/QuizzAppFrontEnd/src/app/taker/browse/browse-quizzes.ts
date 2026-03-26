import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Navbar } from '../../navbar/navbar';
import { QuizService } from '../../service/quiz.service';
import { CategoryService } from '../../service/category.service';
import { ToastService } from '../../service/toast.service';
import { CategoryDTO, QuizDTO } from '../../models/models';

@Component({
  selector: 'app-browse-quizzes',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar],
  templateUrl: './browse-quizzes.html'
})
export class BrowseQuizzes implements OnInit {
  allQuizzes: QuizDTO[] = [];
  filtered: QuizDTO[] = [];
  paged: QuizDTO[] = [];
  categories: CategoryDTO[] = [];
  availableDifficulties: string[] = [];

  loading = true;
  search = '';
  selectedCategoryId: number | null = null;
  selectedDifficulty = '';
  page = 1;
  pageSize = 6;
  totalPages = 1;
  pages: number[] = [];

  get pageStart() { return (this.page - 1) * this.pageSize + 1; }
  get pageEnd()   { return Math.min(this.page * this.pageSize, this.filtered.length); }

  constructor(
    private quizService: QuizService,
    private categoryService: CategoryService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (r) => { this.categories = r.data || []; this.cdr.detectChanges(); }
    });
    this.loading = true;
    this.quizService.getActiveQuizzes().subscribe({
      next: (res) => {
        this.allQuizzes = res.data || [];
        this.applyFilter();
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

  applyFilter(): void {
    const q = this.search.toLowerCase();

    // Filter by category name
    const catName = this.selectedCategoryId
      ? (this.categories.find(c => c.id === this.selectedCategoryId)?.name ?? '')
      : '';
    let result = catName
      ? this.allQuizzes.filter(x => x.categoryName === catName)
      : [...this.allQuizzes];

    // Build difficulty options from current category result
    const found = result.map(x => x.difficulty ?? '').filter(d => d !== '');
    const unique = [...new Set(found)];
    const order = ['Easy', 'Medium', 'Hard'];
    this.availableDifficulties = order.filter(d => unique.includes(d));

    // Reset difficulty if no longer valid
    if (this.selectedDifficulty && !this.availableDifficulties.includes(this.selectedDifficulty)) {
      this.selectedDifficulty = '';
    }

    // Filter by difficulty
    if (this.selectedDifficulty) {
      result = result.filter(x => (x.difficulty ?? '') === this.selectedDifficulty);
    }

    // Filter by search text
    if (q) {
      result = result.filter(x =>
        x.title.toLowerCase().includes(q) ||
        (x.description ?? '').toLowerCase().includes(q)
      );
    }

    this.filtered = result;
    this.page = 1;
    this.updatePage();
  }

  updatePage(): void {
    this.totalPages = Math.max(1, Math.ceil(this.filtered.length / this.pageSize));
    this.pages = Array.from({ length: this.totalPages }, (_, i) => i + 1);
    const start = (this.page - 1) * this.pageSize;
    this.paged = this.filtered.slice(start, start + this.pageSize);
    this.cdr.detectChanges();
  }

  onSearch(e: Event): void {
    this.search = (e.target as HTMLInputElement).value;
    this.applyFilter();
  }

  onCategoryFilter(e: Event): void {
    const val = (e.target as HTMLSelectElement).value;
    this.selectedCategoryId = val ? Number(val) : null;
    this.selectedDifficulty = '';
    this.applyFilter();
  }

  onDifficultyFilter(e: Event): void {
    this.selectedDifficulty = (e.target as HTMLSelectElement).value;
    this.applyFilter();
  }

  goToPage(p: number): void {
    this.page = p;
    this.updatePage();
  }
}
