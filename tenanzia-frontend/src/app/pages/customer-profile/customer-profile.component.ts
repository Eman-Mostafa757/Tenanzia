import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomersService } from '../../services/customers.service';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [CommonModule,NotificationBellComponent],
  templateUrl: './customer-profile.component.html',
})
export class CustomerProfileComponent implements OnInit {
showMenu = false;
  profile: any = null;
  loading = true;
  activeTab = 'orders';
username = '';
curuntPlan :any = null;
  constructor(
    private customersService: CustomersService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
      public themeService: ThemeService,
      private subscriptionService: SubscriptionService

  ) {}

  ngOnInit() {
       this.username = this.authService.getUsername();
this.getCurrentPlan();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.customersService.getProfile(+id).subscribe({
        next: (res) => {
          this.profile = res;
          this.loading = false;
        },
        error: () => this.router.navigate(['/customers'])
      });
    }
  }

  getValueScoreClass() {
    if (this.profile?.valueScore === 'VIP') return 'bg-[#2A1F0A] text-[#EF9F27]';
    if (this.profile?.valueScore === 'Regular') return 'bg-[#1A1829] text-[#7F77DD]';
    return 'bg-[#12251E] text-[#5DCAA5]';
  }

  getStatusClass(status: string) {
    if (status === 'Completed' || status === 'Paid') return 'bg-[#12251E] text-[#5DCAA5]';
    if (status === 'Processing') return 'bg-[#1A1829] text-[#7F77DD]';
    if (status === 'Pending' || status === 'Unpaid') return 'bg-[#2A1F0A] text-[#EF9F27]';
    return 'bg-[#1E1E24] text-[#666]';
  }

  getLastOrderText() {
    if (this.profile?.daysSinceLastOrder === -1) return 'No orders yet';
    if (this.profile?.daysSinceLastOrder === 0) return 'Ordered today';
    if (this.profile?.daysSinceLastOrder === 1) return 'Ordered yesterday';
    if (this.profile?.daysSinceLastOrder > 30) return `${this.profile.daysSinceLastOrder} days ago ⚠`;
    return `${this.profile?.daysSinceLastOrder} days ago`;
  }

  getLastOrderClass() {
    if (this.profile?.daysSinceLastOrder > 30) return 'text-[#D4537E]';
    return 'text-[#5DCAA5]';
  }

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }

   getCurrentPlan()
  {
    this.subscriptionService.getCurrent().subscribe({
      next :(res)=> { this.curuntPlan=res;

      }
    })

  }
}