import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductsService } from '../../services/products.service';
import { AuthService } from '../../services/auth.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationBellComponent],
  templateUrl: './products.component.html',
})
export class ProductsComponent implements OnInit {

  products: any[] = [];
  lowStockProducts: any[] = [];
  curuntPlan: any = null;
  loading = true;
  showModal = false;
  editingProduct: any = null;
  username = '';

  form = {
    name: '',
    description: '',
    price: 0,
    unit: 'piece',
    stockQuantity: 0,
    lowStockThreshold: 5,
    trackStock: true
  };
  Math = Math;
  isOwner = false;
  isManager = false;
  isEmployee = false;
  isManagerOrOwner = false;
  showMenu = false;

  constructor(
    private productsService: ProductsService,
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
    this.loadProducts();
    this.loadLowStock();
  }

  loadProducts() {
    this.loading = true;
    this.productsService.getAll().subscribe({
      next: (res) => { this.products = res; this.loading = false; }
    });
  }

  openAddModal() {
    this.editingProduct = null;
    this.form = {
      name: '', description: '', price: 0, unit: 'piece', stockQuantity: 0,
      lowStockThreshold: 5,
      trackStock: true
    };
    this.showModal = true;
  }

  openEditModal(product: any) {
    this.editingProduct = product;
    this.form = { ...product };
    this.showModal = true;
  }

  saveProduct() {
    if (!this.form.name || !this.form.price) return;

    if (this.editingProduct) {
      this.productsService.update(this.editingProduct.id, this.form).subscribe({
        next: () => { this.showModal = false; this.loadProducts(); }
      });
    } else {
      this.productsService.create(this.form).subscribe({
        next: () => { this.showModal = false; this.loadProducts(); }
      });
    }
  }

  deleteProduct(id: number) {
    if (confirm('Delete this product?')) {
      this.productsService.delete(id).subscribe({
        next: () => this.loadProducts()
      });
    }
  }

  loadLowStock() {
    this.productsService.getLowStock().subscribe({
      next: (res) => this.lowStockProducts = res
    });
  }

  updateStock(product: any, newQuantity: number) {
    this.productsService.updateStock(product.id, newQuantity).subscribe({
      next: () => {
        this.loadProducts();
        this.loadLowStock();
      }
    });
  }

  getStockClass(product: any) {
    if (!product.trackStock) return 'text-[#666]';
    if (product.stockQuantity === 0) return 'text-[#D4537E]';
    if (product.isLowStock) return 'text-[#EF9F27]';
    return 'text-[#5DCAA5]';
  }

  getStockLabel(product: any) {
    if (!product.trackStock) return 'Not tracked';
    if (product.stockQuantity === 0) return 'Out of stock';
    if (product.isLowStock) return 'Low stock';
    return 'In stock';
  }
  getCurrentPlan() {
    this.subscriptionService.getCurrent().subscribe({
      next: (res) => {
        this.curuntPlan = res;

      }
    })

  }

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }
}