export interface RecommendationResult {
  id: number;
  product: string;
  Description: string;
  imageUrl?:string;
  confidence: number;
  similarity: number;
  finalScore: number;
  reason: string;
}