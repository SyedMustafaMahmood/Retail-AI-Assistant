import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Ticket, TicketRequest, TicketSummaryResponse } from '../../shared/models/ticket';
import { Product, RecommendationRequest } from '../../shared/models/product';
import { RecommendationResult } from '../../shared/models/recommendation';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // ─── Tickets ─────────────────────────────────────────

  // Customer: create ticket
  createTicket(request: TicketRequest): Observable<Ticket> {
    return this.http.post<Ticket>(`${this.base}/tickets`, request);
  }

  // Staff + Admin: get all tickets
  getAllTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.base}/tickets`);
  }

  // Customer: get own tickets
  getMyTickets(): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.base}/tickets/my`);
  }

  // Staff + Admin: summarize ticket
  summarizeTicket(id: number): Observable<TicketSummaryResponse> {
    return this.http.post<TicketSummaryResponse>(
      `${this.base}/tickets/${id}/summarize`, {}
    );
  }

  // Staff + Admin: filter by status
  getTicketsByStatus(status: string): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.base}/tickets/status/${status}`);
  }

  // Admin: delete ticket
  deleteTicket(id: number): Observable<string> {
    return this.http.delete(`${this.base}/tickets/${id}`, { responseType: 'text' });
  }

  updateTicketStatus(id: number, status: string): Observable<string> {
  return this.http.patch(
    `${this.base}/tickets/${id}/status`,
    { status },
    { responseType: 'text' }
  );
}

  // ─── Products ────────────────────────────────────────

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.base}/products`);
  }

  getRecommendations(request: RecommendationRequest): Observable<RecommendationResult[]> {
    return this.http.post<RecommendationResult[]>(`${this.base}/recommend`, request);
  }

  // ─── Policy ──────────────────────────────────────────

  askPolicy(query: string): Observable<string> {
  return this.http.post<{answer: string}>(
    `${this.base}/policy/ask`,
    { query }
  ).pipe(
    map(response => response.answer)
  );
}

  uploadPolicyDocument(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(
      `${this.base}/policy/upload`,
      formData,
      { responseType: 'text' }
    );
  }
}