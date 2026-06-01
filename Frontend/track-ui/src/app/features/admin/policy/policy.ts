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
  selector: 'app-admin-policy',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './policy.html',
  styleUrl: './policy.css'
})
export class AdminPolicy implements OnInit {

  messages: PolicyMessage[] = [];
  query = '';
  isLoading = false;
  isUploading = false;
  uploadSuccess = false;
  currentUser: User | null = null;
  selectedFile: File | null = null;

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {

    this.currentUser = this.authService.getCurrentUser();

    this.messages.push({
      role: 'assistant',
      text: 'Hi! You can upload policy documents and ask questions about them. What would you like to do?'
    });
  }

  onFileSelected(event: Event): void {

    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {

      this.selectedFile = input.files[0];
      this.uploadSuccess = false;

      console.log('File selected:', this.selectedFile.name);

      this.cdr.detectChanges();
    }
  }

  uploadDocument(): void {

    if (!this.selectedFile) return;

    console.log('Uploading:', this.selectedFile.name);

    this.isUploading = true;

    this.cdr.detectChanges();

    this.apiService.uploadPolicyDocument(this.selectedFile).subscribe({

      next: (response) => {

        console.log('Upload success:', response);

        this.isUploading = false;
        this.uploadSuccess = true;
        this.selectedFile = null;

        this.cdr.detectChanges();
      },

      error: (err) => {

        console.log('Upload error:', err);

        this.isUploading = false;

        this.cdr.detectChanges();

        alert('Failed to upload document.');
      }
    });
  }

  askQuestion(): void {

    if (!this.query.trim() || this.isLoading) return;

    const userMessage = this.query.trim();

    this.messages = [
      ...this.messages,
      {
        role: 'user',
        text: userMessage
      }
    ];

    this.query = '';
    this.isLoading = true;

    this.cdr.detectChanges();

    this.apiService.askPolicy(userMessage).subscribe({

      next: (response: string) => {

        console.log('Policy response:', response);

        const formattedResponse = response
          .replace(/📌/g, '\n📌')
          .replace(/⚠️/g, '\n⚠️');

        this.messages = [
          ...this.messages,
          {
            role: 'assistant',
            text: formattedResponse
          }
        ];

        this.isLoading = false;

        this.cdr.detectChanges();
      },

      error: (err) => {

        console.log('Policy error:', err);

        this.messages = [
          ...this.messages,
          {
            role: 'assistant',
            text: 'Sorry, I could not process your question. Please try again.'
          }
        ];

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

    this.router.navigate(['/admin/tickets']);
  }
}