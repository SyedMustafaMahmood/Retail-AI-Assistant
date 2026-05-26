import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { Ticket, TicketSummaryResponse } from '../../../shared/models/ticket';
import { User } from '../../../shared/models/user';

@Component({
  selector: 'app-staff-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tickets.html',
  styleUrl: './tickets.css'
})
export class StaffTickets implements OnInit {
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
  searchQuery = ''; // ← ADD THIS
  successMessage: string = '';  // ✅ ADD HERE

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef  // ✅
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
  const query = this.searchQuery.trim().toLowerCase();

  if (query) {
    const isNumber = !isNaN(Number(query));

    if (isNumber) {
      // query is a number → call GetTicketById
      this.apiService.getTicketById(Number(query)).subscribe({
        next: (ticket) => {
          this.filteredTickets = ticket ? [ticket] : [];
          this.cdr.detectChanges();
        },
        error: () => {
          this.filteredTickets = [];
          this.cdr.detectChanges();
        }
      });
    } else {
      // query is text → call SearchTickets by name
      this.apiService.searchTickets(query).subscribe({
        next: (tickets) => {
          this.filteredTickets = tickets;
          this.cdr.detectChanges();
        },
        error: () => alert('Failed to search tickets.')
      });
    }
  } else {
    // no query → normal filter
    this.filteredTickets = this.filterStatus === 'All'
      ? this.tickets
      : this.tickets.filter(t => t.status === this.filterStatus);
    this.cdr.detectChanges();
  }
}

  setFilter(status: string): void {
  this.filterStatus = status;
  if (status === 'All') {
    this.apiService.getAllTickets().subscribe({
      next: (tickets) => {
        this.filteredTickets = tickets;
        this.cdr.detectChanges();
      },
      error: () => alert('Failed to load tickets.')
    });
  } else {
    this.apiService.getTicketsByStatus(status).subscribe({
      next: (tickets) => {
        this.filteredTickets = tickets;
        this.cdr.detectChanges();
      },
      error: () => alert('Failed to load tickets.')
    });
  }
}

  selectTicket(ticket: Ticket): void {
    this.selectedTicket = ticket;
    this.summaryText = "";
    this.cdr.detectChanges(); // ✅
  }
  clearSearch(): void {
  this.searchQuery = '';
  this.applyFilter();
}
  async summarize(): Promise<void> {
  if (!this.selectedTicket) return;

  this.isSummarizing = true;
  this.summaryText = "";
  this.cdr.detectChanges();

  try {
    await this.apiService.summarizeTicketStream(
      this.selectedTicket.id,
      (chunk: string) => {
          
  chunk = chunk
    .replace(/🧩/g, '\n🧩')
    .replace(/✅/g, '\n✅');

  this.summaryText += chunk;
        this.cdr.detectChanges();
      }
    );

    this.isSummarizing = false;
    this.cdr.detectChanges();
  }
  catch (err) {
    console.log('Summarize error:', err);

    this.isSummarizing = false;
    this.cdr.detectChanges();

    alert('Failed to summarize ticket.');
  }
}
isEligibleForDeletion(ticket: any): boolean {
  const daysDiff = (new Date().getTime() - new Date(ticket.createdAt).getTime())
                   / (1000 * 60 * 60 * 24);
  return ticket.status === 'Resolved' && daysDiff > 10;
}

deleteTicket(ticket: any): void {
  if (!confirm(`Are you sure you want to delete ticket #${ticket.id}?`)) return;
  
  this.apiService.deleteTicket(ticket.id).subscribe({
    next: () => {
      this.tickets = this.tickets.filter(t => t.id !== ticket.id);
      this.applyFilter();
      if (this.selectedTicket?.id === ticket.id) {
        this.selectedTicket = null;
      }
     this.successMessage = `Ticket #${ticket.id} deleted successfully.`;
      this.cdr.detectChanges();
      setTimeout(() => {
        this.successMessage = '';
        this.cdr.detectChanges();
      }, 3000); // disappears after 3 seconds
    },
    error: (err) => {
      alert('Failed to delete ticket.');
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
      case 'Reviewed': return 'status-resolved'; // ✅
      default: return '';
    }
  }

  logout(): void {
    this.authService.logout();
  }

  goToPolicy(): void {
    this.router.navigate(['/staff/policy']);
  }

}