export interface KnowledgeDocument {
  documentId: string;
  fileName: string;
  chunkCount: number;
}

export interface DocumentIngestionResult extends KnowledgeDocument {
  createdDate: string;
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
