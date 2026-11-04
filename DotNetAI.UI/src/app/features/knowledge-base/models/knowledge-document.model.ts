export interface KnowledgeDocument {
  documentId: string;
  fileName: string;
  chunkCount: number;
}

export interface DocumentIngestionResult {
  documentId: string;
  fileName: string;
  chunkCount: number;
  createdDate: string;
}
