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

export const routes: Routes = [
     { path: 'login', component: LoginComponent },
       { path: 'dashboard', component: DashboardComponent , canActivate: [authGuard] },
       { path: 'customers', component: CustomersComponent  , canActivate: [authGuard] },
{ path: 'tasks', component: TasksComponent , canActivate: [authGuard] },
{ path: 'team', component: TeamComponent , canActivate: [authGuard] },
{ path: 'team/:id', component: EmployeeProfileComponent, canActivate: [authGuard]  },
{path: 'customers/:id' , component:CustomerProfileComponent, canActivate: [authGuard] },
{ path: 'orders', component: OrdersComponent, canActivate: [authGuard]  },
{ path: 'invoices', component: InvoicesComponent , canActivate: [authGuard] },
{ path: 'register', component: RegisterComponent },
{ path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
{ path: 'subscription', component: SubscriptionComponent, canActivate: [authGuard] },
{ path: 'products', component: ProductsComponent, canActivate: [authGuard] },


  { path: '', redirectTo: 'login', pathMatch: 'full' }
];
