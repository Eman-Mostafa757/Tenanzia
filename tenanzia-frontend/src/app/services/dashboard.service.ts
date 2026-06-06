import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getDashboard() {
    return this.http.get(`${this.apiUrl}/Dashboard`, {
      headers: this.getHeaders()
    });
  }

  getRevenueChart() {
  return this.http.get<any[]>(`${this.apiUrl}/Dashboard/revenue-chart`, {
    headers: this.getHeaders()
  });
}
}