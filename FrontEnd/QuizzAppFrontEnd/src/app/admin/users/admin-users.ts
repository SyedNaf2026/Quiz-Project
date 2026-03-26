import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Navbar } from '../../navbar/navbar';
import { LeaderboardService } from '../../service/leaderboard.service';
import { ToastService } from '../../service/toast.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, Navbar],
  templateUrl: './admin-users.html'
})
export class AdminUsers implements OnInit {
  entries: any[] = [];
  loading = true;

  constructor(
    private leaderboard: LeaderboardService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.leaderboard.getLeaderboard().subscribe({
      next: (res) => {
        this.entries = res.data || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
        setTimeout(() => this.toast.error('Failed to load data.'), 0);
      }
    });
  }
}
