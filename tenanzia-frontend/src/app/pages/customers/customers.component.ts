import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomersService } from '../../services/customers.service';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule,NotificationBellComponent],
  templateUrl: './customers.component.html',
})
export class CustomersComponent implements OnInit {

  customers: any[] = [];
  loading = true;
  search = '';
  showModal = false;
  editingCustomer: any = null;
  limitError = '';
limits: any = null;
curuntPlan : any =null;


  form = {
    name: '',
    email: '',
    phone: '',
    address: '',
    notes: '',
    status: 'Active'
  };
username = '';
  constructor(
    private customersService: CustomersService,
    private authService: AuthService,
    private router: Router,
      public themeService: ThemeService,
      private subscriptionService:SubscriptionService,

  ) {}

  ngOnInit() {
     this.username = this.authService.getUsername();

    this.loadCustomers();
      this.loadLimits();
      this.getCurrentPlan();

  }
  
  loadLimits() {
 this.customersService.GetLimits().subscribe({
    next: (res) => this.limits = res
  });
}

  loadCustomers() {
    this.loading = true;
    this.customersService.getAll(this.search).subscribe({
      next: (res) => {
        this.customers = res;
        this.loading = false;
      },
      error: () => this.authService.logout()
    });
  }

  openAddModal() {
    this.editingCustomer = null;
    this.form = { name: '', email: '', phone: '', address: '', notes: '', status: 'Active' };
    this.showModal = true;
  }

  openEditModal(customer: any) {
    this.editingCustomer = customer;
    this.form = { ...customer };
    this.showModal = true;
  }

  saveCustomer() {
      this.limitError = '';

    if (this.editingCustomer) {
      this.customersService.update(this.editingCustomer.id, this.form).subscribe({
        next: () => { this.showModal = false; this.loadCustomers(); this.loadLimits();}
      });
    } else {
      this.customersService.create(this.form).subscribe({
        next: () => { this.showModal = false; this.loadCustomers();this.loadLimits(); },
         error: (err) => {
        if (err.error?.upgradeRequired) {
          this.limitError = err.error.error;
          this.showModal = false;
        }
      }
      });
    }
  }

  deleteCustomer(id: number) {
    if (confirm('Are you sure?')) {
      this.customersService.delete(id).subscribe({
        next: () => {this.loadCustomers();this.loadLimits();}
      });
    }
  }

  logout() {
    this.authService.logout();
  }

  goTo(page: string) {
    this.router.navigate([`/${page}`]);
  }
  getCurrentPlan()
  {
    this.subscriptionService.getCurrent().subscribe({
      next :(res)=> { this.curuntPlan=res;

      }
    })

  }
  
}