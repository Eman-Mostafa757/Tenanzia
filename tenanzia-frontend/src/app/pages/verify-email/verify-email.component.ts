import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-[#0D0D0F] flex items-center justify-center">
      <div class="bg-[#111114] border border-[#1E1E24] rounded-2xl p-8 text-center max-w-md w-full">
        <div *ngIf="loading" class="text-[#666]">Verifying your email...</div>
        <div *ngIf="success && !loading">
          <div class="text-4xl mb-4">✅</div>
          <h2 class="text-xl font-medium text-[#F0F0F2] mb-2">Email Verified!</h2>
          <p class="text-[#666] text-sm mb-6">Your account is now active.</p>
          <button (click)="goToLogin()"
            class="bg-[#D4537E] text-white px-6 py-2.5 rounded-xl hover:bg-[#C4436E] transition-colors">
            Login Now
          </button>
        </div>
        <div *ngIf="!success && !loading">
          <div class="text-4xl mb-4">❌</div>
          <h2 class="text-xl font-medium text-[#F0F0F2] mb-2">Invalid Link</h2>
          <p class="text-[#666] text-sm">This verification link is invalid or expired.</p>
        </div>
      </div>
    </div>
  `
})
export class VerifyEmailComponent implements OnInit {
  loading = true;
  success = false;

  constructor(private route: ActivatedRoute, private router: Router, private http: HttpClient) {}

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.http.get(`${environment.apiUrl}/Auth/verify-email?token=${token}`, { responseType: 'text' })
        .subscribe({
          next: () => { this.success = true; this.loading = false; },
          error: () => { this.success = false; this.loading = false; }
        });
    }
  }

  goToLogin() { this.router.navigate(['/login']); }
}