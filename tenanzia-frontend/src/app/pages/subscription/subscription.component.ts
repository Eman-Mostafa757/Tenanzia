import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SubscriptionService } from '../../services/subscription.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-subscription',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './subscription.component.html',
})
export class SubscriptionComponent implements OnInit {

  plans: any[] = [];
  current: any = null;
  loading = true;
  success = '';
  error = '';
showUserMenu = false;
  constructor(
    private subscriptionService: SubscriptionService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.subscriptionService.getPlans().subscribe({
      next: (plans) => {
        this.plans = plans;
        this.subscriptionService.getCurrent().subscribe({
          next: (current) => {
            this.current = current;
            this.loading = false;
          }
        });
      }
    });
  }

  upgrade(planName: string) {
    this.subscriptionService.checkout(planName).subscribe({
      next: (res) => {
        window.location.href = res.checkoutUrl;
      },
      error: () => this.error = 'Something went wrong'
    });
  }

  downgrade() {
    if (confirm('Are you sure you want to downgrade to Free Plan?')) {
      this.subscriptionService.downgrade().subscribe({
        next: () => {
          this.success = 'Downgraded to Free Plan';
          this.loadData();
          setTimeout(() => this.success = '', 3000);
        }
      });
    }
  }

  isCurrent(planName: string) {
    return this.current?.planName === planName;
  }

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }
}