import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CustomersService {
  private apiUrl = 'https://localhost:44302/api';


  constructor(private http:HttpClient , private authService: AuthService) { }

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }
getAll(search?: string, status?: string) {
    let url = `${this.apiUrl}/Customers/GetAll?`;
    if (search) url += `search=${search}&`;
    if (status) url += `status=${status}`;
    return this.http.get<any[]>(url, { headers: this.getHeaders() });
  }
 create(data: any) {
    return this.http.post(`${this.apiUrl}/Customers/Create`, data, { headers: this.getHeaders() });
  }

 update(id: number, data: any) {
  return this.http.put(`${this.apiUrl}/Customers/Update/${id}`, data, { 
    headers: this.getHeaders(),
    responseType: 'text' // ← جديد
  });
}

delete(id: number) {
  return this.http.delete(`${this.apiUrl}/Customers/Delete/${id}`, { 
    headers: this.getHeaders(),
    responseType: 'text' // ← جديد
  });
}
 getProfile(id: number) {
  return this.http.get<any>(
    `${this.apiUrl}/Customers/GetProfile/${id}`,
    { headers: this.getHeaders() }
  );
}
GetLimits()
{
  return this.http.get<any>(`${this.apiUrl}/Subscriptions/limits`,{headers: this.getHeaders()});
}
}
