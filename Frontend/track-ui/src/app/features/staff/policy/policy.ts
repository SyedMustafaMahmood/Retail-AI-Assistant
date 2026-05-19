import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { User } from '../../../shared/models/user';

export interface PolicyMessage {
  role: 'user' | 'assistant';
  text: string;
}

@Component({
  selector: 'app-staff-policy',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './policy.html',
  styleUrl: './policy.css'
})
export class StaffPolicy implements OnInit {
  messages: PolicyMessage[] = [];
  query = '';
  isLoading = false;
  currentUser: User | null = null;

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef  // ✅
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.messages = [{
      role: 'assistant',
      text: 'Hi! I can help you with both customer-facing and internal policy questions. What would you like to know?'
    }];
  }

  askQuestion(): void {
    if (!this.query.trim() || this.isLoading) return;

    const userMessage = this.query.trim();
    console.log('Asking:', userMessage);

    this.messages = [...this.messages, { role: 'user', text: userMessage }];
    this.query = '';
    this.isLoading = true;
    this.cdr.detectChanges();

    this.apiService.askPolicy(userMessage).subscribe({
      next: (response) => {
        console.log('Policy response:', response);
        this.messages = [...this.messages, { role: 'assistant', text: response }];
        this.isLoading = false;
        this.cdr.detectChanges(); // ✅
      },
      error: (err) => {
        console.log('Policy error:', err);
        this.messages = [...this.messages, {
          role: 'assistant',
          text: 'Sorry, I could not process your question. Please try again.'
        }];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.askQuestion();
    }
  }

  logout(): void {
    this.authService.logout();
  }

  goToTickets(): void {
    this.router.navigate(['/staff/tickets']);
  }
}