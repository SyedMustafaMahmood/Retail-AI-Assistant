export interface Product {
  id: number;
  name: string;
  description: string;
}

export interface RecommendationRequest {
  products: string[];
}