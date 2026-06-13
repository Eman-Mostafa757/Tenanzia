import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-[#0D0D0F] flex items-center justify-center">
      <div class="bg-[#111114] border border-[#1E1E24] rounded-2xl p-8 w-full max-w-md">
        <div class="text-center mb-6">
          <div class="w-12 h-12 bg-[#D4537E] rounded-xl flex items-center justify-center mx-auto mb-4">
            <span class="text-white font-medium text-lg">T</span>
          </div>
          <h1 class="text-xl font-medium text-[#F0F0F2]">Reset Password</h1>
        </div>

        <div *ngIf="success" class="bg-[#12251E] border border-[#5DCAA5] text-[#5DCAA5] text-sm px-4 py-3 rounded-xl mb-4">
          ✅ Password reset successfully!
          <button (click)="router.navigate(['/login'])" class="underline ml-2">Login</button>
        </div>
        <div *ngIf="error" class="bg-[#3C1528] border border-[#D4537E] text-[#ED93B1] text-sm px-4 py-3 rounded-xl mb-4">
          {{ error }}
        </div>

        <div *ngIf="!success" class="space-y-4">
          <div>
            <label class="block text-xs text-[#666] uppercase tracking-wider mb-1.5">New Password</label>
            <input [(ngModel)]="newPassword" type="password" placeholder="••••••••"
              class="w-full bg-[#0D0D0F] border border-[#1E1E24] rounded-xl px-4 py-3 text-[#F0F0F2] text-sm focus:outline-none focus:border-[#D4537E] transition-colors"/>
          </div>
          <div>
            <label class="block text-xs text-[#666] uppercase tracking-wider mb-1.5">Confirm Password</label>
            <input [(ngModel)]="confirmPassword" type="password" placeholder="••••••••"
              class="w-full bg-[#0D0D0F] border border-[#1E1E24] rounded-xl px-4 py-3 text-[#F0F0F2] text-sm focus:outline-none focus:border-[#D4537E] transition-colors"/>
          </div>
          <button (click)="submit()" [disabled]="loading"
            class="w-full bg-[#D4537E] hover:bg-[#C4436E] disabled:opacity-50 text-white rounded-xl py-3 text-sm transition-colors">
            {{ loading ? 'Resetting...' : 'Reset Password' }}
          </button>
        </div>
      </div>
    </div>
  `
})
export class ResetPasswordComponent implements OnInit {
  token = '';
  newPassword = '';
  confirmPassword = '';
  loading = false;
  success = false;
  error = '';

  constructor(private route: ActivatedRoute, public router: Router, private http: HttpClient) {}

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
  }

  submit() {
    if (!this.newPassword || this.newPassword !== this.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }
    if (this.newPassword.length < 6) {
      this.error = 'Password must be at least 6 characters';
      return;
    }
    this.loading = true;
    this.http.post(`${environment.apiUrl}/Auth/reset-password`,
      { token: this.token, newPassword: this.newPassword },
      { responseType: 'text' }
    ).subscribe({
      next: () => { this.success = true; this.loading = false; },
      error: () => { this.error = 'Invalid or expired link'; this.loading = false; }
    });
  }
}