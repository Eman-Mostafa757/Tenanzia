import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProductsService {

private apiUrl = environment.apiUrl;

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

  updateStock(id: number, quantity: number) {
  return this.http.patch(
    `${this.apiUrl}/Products/${id}/stock`,
    quantity,
    {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`,
        'Content-Type': 'application/json'
      }),
      responseType: 'text'
    }
  );
}

getLowStock() {
  return this.http.get<any[]>(
    `${this.apiUrl}/Products/low-stock`,
    { headers: this.getHeaders() }
  );
}
getById(id:Number){
  return this.http.get(
    `${this.apiUrl}/Products/${id}`,
    {
      headers: this.getHeaders(),
      responseType: 'text'
    }
  )
}
}