import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CustomersComponent } from './pages/customers/customers.component';
import { TasksComponent } from './pages/tasks/tasks.component';
import { TeamComponent } from './pages/team/team.component';
import { EmployeeProfileComponent } from './pages/employee-profile/employee-profile.component';
import { CustomerProfileComponent } from './pages/customer-profile/customer-profile.component';
import { OrdersComponent } from './pages/orders/orders.component';
import { InvoicesComponent } from './pages/invoices/invoices.component';
import { RegisterComponent } from './pages/register/register.component';
import { authGuard } from './guards/auth.guard';
import { SettingsComponent } from './pages/settings/settings.component';
import { SubscriptionComponent } from './pages/subscription/subscription.component';
import { ProductsComponent } from './pages/products/products.component';
import { managerGuard } from './guards/manager.guard';
import { ownerGuard } from './guards/owner.guard';
import { VerifyEmailComponent } from './pages/verify-email/verify-email.component';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';
export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'verify-email', component: VerifyEmailComponent },
{ path: 'forgot-password', component: ForgotPasswordComponent },
{ path: 'reset-password', component: ResetPasswordComponent },

  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'tasks', component: TasksComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: 'orders', component: OrdersComponent, canActivate: [authGuard] },

  { path: 'customers', component: CustomersComponent, canActivate: [authGuard, managerGuard] },
  { path: 'customers/:id', component: CustomerProfileComponent, canActivate: [authGuard, managerGuard] },
  { path: 'team', component: TeamComponent, canActivate: [authGuard, managerGuard] },
  { path: 'team/:id', component: EmployeeProfileComponent, canActivate: [authGuard, managerGuard] },
  { path: 'invoices', component: InvoicesComponent, canActivate: [authGuard, managerGuard] },
  { path: 'products', component: ProductsComponent, canActivate: [authGuard, managerGuard] },

  { path: 'subscription', component: SubscriptionComponent, canActivate: [authGuard, ownerGuard] },


];
