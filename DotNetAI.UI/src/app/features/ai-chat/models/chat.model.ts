export interface ChatRequest {
  message: string;
}

export interface ChatResponse {
  response: string;
}

export interface TopicAnalysis {
  summary: string;
  technologies: string[];
  difficulty: string;
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}
