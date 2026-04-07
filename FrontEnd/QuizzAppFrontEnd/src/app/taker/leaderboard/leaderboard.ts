import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Navbar } from '../../navbar/navbar';
import { LeaderboardService } from '../../service/leaderboard.service';
import { CategoryService } from '../../service/category.service';
import { ToastService } from '../../service/toast.service';
import { CategoryDTO, LeaderboardDTO } from '../../models/models';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, FormsModule, Navbar],
  templateUrl: './leaderboard.html'
})
export class Leaderboard implements OnInit {
  entries: LeaderboardDTO[] = [];
  categories: CategoryDTO[] = [];
  loading = true;

  selectedCategoryId: number | undefined;
  selectedPeriod = 'all';  // all | today | week | month | year | custom
  customFrom = '';
  customTo = '';

  periods = [
    { value: 'all',   label: 'All Time' },
    { value: 'today', label: 'Today' },
    { value: 'week',  label: 'This Week' },
    { value: 'month', label: 'This Month' },
    { value: 'year',  label: 'This Year' },
    { value: 'custom',label: 'Custom Range' }
  ];

  constructor(
    private leaderboardService: LeaderboardService,
    private categoryService: CategoryService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.categoryService.getAllCategories().subscribe({
      next: r => { this.categories = r.data || []; this.cdr.detectChanges(); }
    });
    this.load();
  }

  getDateRange(): { from?: string; to?: string } {
    const now = new Date();
    const fmt = (d: Date) => d.toISOString().split('T')[0]; // YYYY-MM-DD

    switch (this.selectedPeriod) {
      case 'today':
        return { from: fmt(now), to: fmt(now) };
      case 'week': {
        const start = new Date(now);
        start.setDate(now.getDate() - now.getDay()); // Sunday
        return { from: fmt(start), to: fmt(now) };
      }
      case 'month':
        return { from: `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`, to: fmt(now) };
      case 'year':
        return { from: `${now.getFullYear()}-01-01`, to: fmt(now) };
      case 'custom':
        return { from: this.customFrom || undefined, to: this.customTo || undefined };
      default:
        return {};
    }
  }

  load(): void {
    this.loading = true;
    this.cdr.detectChanges();
    const { from, to } = this.getDateRange();
    this.leaderboardService.getLeaderboard(this.selectedCategoryId, from, to).subscribe({
      next: res => {
        this.entries = res.data || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
        setTimeout(() => this.toast.error('Failed to load leaderboard.'), 0);
      }
    });
  }

  onCategoryFilter(e: Event): void {
    const val = (e.target as HTMLSelectElement).value;
    this.selectedCategoryId = val ? Number(val) : undefined;
    this.load();
  }

  onPeriodChange(): void {
    if (this.selectedPeriod !== 'custom') this.load();
    this.cdr.detectChanges();
  }

  applyCustomRange(): void {
    if (!this.customFrom || !this.customTo) {
      this.toast.error('Please select both From and To dates.');
      return;
    }
    this.load();
  }
}
