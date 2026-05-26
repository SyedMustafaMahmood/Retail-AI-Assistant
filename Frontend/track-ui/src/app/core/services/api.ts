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
  private baseUrl = 'https://localhost:7073';
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
 async summarizeTicketStream(
  id: number,
  onChunk: (chunk: string) => void
): Promise<void> {

  const token = window.localStorage.getItem("auth_token");

  if (!token) {
    throw new Error("No auth token found in localStorage");
  }

  const response = await fetch(
    `${this.base}/tickets/${id}/summarize-stream`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      }
    }
  );

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  const reader = response.body?.getReader();
  const decoder = new TextDecoder();

  if (!reader) {
    throw new Error("No response body for streaming");
  }

  let buffer = "";
  let words: string[] = [];

  const delay = (ms: number) =>
    new Promise(resolve => setTimeout(resolve, ms));

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });

    // split into words
    words = buffer.split(/\s+/);

    // keep last partial word in buffer
    buffer = words.pop() || "";

    // 🔥 WORD BY WORD ANIMATION
    for (const word of words) {
      onChunk(word + " ");
      await delay(60); // 👈 CONTROL SPEED HERE
    }
  }

  // flush remaining word
  if (buffer.trim().length > 0) {
    onChunk(buffer + " ");
  }
}

  // Staff + Admin: filter by status
  getTicketsByStatus(status: string): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.base}/tickets/status/${status}`);
  }

  // Admin: delete ticket
  deleteTicket(id: number): Observable<string> {
    return this.http.delete(`${this.base}/tickets/${id}`, { responseType: 'text' });
  }

  //Staff:filter by id
  getTicketById(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.base}/tickets/${id}`);
  }
  //staff:filer by customer name
  searchTickets(query: string): Observable<Ticket[]> {
    return this.http.get<Ticket[]>(`${this.base}/tickets/search?query=${query}`);
  }
  // Staff + Admin: update ticket status
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
