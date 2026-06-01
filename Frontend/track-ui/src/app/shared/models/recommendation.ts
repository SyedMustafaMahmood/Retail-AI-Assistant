export interface RecommendationResult {
  id: number;
  product: string;
  confidence: number;
  similarity: number;
  finalScore: number;
  reason: string;
  imageUrl: string;
  Description: string;
}