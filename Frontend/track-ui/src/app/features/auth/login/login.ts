import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  loginForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;
    this.isLoading = true;

    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
          window.localStorage.setItem("auth_token", response.accessToken); // 🔥 ADD THIS
        this.isLoading = false;
        switch (response.user.role) {  // ✅ response.user.role not response.role
          case 'Customer':
            this.router.navigate(['/customer/products']);
            break;
          case 'SupportAgent':
            this.router.navigate(['/staff/tickets']);
            break;
          case 'Admin':
            this.router.navigate(['/admin/tickets']);
            break;
          default:
            this.router.navigate(['/login']);
        }
      },
      error: () => {
        this.isLoading = false;
        alert('Login failed. Please check your credentials.');
      }
    });
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}