import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Navbar } from '../../navbar/navbar';
import { LeaderboardService } from '../../service/leaderboard.service';
import { CategoryService } from '../../service/category.service';
import { ToastService } from '../../service/toast.service';
import { CategoryDTO, LeaderboardDTO } from '../../models/models';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, Navbar],
  templateUrl: './leaderboard.html'
})
export class Leaderboard implements OnInit {
  entries: LeaderboardDTO[] = [];
  categories: CategoryDTO[] = [];
  loading = true;

  constructor(
    private leaderboardService: LeaderboardService,
    private categoryService: CategoryService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (r) => {
        this.categories = r.data || [];
        this.cdr.detectChanges();
      }
    });
    this.load();
  }

  load(catId?: number): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.leaderboardService.getLeaderboard(catId).subscribe({
      next: (res) => {
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

  onFilter(e: Event): void {
    const val = (e.target as HTMLSelectElement).value;
    this.load(val ? Number(val) : undefined);
  }
}
