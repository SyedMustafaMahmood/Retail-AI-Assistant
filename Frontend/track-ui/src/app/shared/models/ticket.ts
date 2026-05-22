export interface Ticket {
  id: number;
  customerName: string;
  subject: string;
  description: string;
  status: 'Open' | 'InProgress' | 'Resolved' | 'Reviewed';
  createdAt: string;
}

export interface TicketRequest {
  customerName: string;
  subject: string;
  description: string;
}

export interface TicketSummaryResponse {
  ticketId: number;
  customerName: string;
  subject: string;
  summary: string;
}