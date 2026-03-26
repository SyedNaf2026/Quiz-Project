import { Component, OnInit } from '@angular/core';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../service/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar implements OnInit {
  role = '';
  userName = '';
  isDark = false;
  menuOpen = false;

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.loadUserInfo();
    this.isDark = localStorage.getItem('theme') === 'dark';
    this.applyTheme();

    // Re-read role/name on every navigation (handles post-login state)
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe(() => this.loadUserInfo());
  }

  loadUserInfo(): void {
    this.role = this.auth.getRole() || '';
    this.userName = this.auth.getUserName() || '';
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
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  isAdmin() { return this.role === 'Admin'; }
  isCreator() { return this.role === 'QuizCreator'; }
  isTaker() { return this.role === 'QuizTaker'; }
}
