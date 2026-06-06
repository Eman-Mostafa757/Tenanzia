import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class InvoicesService {

private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAll(status?: string) {
    let url = `${this.apiUrl}/Invoices`;
    if (status) url += `?status=${status}`;
    return this.http.get<any[]>(url, { headers: this.getHeaders() });
  }

  markAsPaid(id: number) {
    return this.http.patch(
      `${this.apiUrl}/Invoices/${id}/pay`,
      {},
      { headers: this.getHeaders(), responseType: 'text' }
    );
  }

  cancel(id: number) {
    return this.http.patch(
      `${this.apiUrl}/Invoices/${id}/cancel`,
      {},
      { headers: this.getHeaders(), responseType: 'text' }
    );
  }

  delete(id: number) {
    return this.http.delete(
      `${this.apiUrl}/Invoices/${id}`,
      { headers: this.getHeaders(), responseType: 'text' }
    );
  }
  getById(id: number) {
  return this.http.get<any>(
    `${this.apiUrl}/Invoices/${id}`,
    { headers: this.getHeaders() }
  );
}
sendInvoice(id: number) {
  return this.http.post(
    `${this.apiUrl}/Invoices/${id}/send`,
    {},
    {
      headers: this.getHeaders(),
      responseType: 'text'
    }
  );
}
}