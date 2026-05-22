import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { Product, RecommendationRequest } from '../../../shared/models/product';
import { RecommendationResult } from '../../../shared/models/recommendation';
import { User } from '../../../shared/models/user';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './products.html',
  styleUrl: './products.css'
})
export class Products implements OnInit {
  products: Product[] = [];
  cart: Product[] = [];
  recommendations: RecommendationResult[] = [];
  isLoadingProducts = false;
  isLoadingRecommendations = false;
  currentUser: User | null = null;

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef  // ✅ Add this
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoadingProducts = true;
    this.apiService.getProducts().subscribe({
      next: (products) => {
        this.products = products;
        this.isLoadingProducts = false;
        this.cdr.detectChanges(); // ✅ Force UI update
      },
      error: (err) => {
        console.log('Error:', err);
        this.isLoadingProducts = false;
        this.cdr.detectChanges();
        alert('Failed to load products.');
      }
    });
  }

  addToCart(product: Product): void {
    console.log('Adding to cart:', product);
    console.log('Cart before:', this.cart);
    const exists = this.cart.find(p => p.id === product.id);
    if (exists) {
      console.log('Already in cart!');
      return;
    }
    this.cart = [...this.cart, product];
    console.log('Cart after:', this.cart);
    this.cdr.detectChanges();
    this.loadRecommendations();
  }

  removeFromCart(product: Product): void {
    this.cart = this.cart.filter(p => p.id !== product.id);
    this.cdr.detectChanges();
    if (this.cart.length > 0) {
      this.loadRecommendations();
    } else {
      this.recommendations = [];
    }
  }

  isInCart(product: Product): boolean {
    return !!this.cart.find(p => p.id === product.id);
  }

  loadRecommendations(): void {
    if (this.cart.length === 0) return;
    this.isLoadingRecommendations = true;
    const request: RecommendationRequest = {
      products: this.cart.map(p => p.name)
    };
    this.apiService.getRecommendations(request).subscribe({
      next: (results) => {
        this.recommendations = results;
        this.isLoadingRecommendations = false;
        this.cdr.detectChanges(); // ✅ Force UI update
      },
      error: () => {
        this.isLoadingRecommendations = false;
        this.cdr.detectChanges();
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }

  goToTickets(): void {
    this.router.navigate(['/customer/tickets']);
  }

  goToPolicy(): void {
    this.router.navigate(['/customer/policy']);
  }
}