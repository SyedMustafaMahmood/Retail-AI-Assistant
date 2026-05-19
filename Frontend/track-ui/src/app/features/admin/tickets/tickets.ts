import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { Ticket, TicketSummaryResponse } from '../../../shared/models/ticket';
import { User } from '../../../shared/models/user';

@Component({
  selector: 'app-admin-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tickets.html',
  styleUrl: './tickets.css'
})
export class AdminTickets implements OnInit {
  tickets: Ticket[] = [];
  filteredTickets: Ticket[] = [];
  selectedTicket: Ticket | null = null;
  summary: TicketSummaryResponse | null = null;
  currentUser: User | null = null;
  isLoadingTickets = false;
  isSummarizing = false;
  isUpdatingStatus = false;
  filterStatus = 'All';
  statuses = ['All', 'Open', 'InProgress', 'Resolved'];

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef  // ✅ Added
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoadingTickets = true;
    this.apiService.getAllTickets().subscribe({
      next: (tickets) => {
        this.tickets = tickets;
        this.applyFilter();
        this.isLoadingTickets = false;
        this.cdr.detectChanges(); // ✅
      },
      error: (err) => {
        console.log('Error loading tickets:', err);
        this.isLoadingTickets = false;
        this.cdr.detectChanges();
        alert('Failed to load tickets.');
      }
    });
  }

  applyFilter(): void {
    this.filteredTickets = this.filterStatus === 'All'
      ? this.tickets
      : this.tickets.filter(t => t.status === this.filterStatus);
    this.cdr.detectChanges(); // ✅
  }

  setFilter(status: string): void {
    this.filterStatus = status;

    if (status === 'All') {
      this.apiService.getAllTickets().subscribe({
        next: (tickets) => {
          this.filteredTickets = tickets;
          this.cdr.detectChanges(); // ✅
        },
        error: () => alert('Failed to load tickets.')
      });
    } else {
      this.apiService.getTicketsByStatus(status).subscribe({
        next: (tickets) => {
          this.filteredTickets = tickets;
          this.cdr.detectChanges(); // ✅
        },
        error: () => alert('Failed to load tickets.')
      });
    }
  }

  selectTicket(ticket: Ticket): void {
    this.selectedTicket = ticket;
    this.summary = null;
    this.cdr.detectChanges(); // ✅
  }

  summarize(): void {
    if (!this.selectedTicket) return;
    this.isSummarizing = true;

    this.apiService.summarizeTicket(this.selectedTicket.id).subscribe({
      next: (response) => {
        this.summary = response;
        this.isSummarizing = false;
        this.cdr.detectChanges(); // ✅
      },
      error: (err) => {
        console.log('Summarize error:', err);
        this.isSummarizing = false;
        this.cdr.detectChanges();
        alert('Failed to summarize ticket.');
      }
    });
  }

  updateStatus(status: string): void {
    if (!this.selectedTicket) return;
    this.isUpdatingStatus = true;

    this.apiService.updateTicketStatus(this.selectedTicket.id, status).subscribe({
      next: () => {
        this.selectedTicket!.status = status as Ticket['status'];
        this.isUpdatingStatus = false;
        this.cdr.detectChanges(); // ✅
        this.loadTickets();
      },
      error: (err) => {
        console.log('Update status error:', err);
        this.isUpdatingStatus = false;
        this.cdr.detectChanges();
        alert('Failed to update status.');
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Open': return 'status-open';
      case 'InProgress': return 'status-progress';
      case 'Resolved': return 'status-resolved';
      case 'Reviewed': return 'status-resolved'; // ✅ Added
      default: return '';
    }
  }

  logout(): void {
    this.authService.logout();
  }

  goToPolicy(): void {
    this.router.navigate(['/admin/policy']);
  }
}