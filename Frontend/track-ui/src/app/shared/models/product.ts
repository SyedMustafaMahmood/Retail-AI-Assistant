export interface Product {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
}

export interface RecommendationRequest {
  products: string[];
}