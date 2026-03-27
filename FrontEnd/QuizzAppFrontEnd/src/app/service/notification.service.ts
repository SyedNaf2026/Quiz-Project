import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { ApiResponse, NotificationDTO } from '../models/models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private baseUrl = 'https://localhost:7220/api/notification';
  private hubUrl = 'https://localhost:7220/hubs/notifications';
  private hubConnection?: signalR.HubConnection;

  // Reactive list of notifications — navbar subscribes to this
  notifications$ = new BehaviorSubject<NotificationDTO[]>([]);

  constructor(private http: HttpClient, private auth: AuthService) {}

  // Call this once after login
  connect(userId: number): void {
    if (this.hubConnection) return; // already connected

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: NotificationDTO) => {
      const current = this.notifications$.value;
      this.notifications$.next([notification, ...current]);
    });

    this.hubConnection.start()
      .then(() => this.hubConnection!.invoke('JoinUserGroup', userId.toString()))
      .catch((err: unknown) => console.error('SignalR connection error:', err));

    // Load existing notifications from REST
    this.loadNotifications();
  }

  disconnect(): void {
    this.hubConnection?.stop();
    this.hubConnection = undefined;
    this.notifications$.next([]);
  }

  loadNotifications(): void {
    this.http.get<ApiResponse<NotificationDTO[]>>(this.baseUrl).subscribe({
      next: res => {
        if (res.success) this.notifications$.next(res.data);
      }
    });
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
