import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OrdersService } from '../../services/orders.service';
import { CustomersService } from '../../services/customers.service';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';
import { ProductsService } from '../../services/products.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationBellComponent],
  templateUrl: './orders.component.html',
})
export class OrdersComponent implements OnInit {
  allProducts: any[] = [];
  lowStock: any[] = [];
  orders: any[] = [];
  order: any = null;
  filteredOrders: any[] = [];
  stats: any = null;
  customers: any[] = [];
  loading = true;
  selectedStatus = '';
  searchText = '';
  showCreateModal = false;
  showDetailsModal = false;
  selectedOrder: any = null;
  username = '';
  showStockIsNotHaveQuantaty = false;
  currentUserId: number | undefined;
  curuntPlan: any = null;

  form: {
    customerId: number | null,
    notes: string,
    items: {
      productId: number | null;
      productName: string;
      quantity: number;
      unitPrice: number;
      suggestions: any[];
    }[]
  } = {
      customerId: null,
      notes: '',
      items: [{
        productId: null,
        productName: '',
        quantity: 1,
        unitPrice: 0,
        suggestions: []
      }]
    };
  isOwner = false;
  isManager = false;
  isEmployee = false;
  isManagerOrOwner = false;
  constructor(
    private ordersService: OrdersService,
    private customersService: CustomersService,
    private authService: AuthService,
    private router: Router,
    public themeService: ThemeService,
    private subscriptionService: SubscriptionService,
    private productsService: ProductsService,


  ) { }

  ngOnInit() {
    this.isOwner = this.authService.isOwner();
    this.isManager = this.authService.isManager();
    this.isEmployee = this.authService.isEmployee();
    this.isManagerOrOwner = this.authService.isManagerOrOwner();
    this.username = this.authService.getUsername();
    this.loadOrders();
    this.loadStats();
    this.loadCustomers();
    this.getCurrentPlan();
    this.loadProducts();

  }

  loadOrders() {
    this.loading = true;
    this.ordersService.getAll(this.selectedStatus).subscribe({
      next: (res) => {
        this.orders = res;
        this.applySearch();
        this.loading = false;
      },
      error: () => this.authService.logout()
    });
  }

  loadStats() {
    this.ordersService.getStats().subscribe({
      next: (res) => this.stats = res
    });
  }

  loadCustomers() {
    this.customersService.getAll().subscribe({
      next: (res) => this.customers = res
    });
  }

  applySearch() {
    if (!this.searchText) {
      this.filteredOrders = this.orders;
      return;
    }
    this.filteredOrders = this.orders.filter(o =>
      o.customerName.toLowerCase().includes(this.searchText.toLowerCase())
    );
  }

  filterByStatus(status: string) {
    this.selectedStatus = status;
    this.loadOrders();
  }

  openDetails(order: any) {
    this.selectedOrder = order;
    this.showDetailsModal = true;
  }

  updateStatus(id: number, status: string) {
    this.ordersService.updateStatus(id, status).subscribe({
      next: () => {
        this.loadOrders();
        this.loadStats();
        if (this.selectedOrder?.id === id) {
          this.selectedOrder.status = status;
        }
      },
      error: (err) => {
        // Stock error
        const errorMsg = err.error?.error || err.error || 'Something went wrong';
        console.log(errorMsg);
        alert(errorMsg);
      }
    });
  }
  // SyntaxError: Unexpected token 'S', "Status upd"... is not valid JSON



  // for (const item of this.form.items) {

  //   const product = this.allProducts.find(
  //     p => p.id === item.productId
  //   );

  //   if (product && item.quantity > product.stockQuantity) {

  //     alert(
  //       `${product.name} has only ${product.stockQuantity} in stock`
  //     );

  //     return;
  //   }
  // }


  addItem() {
    this.form.items.push({
      productId: null,
      productName: '',
      quantity: 1,
      unitPrice: 0,
      suggestions: []
    });
  }

  showAllProducts(item: any) {
    console.log('allProducts', this.allProducts);
    item.suggestions = [...this.allProducts];
  }

  onProductInputChange(item: any) {
    item.productId = null;
    this.searchProducts(item, item.productName);
  }

  removeItem(index: number) {
    if (this.form.items.length > 1)
      this.form.items.splice(index, 1);
  }

  getOrderTotal() {
    return this.form.items.reduce((sum, i) => sum + (i.quantity * i.unitPrice), 0);
  }

  createOrder() {
    if (!this.form.customerId) {
      alert('Please select customer');
      return;
    }

    if (this.form.items.some(i => !i.productId)) {
      alert('Please select products from the list');
      return;
    }

    const orderData = {
      customerId: this.form.customerId,
      notes: this.form.notes,
      items: this.form.items.map(i => ({
        productName: i.productName,
        quantity: i.quantity,
        unitPrice: i.unitPrice
      }))
    };
    for (const item of this.form.items) {

      const product = this.allProducts.find(
        p => p.id === item.productId
      );

      if (product && item.quantity > product.stockQuantity) {

        alert(
          `${product.name} has only ${product.stockQuantity} in stock`
        );

        return;
      }
    }

    this.ordersService.create(orderData).subscribe({
      next: () => {
        this.showCreateModal = false;
        this.form = {
          customerId: null,
          notes: '',
          items: [{
            productName: '', quantity: 1, unitPrice: 0, suggestions: [],
            productId: null
          }]
        };
        this.loadOrders();
        this.loadStats();
      }
    });
  }

  getStatusClass(status: string) {
    if (status === 'Completed') return 'bg-[#12251E] text-[#5DCAA5]';
    if (status === 'Processing') return 'bg-[#1A1829] text-[#7F77DD]';
    if (status === 'Pending') return 'bg-[#2A1F0A] text-[#EF9F27]';
    return 'bg-[#1E1E24] text-[#666]';
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
  loadProducts() {
    this.productsService.getAll().subscribe({
      next: (res) => {
        console.log('Products:', res);
        this.allProducts = res;
      },
      error: (err) => {
        console.log('Error:', err);
      }
    });
  }

  searchProducts(item: any, query: string) {

    if (!query) {
      item.suggestions = [...this.allProducts];
      return;
    }

    item.suggestions = this.allProducts.filter(p =>
      p.name.toLowerCase().includes(query.toLowerCase())
    );
  }

  selectProduct(item: any, product: any) {
    item.productId = product.id;
    item.productName = product.name;
    item.unitPrice = product.price;
    item.suggestions = [];
  }

  clearSuggestionsDelayed(item: any) {
    setTimeout(() => {
      item.suggestions = [];
    }, 200);
  }

  getLowStock() {
    this.productsService.getLowStock().subscribe(
      {
        next: (res) => { this.lowStock = res; }
      }
    )

  }




}