import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ProductsService {

  private apiUrl = 'https://localhost:44302/api';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAll() {
    return this.http.get<any[]>(`${this.apiUrl}/Products`, {
      headers: this.getHeaders()
    });
  }

  create(data: any) {
    return this.http.post(`${this.apiUrl}/Products`, data, {
      headers: this.getHeaders()
    });
  }

  update(id: number, data: any) {
    return this.http.put(`${this.apiUrl}/Products/${id}`, data, {
      headers: this.getHeaders(),
      responseType: 'text'
    });
  }

  delete(id: number) {
    return this.http.delete(`${this.apiUrl}/Products/${id}`, {
      headers: this.getHeaders(),
      responseType: 'text'
    });
  }
}