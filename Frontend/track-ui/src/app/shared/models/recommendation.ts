export interface RecommendationResult {
  product: string;
  confidence: number;
  similarity: number;
  finalScore: number;
  reason: string;
}