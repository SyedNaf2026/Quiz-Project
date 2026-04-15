import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Navbar } from '../../navbar/navbar';
import { CategoryService } from '../../service/category.service';
import { UserService } from '../../service/user.service';
import { ToastService } from '../../service/toast.service';
import { CategoryDTO } from '../../models/models';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, Navbar],
  templateUrl: './admin-categories.html'
})
export class AdminCategories implements OnInit {
  categories: CategoryDTO[] = [];
  filtered: CategoryDTO[] = [];
  search = '';
  showForm = false;
  loading = true;
  form: FormGroup;

  // Inline edit
  editingId: number | null = null;
  editName = '';
  editDescription = '';
  editSaving = false;
  currentUserId = 0;

  constructor(
    private categoryService: CategoryService,
    private userService: UserService,
    private toast: ToastService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit(): void {
    this.userService.getProfile().subscribe({
      next: res => { this.currentUserId = res.data?.id ?? 0; }
    });
    this.load();
  }

  isOwner(cat: CategoryDTO): boolean {
    // null/undefined createdBy = legacy category — allow any creator to edit
    if (cat.createdBy == null) return true;
    return cat.createdBy === this.currentUserId;
  }

  load(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.categoryService.getAllCategories().subscribe({
      next: res => {
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
      next: res => {
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

  startEdit(cat: CategoryDTO): void {
    this.editingId = cat.id;
    this.editName = cat.name;
    this.editDescription = cat.description;
    this.cdr.detectChanges();
  }

  cancelEdit(): void {
    this.editingId = null;
    this.cdr.detectChanges();
  }

  saveEdit(cat: CategoryDTO): void {
    if (!this.editName.trim()) { this.toast.error('Name is required.'); return; }
    this.editSaving = true;
    this.categoryService.updateCategory(cat.id, { name: this.editName.trim(), description: this.editDescription.trim() }).subscribe({
      next: res => {
        this.editSaving = false;
        if (res.success) {
          this.toast.success('Category updated!');
          this.editingId = null;
          this.load();
        } else {
          this.toast.error(res.message);
        }
        this.cdr.detectChanges();
      },
      error: err => {
        this.editSaving = false;
        const msg = err?.error?.message || 'You can only edit your own categories.';
        this.toast.error(msg);
        this.cdr.detectChanges();
      }
    });
  }

  deleteCategory(cat: CategoryDTO): void {
    if (!confirm(`Delete "${cat.name}"? This cannot be undone.`)) return;
    this.categoryService.deleteCategory(cat.id).subscribe({
      next: res => {
        if (res.success) { this.toast.success('Category deleted.'); this.load(); }
        else this.toast.error(res.message);
      },
      error: (err) => {
        const msg = err?.error?.message || 'You can only delete your own categories.';
        this.toast.error(msg);
      }
    });
  }
}
