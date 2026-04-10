import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { Navbar } from '../../navbar/navbar';
import { QuizService } from '../../service/quiz.service';
import { CategoryService } from '../../service/category.service';
import { ToastService } from '../../service/toast.service';
import { CategoryDTO } from '../../models/models';

@Component({
  selector: 'app-create-quiz',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, Navbar],
  templateUrl: './create-quiz.html'
})
export class CreateQuiz implements OnInit {
  form: FormGroup;
  categories: CategoryDTO[] = [];
  loading = false;
  isEdit = false;
  quizId: number | null = null;

  // Inline new category
  showNewCategory = false;
  newCategoryName = '';
  newCategoryDesc = '';
  savingCategory = false;

  constructor(
    private fb: FormBuilder,
    private quizService: QuizService,
    private categoryService: CategoryService,
    private toast: ToastService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      categoryId: ['', Validators.required],
      timeLimit: [null],
      isActive: [true],
      difficulty: ['Medium']
    });
  }

  ngOnInit(): void {
    this.quizId = Number(this.route.snapshot.paramMap.get('id')) || null;

    if (this.quizId) {
      this.isEdit = true;
      forkJoin({
        cats: this.categoryService.getAllCategories(),
        quiz: this.quizService.getQuizById(this.quizId)
      }).subscribe({
        next: ({ cats, quiz }) => {
          this.categories = cats.data || [];
          if (quiz.data) {
            const q = quiz.data;
            const cat = this.categories.find(c => c.name === q.categoryName);
            this.form.patchValue({
              title: q.title,
              description: q.description,
              categoryId: cat?.id || '',
              timeLimit: q.timeLimit,
              isActive: q.isActive,
              difficulty: q.difficulty || 'Medium'
            });
          }
          this.cdr.detectChanges();
        },
        error: () => this.toast.error('Failed to load quiz data.')
      });
    } else {
      this.categoryService.getAllCategories().subscribe({
        next: (res) => {
          this.categories = res.data || [];
          this.cdr.detectChanges();
        }
      });
    }
  }

  addCategory(): void {
    if (!this.newCategoryName.trim()) {
      setTimeout(() => this.toast.error('Category name is required.'), 0);
      return;
    }
    this.savingCategory = true;
    this.cdr.detectChanges();
    this.categoryService.createCategory({ name: this.newCategoryName.trim(), description: this.newCategoryDesc.trim() }).subscribe({
      next: (res) => {
        this.savingCategory = false;
        if (res.success && res.data) {
          this.categories = [res.data, ...this.categories];
          this.form.patchValue({ categoryId: res.data.id });
          this.showNewCategory = false;
          this.newCategoryName = '';
          this.newCategoryDesc = '';
          setTimeout(() => this.toast.success('Category added!'), 0);
        } else {
          setTimeout(() => this.toast.error(res.message || 'Failed to create category.'), 0);
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.savingCategory = false;
        const msg = err?.error?.message || 'Failed to create category.';
        setTimeout(() => this.toast.error(msg), 0);
        this.cdr.detectChanges();
      }
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    const val = this.form.value;
    const payload = { ...val, categoryId: Number(val.categoryId), timeLimit: val.timeLimit ? Number(val.timeLimit) : null };

    if (this.isEdit && this.quizId) {
      this.quizService.updateQuiz(this.quizId, payload).subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) { this.toast.success('Quiz updated!'); this.router.navigate(['/creator/my-quizzes']); }
          else this.toast.error(res.message);
        },
        error: () => { this.loading = false; this.toast.error('Update failed.'); }
      });
    } else {
      this.quizService.createQuiz(payload).subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) { this.toast.success('Quiz created!'); this.router.navigate(['/creator/my-quizzes']); }
          else this.toast.error(res.message);
        },
        error: () => { this.loading = false; this.toast.error('Create failed.'); }
      });
    }
  }
}
