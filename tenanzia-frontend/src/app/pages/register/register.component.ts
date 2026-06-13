import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
registered = false;
registeredEmail = '';
  form = {
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    companyName: ''
  };

  error = '';
  loading = false;
  step = 1; // 1 = Account Info, 2 = Company Info

  constructor(
    private authService: AuthService,
    private router: Router,
      public themeService: ThemeService

  ) {}

  nextStep() {
    if (!this.form.username || !this.form.email || !this.form.password) {
      this.error = 'Please fill in all fields';
      return;
    }
    if (this.form.password !== this.form.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }
    if (this.form.password.length < 6) {
      this.error = 'Password must be at least 6 characters';
      return;
    }
    this.error = '';
    this.step = 2;
  }

  onRegister() {
    if (!this.form.companyName) {
      this.error = 'Please enter your company name';
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.register({
      username: this.form.username,
      email: this.form.email,
      password: this.form.password,
      companyName: this.form.companyName
    }).subscribe({
      next: () => {
        this.loading = false;
      this.registered = true;
      this.registeredEmail = this.form.email;
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error || 'Registration failed';
      }
    });
  }
  
}