import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {

private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getMe() {
    return this.http.get<any>(`${this.apiUrl}/Auth/me`, {
      headers: this.getHeaders()
    });
  }

  updateMe(data: any) {
    return this.http.put(`${this.apiUrl}/Auth/me`, data, {
      headers: this.getHeaders(),
      responseType: 'text'
    });
  }

  updateCompany(name: string) {
    return this.http.put(`${this.apiUrl}/Auth/company`,
      JSON.stringify(name),
      {
        headers: new HttpHeaders({
          'Authorization': `Bearer ${this.authService.getToken()}`,
          'Content-Type': 'application/json'
        }),
        responseType: 'text'
      }
    );
  }
}