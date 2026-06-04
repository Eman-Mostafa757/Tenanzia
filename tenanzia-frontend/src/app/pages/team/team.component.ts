import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TeamService } from '../../services/team.service';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';

@Component({
  selector: 'app-team',
  standalone: true,
  imports: [CommonModule, FormsModule,NotificationBellComponent],
  templateUrl: './team.component.html',
})
export class TeamComponent implements OnInit {
workload: any[] = [];
showEditModal = false;
curuntPlan :any=null;
editingMember: any = null;
editForm = {
  username: '',
  email: '',
  password: ''
};

  members: any[] = [];
  loading = true;
  showModal = false;
  success = '';
  error = '';

  form = {
    username: '',
    email: '',
    password: '',
    role: 'Employee'
  };
  username='';

  constructor(
    private teamService: TeamService,
    private authService: AuthService,
    private router: Router,
      public themeService: ThemeService,
      private subscriptionService:SubscriptionService,

  ) {}

  ngOnInit() {
          this.username = this.authService.getUsername();

    this.loadMembers();
      this.loadWorkload();
      this.getCurrentPlan();

  }
  loadWorkload() {
  this.teamService.getTeamWorkload().subscribe({
    next: (res) => this.workload = res
  });
}


  loadMembers() {
    this.loading = true;
    this.teamService.getTeamMembers().subscribe({
      next: (res) => {
        this.members = res;
        this.loading = false;
      },
      error: () => this.authService.logout()
    });
  }

  inviteEmployee() {
    if (!this.form.username || !this.form.email || !this.form.password) {
      this.error = 'Please fill in all fields';
      return;
    }

    this.teamService.inviteEmployee(this.form).subscribe({
      next: () => {
        this.showModal = false;
        this.success = 'Employee invited successfully!';
        this.form = { username: '', email: '', password: '', role: 'Employee' };
        this.loadMembers();
        setTimeout(() => this.success = '', 3000);
      },
      error: (err) => {
        this.error = 'Something went wrong';
      }
    });
  }

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }



  getWorkloadColor(level: string) {
  if (level === 'Free') return 'bg-[#1E1E24]';
  if (level === 'Normal') return 'bg-[#5DCAA5]';
  if (level === 'Heavy') return 'bg-[#EF9F27]';
  return 'bg-[#D4537E]';
}

getWorkloadWidth(activeTasks: number) {
  const max = 10;
  return Math.min((activeTasks / max) * 100, 100);
}

getActivityDot(status: string) {
  if (status === 'Active') return 'bg-[#5DCAA5]';
  if (status === 'Overdue') return 'bg-[#D4537E]';
  return 'bg-[#666]';
}
showAssignModal = false;
assigningMember: any = null;
allTasks: any[] = [];
selectedTaskId: number | null = null;

openAssignModal(member: any) {
  this.assigningMember = member;
  this.selectedTaskId = null;
  this.teamService.getUnassignedTasks().subscribe({
    next: (res) => {
      // فلتري الـ tasks اللي مش completed أو cancelled
      this.allTasks = res.filter((t: any) =>
        t.status !== 'Completed' && t.status !== 'Cancelled'
      );
      this.showAssignModal = true;
    }
  });
}

assignTask() {
  if (!this.selectedTaskId || !this.assigningMember) return;
  this.teamService.assignTask(this.assigningMember.id, this.selectedTaskId).subscribe({
    next: () => {
      this.showAssignModal = false;
      this.success = `Task assigned to ${this.assigningMember.username}!`;
      this.loadWorkload();
      setTimeout(() => this.success = '', 3000);
    }
  });
}
openEditModal(member: any) {
  this.editingMember = member;
  this.editForm = {
    username: member.username,
    email: member.email,
    password: ''
  };
  this.showEditModal = true;
}

saveUser() {
  if (!this.editingMember) return;

  const data: any = {};
  if (this.editForm.username) data.username = this.editForm.username;
  if (this.editForm.email) data.email = this.editForm.email;
  if (this.editForm.password) data.password = this.editForm.password;

  this.teamService.updateUser(this.editingMember.id, data).subscribe({
    next: () => {
      this.showEditModal = false;
      this.success = 'Member updated successfully!';
      this.loadMembers();
      this.loadWorkload();
      setTimeout(() => this.success = '', 3000);
    },
    error: () => {
      this.error = 'Something went wrong';
    }
  });
}

removeUser(userId: number) {
  if (confirm('Remove this member from the team?')) {
    this.teamService.removeUser(userId).subscribe({
      next: () => {
        this.success = 'Member removed!';
        this.loadMembers();
        this.loadWorkload();
        setTimeout(() => this.success = '', 3000);
      }
    });
  }
}

updateRole(userId: number, newRole: string) {
  this.teamService.updateRole(userId, newRole).subscribe({
    next: () => {
      this.success = 'Role updated!';
      this.loadMembers();
      setTimeout(() => this.success = '', 3000);
    }
  });
}


  getCurrentPlan()
  {
    this.subscriptionService.getCurrent().subscribe({
      next :(res)=> { this.curuntPlan=res;

      }
    })

  }

}