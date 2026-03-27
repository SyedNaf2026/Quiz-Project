import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../service/auth.service';
import { NotificationService } from '../service/notification.service';
import { UserService } from '../service/user.service';
import { NotificationDTO } from '../models/models';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar implements OnInit, OnDestroy {
  role = '';
  userName = '';
  isDark = false;
  menuOpen = false;
  notifOpen = false;
  notifications: NotificationDTO[] = [];
  private notifSub?: Subscription;
  private connected = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    public notifService: NotificationService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.loadUserInfo();
    this.isDark = localStorage.getItem('theme') === 'dark';
    this.applyTheme();

    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe(() => this.loadUserInfo());

    // Subscribe to live notification updates
    this.notifSub = this.notifService.notifications$.subscribe(n => {
      this.notifications = n;
    });
  }

  loadUserInfo(): void {
    this.role = this.auth.getRole() || '';
    this.userName = this.auth.getUserName() || '';

    // Connect SignalR once after login
    if (this.auth.isLoggedIn() && !this.connected) {
      this.connected = true;
      this.userService.getProfile().subscribe({
        next: res => {
          if (res.success) this.notifService.connect(res.data.id);
        }
      });
    }

    // Disconnect on logout
    if (!this.auth.isLoggedIn() && this.connected) {
      this.connected = false;
      this.notifService.disconnect();
    }
  }

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  toggleNotif(): void {
    this.notifOpen = !this.notifOpen;
  }

  markRead(n: NotificationDTO): void {
    if (!n.isRead) this.notifService.markRead(n.id);
  }

  markAllRead(): void {
    this.notifService.markAllRead();
  }

  // Close dropdown when clicking outside
  @HostListener('document:click', ['$event'])
  onDocClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.notif-wrapper')) this.notifOpen = false;
  }

  get homeRoute(): string {
    if (this.role === 'Admin') return '/admin/dashboard';
    return '/home';
  }

  toggleTheme(): void {
    this.isDark = !this.isDark;
    localStorage.setItem('theme', this.isDark ? 'dark' : 'light');
    this.applyTheme();
  }

  applyTheme(): void {
    document.documentElement.setAttribute('data-theme', this.isDark ? 'dark' : 'light');
  }

  logout(): void {
    this.notifService.disconnect();
    this.connected = false;
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  isAdmin() { return this.role === 'Admin'; }
  isCreator() { return this.role === 'QuizCreator'; }
  isTaker() { return this.role === 'QuizTaker'; }

  ngOnDestroy(): void {
    this.notifSub?.unsubscribe();
  }
}
