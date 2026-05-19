export interface User {
  id: number;
  name: string;
  email: string;
  role: 'Customer' | 'SupportAgent' | 'Admin';
}