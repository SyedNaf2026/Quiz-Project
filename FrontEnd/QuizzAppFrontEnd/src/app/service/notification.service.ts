import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { ApiResponse, NotificationDTO } from '../models/models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private baseUrl = 'https://localhost:7220/api/notification';
  private hubUrl = 'https://localhost:7220/hubs/notifications';
  private hubConnection?: signalR.HubConnection;

  notifications$ = new BehaviorSubject<NotificationDTO[]>([]);

  constructor(
    private http: HttpClient,
    private auth: AuthService,
    private router: Router
  ) {}

  connect(userId: number): void {
    if (this.hubConnection) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, { accessTokenFactory: () => this.auth.getToken() ?? '' })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: NotificationDTO) => {
      const current = this.notifications$.value;
      this.notifications$.next([notification, ...current]);
      // Show real-time notification as a clickable popup
      this.showPopup(notification);
    });

    this.hubConnection.start()
      .then(() => this.hubConnection!.invoke('JoinUserGroup', userId.toString()))
      .then(() => this.loadNotifications(true))
      .catch((err: unknown) => console.error('SignalR connection error:', err));
  }

  disconnect(): void {
    this.hubConnection?.stop();
    this.hubConnection = undefined;
    this.notifications$.next([]);
  }

  loadNotifications(showUnreadPopups = false): void {
    this.http.get<ApiResponse<NotificationDTO[]>>(this.baseUrl).subscribe({
      next: res => {
        if (res.success) {
          this.notifications$.next(res.data);
          // On login, show unread notifications as popups
          if (showUnreadPopups) {
            const unread = res.data.filter(n => !n.isRead).slice(0, 3); // max 3 popups
            unread.forEach((n, i) => setTimeout(() => this.showPopup(n), i * 600));
          }
        }
      }
    });
  }

  private showPopup(n: NotificationDTO): void {
    const icon = this.getIcon(n.type);
    const route = this.getRoute(n.type);
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
    const bg = isDark ? '#1e1e2e' : '#ffffff';
    const textColor = isDark ? '#e2e8f0' : '#1a1a2e';
    const mutedColor = isDark ? '#94a3b8' : '#64748b';

    // Stack popups by counting existing ones
    const existing = document.querySelectorAll('.notif-popup').length;
    const topOffset = 1 + existing * 5.5;

    const el = document.createElement('div');
    el.className = 'notif-popup';
    el.style.cssText = `
      position:fixed;top:${topOffset}rem;right:1rem;z-index:99999;
      background:${bg};color:${textColor};
      padding:0.875rem 1.25rem;border-radius:10px;
      font-size:0.875rem;font-weight:500;
      box-shadow:0 4px 20px rgba(0,0,0,0.2);
      display:flex;align-items:flex-start;gap:0.6rem;
      min-width:280px;max-width:340px;cursor:pointer;
      border-left:4px solid #4f46e5;
      font-family:Inter,sans-serif;
    `;
    el.innerHTML = `
      <span style="font-size:1.2rem;flex-shrink:0">${icon}</span>
      <div style="flex:1">
        <p style="margin:0 0 2px;line-height:1.4;color:${textColor}">${n.message}</p>
        ${route ? `<span style="font-size:0.75rem;color:#4f46e5">Click to view →</span>` : ''}
      </div>
      <button style="background:none;border:none;cursor:pointer;color:${mutedColor};font-size:1rem;padding:0;flex-shrink:0">✕</button>
    `;
    const closeBtn = el.querySelector('button')!;
    closeBtn.onclick = (e) => { e.stopPropagation(); el.remove(); };
    el.onclick = () => {
      el.remove();
      if (route) this.router.navigate([route]);
    };
    document.body.appendChild(el);
    setTimeout(() => { if (el.parentNode) el.remove(); }, 6000);
  }

  private getIcon(type: string): string {
    const map: Record<string, string> = {
      quiz_added: '🆕', quiz_deactivated: '🚫', quiz_updated: '✏️',
      rank_lost: '📉', leaderboard_update: '🏆', group_quiz_assigned: '📝'
    };
    return map[type] ?? '🔔';
  }

  private getRoute(type: string): string {
    const role = this.auth.getRole();
    if (type === 'group_quiz_assigned') return '/taker/browse';
    if (type === 'quiz_added' || type === 'quiz_updated' || type === 'quiz_deactivated') return '/taker/browse';
    if (type === 'leaderboard_update' || type === 'rank_lost') return '/taker/leaderboard';
    return '';
  }

  markRead(id: number): void {
    this.http.put<ApiResponse<string>>(`${this.baseUrl}/${id}/read`, {}).subscribe({
      next: () => {
        const updated = this.notifications$.value.map(n =>
          n.id === id ? { ...n, isRead: true } : n
        );
        this.notifications$.next(updated);
      }
    });
  }

  markAllRead(): void {
    this.http.put<ApiResponse<string>>(`${this.baseUrl}/read-all`, {}).subscribe({
      next: () => {
        const updated = this.notifications$.value.map(n => ({ ...n, isRead: true }));
        this.notifications$.next(updated);
      }
    });
  }

  get unreadCount(): number {
    return this.notifications$.value.filter(n => !n.isRead).length;
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
