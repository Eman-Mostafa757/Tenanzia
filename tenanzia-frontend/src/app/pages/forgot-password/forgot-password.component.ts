import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-[#0D0D0F] flex items-center justify-center">
      <div class="bg-[#111114] border border-[#1E1E24] rounded-2xl p-8 w-full max-w-md">
        <div class="text-center mb-6">
          <div class="w-12 h-12 bg-[#D4537E] rounded-xl flex items-center justify-center mx-auto mb-4">
            <span class="text-white font-medium text-lg">T</span>
          </div>
          <h1 class="text-xl font-medium text-[#F0F0F2]">Forgot Password</h1>
          <p class="text-[#666] text-sm mt-1">Enter your email to receive a reset link</p>
        </div>

        <div *ngIf="success" class="bg-[#12251E] border border-[#5DCAA5] text-[#5DCAA5] text-sm px-4 py-3 rounded-xl mb-4">
          ✅ Reset link sent! Check your email.
        </div>
        <div *ngIf="error" class="bg-[#3C1528] border border-[#D4537E] text-[#ED93B1] text-sm px-4 py-3 rounded-xl mb-4">
          {{ error }}
        </div>

        <div class="space-y-4">
          <div>
            <label class="block text-xs text-[#666] uppercase tracking-wider mb-1.5">Email</label>
            <input [(ngModel)]="email" type="email" placeholder="you@example.com"
              class="w-full bg-[#0D0D0F] border border-[#1E1E24] rounded-xl px-4 py-3 text-[#F0F0F2] text-sm focus:outline-none focus:border-[#D4537E] transition-colors"/>
          </div>
          <button (click)="submit()" [disabled]="loading"
            class="w-full bg-[#D4537E] hover:bg-[#C4436E] disabled:opacity-50 text-white rounded-xl py-3 text-sm transition-colors">
            {{ loading ? 'Sending...' : 'Send Reset Link' }}
          </button>
          <p class="text-center text-[#666] text-sm">
            <a (click)="router.navigate(['/login'])" class="text-[#D4537E] cursor-pointer hover:text-[#E4638E]">
              Back to Login
            </a>
          </p>
        </div>
      </div>
    </div>
  `
})
export class ForgotPasswordComponent {
  email = '';
  loading = false;
  success = false;
  error = '';

  constructor(public router: Router, private http: HttpClient) {}

  submit() {
    if (!this.email) return;
    this.loading = true;
    this.http.post(`${environment.apiUrl}/Auth/forgot-password`,
      JSON.stringify(this.email),
      { headers: { 'Content-Type': 'application/json' }, responseType: 'text' }
    ).subscribe({
      next: () => { this.success = true; this.loading = false; },
      error: () => { this.error = 'Something went wrong'; this.loading = false; }
    });
  }
}