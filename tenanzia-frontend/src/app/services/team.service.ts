import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class TeamService {

  private apiUrl = 'https://localhost:44302/api';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getTeamMembers() {
    return this.http.get<any[]>(`${this.apiUrl}/Auth/users`, {
      headers: this.getHeaders()
    });
  }

  inviteEmployee(data: any) {
    return this.http.post(`${this.apiUrl}/Auth/invite`, data, {
      headers: this.getHeaders(),
      responseType: 'text'
    });
  }

getUserProfile(userId: number) {
  return this.http.get<any>(
    `${this.apiUrl}/Auth/users/${userId}/profile`,
    { headers: this.getHeaders() }
  );
}
  getTeamWorkload() {
  return this.http.get<any[]>(
    `${this.apiUrl}/Auth/users/workload`,
    { headers: this.getHeaders() }
  );
}
  getUnassignedTasks() {
  return this.http.get<any[]>(
    `${this.apiUrl}/Tasks/GetAll`,
    { headers: this.getHeaders() }
  );
}

assignTask(userId: number, taskId: number) {
  return this.http.post(
    `${this.apiUrl}/Auth/users/${userId}/assign-task/${taskId}`,
    {},
    {
      headers: this.getHeaders(),
      responseType: 'text'
    }
  );
}


updateUser(userId: number, data: any) {
  return this.http.put(
    `${this.apiUrl}/Auth/users/${userId}`,
    data,
    {
      headers: this.getHeaders(),
      responseType: 'text'
    }
  );
}
updateRole(userId: number, role: string) {
  return this.http.put(
    `${this.apiUrl}/Auth/users/${userId}/role`,
    JSON.stringify(role),
    {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`,
        'Content-Type': 'application/json'
      }),
      responseType: 'text'
    }
  );
}

removeUser(userId: number) {
  return this.http.delete(
    `${this.apiUrl}/Auth/users/${userId}`,
    {
      headers: this.getHeaders(),
      responseType: 'text'
    }
  );
}

}