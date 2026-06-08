import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TeamService } from '../../services/team.service';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';

@Component({
  selector: 'app-employee-profile',
  standalone: true,
  imports: [CommonModule,NotificationBellComponent],
  templateUrl: './employee-profile.component.html',
})
export class EmployeeProfileComponent implements OnInit {
showMenu = false;

  profile: any = null;
  loading = true;
  username ='';
  curuntPlan:any=null;

  constructor(
    private teamService: TeamService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
      public themeService: ThemeService,
      private subscriptionService :SubscriptionService,

  ) {}

  ngOnInit() {
      this.username = this.authService.getUsername();
this.getCurrentPlan();
    const userId = this.route.snapshot.paramMap.get('id');
    if (userId) {
      this.teamService.getUserProfile(+userId).subscribe({
        next: (res) => {
          this.profile = res;
          this.loading = false;
        },
        error: () => this.router.navigate(['/team'])
      });
    }
  }

  getActivityClass() {
    if (this.profile?.activityStatus === 'Active') return 'bg-[#12251E] text-[#5DCAA5]';
    if (this.profile?.activityStatus === 'Overdue') return 'bg-[#3C1528] text-[#ED93B1]';
    return 'bg-[#1E1E24] text-[#666]';
  }

  getActivityDot() {
    if (this.profile?.activityStatus === 'Active') return 'bg-[#5DCAA5]';
    if (this.profile?.activityStatus === 'Overdue') return 'bg-[#D4537E]';
    return 'bg-[#666]';
  }

  getStatusClass(status: string) {
    if (status === 'Completed') return 'bg-[#12251E] text-[#5DCAA5]';
    if (status === 'InProgress') return 'bg-[#1A1829] text-[#7F77DD]';
    if (status === 'ToDo') return 'bg-[#3C1528] text-[#ED93B1]';
    return 'bg-[#1E1E24] text-[#666]';
  }

  getPriorityClass(priority: string) {
    if (priority === 'High') return 'text-[#ED93B1]';
    if (priority === 'Medium') return 'text-[#EF9F27]';
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