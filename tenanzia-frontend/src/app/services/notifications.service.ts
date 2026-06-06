import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {

private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAll() {
    return this.http.get<any[]>(
      `${this.apiUrl}/Notifications`,
      { headers: this.getHeaders() }
    );
  }

  getUnreadCount() {
    return this.http.get<any>(
      `${this.apiUrl}/Notifications/unread-count`,
      { headers: this.getHeaders() }
    );
  }

  markAsRead(id: number) {
    return this.http.patch(
      `${this.apiUrl}/Notifications/${id}/read`,
      {},
      { headers: this.getHeaders(), responseType: 'text' }
    );
  }

  markAllAsRead() {
    return this.http.patch(
      `${this.apiUrl}/Notifications/read-all`,
      {},
      { headers: this.getHeaders(), responseType: 'text' }
    );
  }
}