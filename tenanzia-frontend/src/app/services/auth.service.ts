import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'https://localhost:44302/api';

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/login`, { email, password }).pipe(
      tap((res: any) => {
        localStorage.setItem('token', res.token);
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }
  getUsername(): string {
  const token = this.getToken();
  if (!token) return '';
  
  const payload = JSON.parse(atob(token.split('.')[1]));
  return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '';
}

getRole(): string {
  const token = this.getToken();
  if (!token) return '';
  const payload = JSON.parse(atob(token.split('.')[1]));
  return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
}

isOwner(): boolean { return this.getRole() === 'Owner'; }
isManager(): boolean { return this.getRole() === 'Manager'; }
isEmployee(): boolean { return this.getRole() === 'Employee'; }
isManagerOrOwner(): boolean { return this.isOwner() || this.isManager(); }

register(data: any): Observable<any> {
  return this.http.post(`${this.apiUrl}/Auth/Register`, data, {
    responseType: 'text'
  });
}

}