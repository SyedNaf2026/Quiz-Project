import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Navbar } from '../../navbar/navbar';
import { CategoryService } from '../../service/category.service';
import { ToastService } from '../../service/toast.service';
import { CategoryDTO } from '../../models/models';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Navbar],
  templateUrl: './admin-categories.html'
})
export class AdminCategories implements OnInit {
  categories: CategoryDTO[] = [];
  filtered: CategoryDTO[] = [];
  search = '';
  showForm = false;
  loading = true;
  form: FormGroup;

  constructor(
    private categoryService: CategoryService,
    private toast: ToastService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.categoryService.getAllCategories().subscribe({
      next: (res) => {
        this.categories = res.data || [];
        this.applyFilter();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
        setTimeout(() => this.toast.error('Failed to load categories.'), 0);
      }
    });
  }

  applyFilter(): void {
    const q = this.search.toLowerCase();
    this.filtered = q ? this.categories.filter(c => c.name.toLowerCase().includes(q)) : [...this.categories];
  }

  onSearch(e: Event): void {
    this.search = (e.target as HTMLInputElement).value;
    this.applyFilter();
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.categoryService.createCategory(this.form.value).subscribe({
      next: (res) => {
        if (res.success) {
          this.toast.success('Category created!');
          this.showForm = false;
          this.form.reset();
          this.load();
        } else {
          this.toast.error(res.message);
        }
      },
      error: () => this.toast.error('Failed to create category.')
    });
  }
}
