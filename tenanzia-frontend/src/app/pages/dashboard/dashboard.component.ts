import { Component, OnInit , AfterViewInit, ViewChild, ElementRef} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardService } from '../../services/dashboard.service';
import { AuthService } from '../../services/auth.service';
import { OrdersService } from '../../services/orders.service';
import { Chart, registerables } from 'chart.js';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';
import { ExportService } from '../../services/export.service';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule,NotificationBellComponent],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit , AfterViewInit {

 @ViewChild('revenueChart') revenueChartRef!: ElementRef;
  @ViewChild('ordersChart') ordersChartRef!: ElementRef

  data: any = null;
  loading = true;
    chartData: any[] = [];
 revenueChart: any = null;
  ordersChart: any = null;
username = '';
curuntPlan :any=null;

isOwner = false;
isManager = false;
isEmployee = false;
isManagerOrOwner = false;



  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    private ordersService:OrdersService,
    private router: Router,
     public themeService: ThemeService,
     private subscriptionService: SubscriptionService,
       private exportService: ExportService

  ) {}


  ngOnInit() {
    this.isOwner = this.authService.isOwner();
  this.isManager = this.authService.isManager();
  this.isEmployee = this.authService.isEmployee();
  this.isManagerOrOwner = this.authService.isManagerOrOwner();
    this.getCurrentPlan();
  this.username = this.authService.getUsername();

  this.dashboardService.getDashboard().subscribe({
    next: (res) => {
      this.data = res;
      this.loading = false;

      // بعد ما الـ view يتحمل
      setTimeout(() => {
        this.loadChartData();
      }, 200);
    },
    error: () => this.authService.logout()
  });
}


  logout() {
    this.authService.logout();
  }
  goTo(page: string) {
  this.router.navigate([`/${page}`]);
}


 ngAfterViewInit() {}

  loadChartData() {
  this.dashboardService.getRevenueChart().subscribe({
    next: (res) => {
      this.chartData = res;
      setTimeout(() => this.renderCharts(), 100);
    }
  });
}
  renderCharts() {
    // Revenue Chart
    if (this.revenueChartRef) {
      if (this.revenueChart) this.revenueChart.destroy();

      this.revenueChart = new Chart(this.revenueChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: this.chartData.map(d => d.label),
          datasets: [{
            label: 'Revenue ($)',
            data: this.chartData.map(d => d.revenue),
            backgroundColor: 'rgba(212, 83, 126, 0.3)',
            borderColor: '#D4537E',
            borderWidth: 2,
            borderRadius: 6,
          }]
        },
        options: {
          responsive: true,
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: (ctx) => `$${ctx.raw?.toLocaleString()}`
              }
            }
          },
          scales: {
            x: {
              grid: { color: '#1E1E24' },
              ticks: { color: '#666', font: { size: 11 } }
            },
            y: {
              grid: { color: '#1E1E24' },
              ticks: {
                color: '#666',
                font: { size: 11 },
                callback: (val) => `$${val?.toLocaleString()}`
              }
            }
          }
        }
      });
    }

    // Orders Chart
    if (this.ordersChartRef) {
      if (this.ordersChart) this.ordersChart.destroy();

      this.ordersChart = new Chart(this.ordersChartRef.nativeElement, {
        type: 'line',
        data: {
          labels: this.chartData.map(d => d.label),
          datasets: [{
            label: 'Completed Orders',
            data: this.chartData.map(d => d.orders),
            borderColor: '#5DCAA5',
            backgroundColor: 'rgba(93, 202, 165, 0.1)',
            borderWidth: 2,
            pointBackgroundColor: '#5DCAA5',
            pointRadius: 4,
            fill: true,
            tension: 0.4
          }]
        },
        options: {
          responsive: true,
          plugins: {
            legend: { display: false }
          },
          scales: {
            x: {
              grid: { color: '#1E1E24' },
              ticks: { color: '#666', font: { size: 11 } }
            },
           y: {
  beginAtZero: true,
  grid: { color: '#1E1E24' },
  ticks: {
    color: '#666',
    font: { size: 11 },
    stepSize: 1
  }
}
          }
        }
      });
    }
  }

   getCurrentPlan()
  {
    this.subscriptionService.getCurrent().subscribe({
      next :(res)=> { this.curuntPlan=res;

      }
    })

  }
  exportRevenue() {
  this.exportService.exportRevenuePDF(
    this.chartData,
    this.data,
    this.data.companyName
  );
}

}