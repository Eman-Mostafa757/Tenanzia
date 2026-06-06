import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TasksService {

private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getKanban() {
    return this.http.get<any>(`${this.apiUrl}/Tasks/kanban`, { headers: this.getHeaders() });
  }

  create(data: any) {
    return this.http.post(`${this.apiUrl}/Tasks/Create`, data, { headers: this.getHeaders() });
  }

  updateStatus(id: number, status: string) {
  return this.http.patch(`${this.apiUrl}/Tasks/${id}/status`, JSON.stringify(status), {
    headers: new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`,
      'Content-Type': 'application/json'
    }),
    responseType: 'text' // ← جديد
  });
}

delete(id: number) {
  return this.http.delete(`${this.apiUrl}/Tasks/Delete/${id}`, { 
    headers: this.getHeaders(),
    responseType: 'text' // ← جديد
  });
}
getTaskDetails(id: number) {
  return this.http.get<any>(
    `${this.apiUrl}/Tasks/${id}/details`,
    { headers: this.getHeaders() }
  );
}

addComment(taskId: number, content: string) {
  return this.http.post(
    `${this.apiUrl}/Tasks/${taskId}/comments`,
    JSON.stringify(content),
    {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`,
        'Content-Type': 'application/json'
      }),
      responseType: 'text'
    }
  );
}

deleteComment(taskId: number, commentId: number) {
  return this.http.delete(
    `${this.apiUrl}/Tasks/${taskId}/comments/${commentId}`,
    {
      headers: this.getHeaders(),
      responseType: 'text'
    }
  );
}

}