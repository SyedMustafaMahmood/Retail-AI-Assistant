import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { Ticket } from '../../../shared/models/ticket';
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

  summaryText: string = "";

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
    private cdr: ChangeDetectorRef
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
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingTickets = false;
        alert('Failed to load tickets.');
      }
    });
  }

  applyFilter(): void {
    this.filteredTickets = this.filterStatus === 'All'
      ? this.tickets
      : this.tickets.filter(t => t.status === this.filterStatus);

    this.cdr.detectChanges();
  }

  setFilter(status: string): void {
    this.filterStatus = status;

    if (status === 'All') {
      this.apiService.getAllTickets().subscribe({
        next: (tickets) => {
          this.filteredTickets = tickets;
          this.cdr.detectChanges();
        }
      });
    } else {
      this.apiService.getTicketsByStatus(status).subscribe({
        next: (tickets) => {
          this.filteredTickets = tickets;
          this.cdr.detectChanges();
        }
      });
    }
  }

  selectTicket(ticket: Ticket): void {
    this.selectedTicket = ticket;

    // reset streaming text
    this.summaryText = "";

    this.cdr.detectChanges();
  }

  // STREAMING SUMMARIZE FUNCTION
  async summarize(): Promise<void> {

    if (!this.selectedTicket) return;

    this.isSummarizing = true;
    this.summaryText = "";
    this.cdr.detectChanges();

    try {
      await this.apiService.summarizeTicketStream(
        this.selectedTicket.id,
        (chunk: string) => {
          this.summaryText += chunk;
          this.cdr.detectChanges();
        }
      );

      this.isSummarizing = false;
      this.cdr.detectChanges();
    }
    catch (err: any) {
  console.error("FULL ERROR:", err);
  console.error("STATUS:", err?.status);
  console.error("MESSAGE:", err?.message);
}
  }

  updateStatus(status: string): void {
    if (!this.selectedTicket) return;

    this.isUpdatingStatus = true;

    this.apiService.updateTicketStatus(this.selectedTicket.id, status)
      .subscribe({
        next: () => {
          this.selectedTicket!.status = status as Ticket['status'];
          this.isUpdatingStatus = false;
          this.loadTickets();
          this.cdr.detectChanges();
        },
        error: () => {
          this.isUpdatingStatus = false;
          alert('Failed to update status.');
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

  goToPolicy(): void {
    this.router.navigate(['/admin/policy']);
  }
}