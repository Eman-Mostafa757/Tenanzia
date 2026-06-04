import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class SubscriptionService {

  private apiUrl = 'https://localhost:44302/api';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getPlans() {
    return this.http.get<any[]>(`${this.apiUrl}/Subscriptions/plans`);
  }

  getCurrent() {
    return this.http.get<any>(`${this.apiUrl}/Subscriptions/current`, {
      headers: this.getHeaders()
    });
  }

  checkout(planName: string) {
    return this.http.post<any>(`${this.apiUrl}/Subscriptions/checkout`,
      { planName },
      { headers: this.getHeaders() }
    );
  }

  downgrade() {
    return this.http.post(`${this.apiUrl}/Subscriptions/downgrade`, {}, {
      headers: this.getHeaders(),
      responseType: 'text'
    });
  }
}