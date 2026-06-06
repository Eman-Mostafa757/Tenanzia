import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrdersService {

private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAll(status?: string) {
    let url = `${this.apiUrl}/Orders`;
    if (status) url += `?status=${status}`;
    return this.http.get<any[]>(url, { headers: this.getHeaders() });
  }

  getStats() {
    return this.http.get<any>(
      `${this.apiUrl}/Orders/stats`,
      { headers: this.getHeaders() }
    );
  }

  create(data: any) {
    return this.http.post(
      `${this.apiUrl}/Orders`,
      data,
      { headers: this.getHeaders() }
    );
  }

  updateStatus(id: number, status: string) {
    return this.http.patch(
      `${this.apiUrl}/Orders/${id}/status`,
      JSON.stringify(status),
      {
        headers: new HttpHeaders({
          'Authorization': `Bearer ${this.authService.getToken()}`,
          'Content-Type': 'application/json'
        }),
        
      }
    );
  }

  delete(id: number) {
    return this.http.delete(
      `${this.apiUrl}/Orders/${id}`,
      {
        headers: this.getHeaders(),
        responseType: 'text'
      }
    );
  }
  getById(id:number)
  {
    return this.http.get(
      `${this.apiUrl}/Orders/${id}`,
      {
        headers: this.getHeaders(),
        
      }
    );
  }
}