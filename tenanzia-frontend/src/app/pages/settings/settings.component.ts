import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SettingsService } from '../../services/settings.service';
import { AuthService } from '../../services/auth.service';
import { SubscriptionService } from '../../services/subscription.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationBellComponent],
  templateUrl: './settings.component.html',
})
export class SettingsComponent implements OnInit {
showUserMenu = false;
  me: any = null;
  loading = true;
  activeTab = 'profile';
  success = '';
  error = '';
      username= '';
  curuntPlan: any = null;
  profileForm = {
    username: '',
    email: '',
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  companyForm = {
    name: ''
  };
  isOwner = false;
  isManager = false;
  isEmployee = false;
  isManagerOrOwner = false;
  showMenu = false;

  constructor(
    private settingsService: SettingsService,
    private authService: AuthService,
    private router: Router,
    private subscriptionService: SubscriptionService,
    public themeService: ThemeService

  ) { }

  ngOnInit() {
    this.isOwner = this.authService.isOwner();
    this.isManager = this.authService.isManager();
    this.isEmployee = this.authService.isEmployee();
    this.isManagerOrOwner = this.authService.isManagerOrOwner();
        this.username = this.authService.getUsername();

    this.loadMe();
    this.getCurrentPlan();
  }

  loadMe() {
    this.settingsService.getMe().subscribe({
      next: (res) => {
        this.me = res;
        this.profileForm.username = res.username;
        this.profileForm.email = res.email;
        this.companyForm.name = res.company?.name || '';
        this.loading = false;
      }
    });
  }

  saveProfile() {
    this.error = '';
    this.success = '';

    if (this.profileForm.newPassword) {
      if (this.profileForm.newPassword !== this.profileForm.confirmPassword) {
        this.error = 'Passwords do not match';
        return;
      }
      if (this.profileForm.newPassword.length < 6) {
        this.error = 'Password must be at least 6 characters';
        return;
      }
    }

    const data: any = {
      username: this.profileForm.username,
      email: this.profileForm.email
    };

    if (this.profileForm.newPassword) {
      data.password = this.profileForm.newPassword;
    }

    this.settingsService.updateMe(data).subscribe({
      next: () => {
        this.success = 'Profile updated successfully!';
        this.profileForm.currentPassword = '';
        this.profileForm.newPassword = '';
        this.profileForm.confirmPassword = '';
        setTimeout(() => this.success = '', 3000);
      },
      error: (err) => {
        this.error = err.error || 'Something went wrong';
      }
    });
  }

  saveCompany() {
    this.error = '';
    this.success = '';

    if (!this.companyForm.name) {
      this.error = 'Company name is required';
      return;
    }

    this.settingsService.updateCompany(this.companyForm.name).subscribe({
      next: () => {
        this.success = 'Company name updated!';
        setTimeout(() => this.success = '', 3000);
      },
      error: () => {
        this.error = 'Something went wrong';
      }
    });
  }

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }

  getCurrentPlan() {
    this.subscriptionService.getCurrent().subscribe({
      next: (res) => {
        this.curuntPlan = res;

      }
    })

  }

}