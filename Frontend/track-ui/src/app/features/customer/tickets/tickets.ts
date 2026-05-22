import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { Ticket, TicketRequest } from '../../../shared/models/ticket';
import { User } from '../../../shared/models/user';

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tickets.html',
  styleUrl: './tickets.css'
})
export class Tickets implements OnInit {
  tickets: Ticket[] = [];
  currentUser: User | null = null;
  isLoadingTickets = false;
  isSubmitting = false;
  showForm = false;

  newTicket: TicketRequest = {
    customerName: '',
    subject: '',
    description: ''
  };

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef  // ✅ Added
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (this.currentUser) {
      this.newTicket.customerName = this.currentUser.name;
    }
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoadingTickets = true;
    this.apiService.getMyTickets().subscribe({
      next: (tickets) => {
        console.log('Tickets loaded:', tickets);
        this.tickets = tickets;
        this.isLoadingTickets = false;
        this.cdr.detectChanges(); // ✅ Force UI update
      },
      error: (err) => {
        console.log('Error loading tickets:', err);
        this.isLoadingTickets = false;
        this.cdr.detectChanges();
        alert('Failed to load tickets.');
      }
    });
  }

  submitTicket(): void {
    if (!this.newTicket.subject || !this.newTicket.description) return;
    this.isSubmitting = true;

    console.log('Submitting ticket:', this.newTicket);
    console.log('Token:', this.authService.getToken());

    this.apiService.createTicket(this.newTicket).subscribe({
      next: (ticket) => {
        console.log('Ticket created:', ticket);
        this.isSubmitting = false;
        this.showForm = false;
        this.newTicket.subject = '';
        this.newTicket.description = '';
        this.cdr.detectChanges();
        this.loadTickets();
      },
      error: (err) => {
        console.log('Error creating ticket:', err);
        this.isSubmitting = false;
        this.cdr.detectChanges();
        alert('Failed to submit ticket.');
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Open': return 'status-open';
      case 'InProgress': return 'status-progress';
      case 'Resolved': return 'status-resolved';
      case 'Reviewed': return 'status-resolved';
      default: return '';
    }
  }

  logout(): void {
    this.authService.logout();
  }

  goToProducts(): void {
    this.router.navigate(['/customer/products']);
  }

  goToPolicy(): void {
    this.router.navigate(['/customer/policy']);
  }
}