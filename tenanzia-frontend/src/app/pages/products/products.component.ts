import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductsService } from '../../services/products.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html',
})
export class ProductsComponent implements OnInit {

  products: any[] = [];
  loading = true;
  showModal = false;
  editingProduct: any = null;

  form = {
    name: '',
    description: '',
    price: 0,
    unit: 'piece'
  };

  constructor(
    private productsService: ProductsService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() { this.loadProducts(); }

  loadProducts() {
    this.loading = true;
    this.productsService.getAll().subscribe({
      next: (res) => { this.products = res; this.loading = false; }
    });
  }

  openAddModal() {
    this.editingProduct = null;
    this.form = { name: '', description: '', price: 0, unit: 'piece' };
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

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }
}