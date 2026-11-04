export interface KnowledgeSearchRequest {
  query: string;
  topK: number;
}

export interface KnowledgeSearchMatch {
  document: string;
  chunkId: string;
  content: string;
  score: number;
}

export interface KnowledgeSearchResponse {
  results: KnowledgeSearchMatch[];
}
