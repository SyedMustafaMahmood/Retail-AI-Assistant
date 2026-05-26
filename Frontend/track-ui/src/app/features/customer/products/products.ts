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
  private fromRecommendation = false; // Flag to track if adding from recommendation
  
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
      this.products = products.map(p => ({
        ...p,
        imageUrl: 'https://localhost:7073' + p.imageUrl
        //         ↑ replace 7073 with your actual port
      }));
      console.log('Fixed imageUrl:', this.products[0].imageUrl);
      // Should now show: http://localhost:5000/images/products/Keyboard.jpg
      this.isLoadingProducts = false;
      this.cdr.detectChanges();
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
    // ❌ skip recommendation call if coming from recommendations
  if (this.fromRecommendation) {
    this.fromRecommendation = false;
    return;
  }
    this.loadRecommendations();
  }
  addRecommendedToCart(rec: RecommendationResult): void {
  const product: Product = {
    id: rec.id,
    name: rec.product,
    description: rec.Description,
    imageUrl: rec.imageUrl  // ✅ comes from backend directly
  };
  this.addToCart(product);
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
  return this.cart.some(p => p.id === product.id);
}
getRecProduct(rec: RecommendationResult): Product {
  return {
    id: rec.id,
    name: rec.product,
    description: rec.Description ,// Placeholder, as description is not provided in RecommendationResult
    imageUrl:rec.imageUrl
  };
}
  loadRecommendations(): void {
    if (this.cart.length === 0) return;
    this.isLoadingRecommendations = true;
    const request: RecommendationRequest = {
      products: this.cart.map(p => p.name)
    };
    this.apiService.getRecommendations(request).subscribe({
      next: (results) => {
        this.recommendations = results.map((r, index) => ({
    ...r,
    id: index + 1000  , // ✅ temporary unique id
    imageUrl: `https://localhost:7073/images/products/${r.product}.jpg`
  }));
    console.log('Recommendations:', this.recommendations);  // ← add this

        this.isLoadingRecommendations = false;
        this.cdr.detectChanges(); // ✅ Force UI update
      },
      error: () => {
        this.isLoadingRecommendations = false;
        this.cdr.detectChanges();
      }
    });
  }
  getStars(index: number): string {
  switch (index) {
    case 0:
      return '★★★★★'; // 1st
    case 1:
      return '★★★★☆'; // 2nd
    case 2:
      return '★★★☆☆'; // 3rd
    default:
      return '★★★☆☆'; // others
  }
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