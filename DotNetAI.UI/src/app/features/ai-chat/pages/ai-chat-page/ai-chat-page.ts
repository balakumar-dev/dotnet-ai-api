import { AfterViewInit, Component, effect, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

import { normalizeHttpError } from '../../../../core/error/error-normalizer';
import { ChatApiService } from '../../data-access/chat-api.service';
import { ChatMessage, TopicAnalysis } from '../../models/chat.model';

@Component({
  selector: 'app-ai-chat-page',
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatIconModule,
  ],
  templateUrl: './ai-chat-page.html',
  styleUrl: './ai-chat-page.scss',
})
export class AiChatPage implements AfterViewInit {
  private readonly api = inject(ChatApiService);

  @ViewChild('conversationEl') private conversationRef!: ElementRef<HTMLDivElement>;
  @ViewChild('textareaEl') private textareaRef!: ElementRef<HTMLTextAreaElement>;

  readonly messages = signal<ChatMessage[]>([]);
  readonly analysis = signal<TopicAnalysis | null>(null);
  readonly busy = signal(false);
  readonly error = signal('');
  message = '';
  mode = 'chat';

  readonly suggestions = [
    'Explain dependency injection in .NET',
    'What is RAG and how does it work?',
    'How do I use Entity Framework Core?',
    'What are the SOLID principles?',
  ];

  constructor() {
    // Auto-scroll whenever messages list or busy state changes
    effect(() => {
      this.messages();
      this.busy();
      this.scrollToBottom();
    });
  }

  ngAfterViewInit(): void {
    this.textareaRef?.nativeElement.focus();
  }

  send(): void {
    const message = this.message.trim();
    if (!message || this.busy()) {
      return;
    }

    this.error.set('');
    this.messages.update((items) => [...items, { role: 'user', content: message }]);
    this.message = '';
    this.resetTextareaHeight();
    this.busy.set(true);

    const request = { message };
    const response$ =
      this.mode === 'knowledge'
        ? this.api.knowledgeChat(request)
        : this.api.chat(request);

    response$
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (result) =>
          this.messages.update((items) => [
            ...items,
            { role: 'assistant', content: result.response },
          ]),
        error: (error) => this.error.set(normalizeHttpError(error).message),
      });
  }

  analyze(): void {
    const message = this.message.trim();
    if (!message || this.busy()) {
      return;
    }

    this.busy.set(true);
    this.error.set('');
    this.api
      .analyze({ message })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (result) => this.analysis.set(result),
        error: (error) => this.error.set(normalizeHttpError(error).message),
      });
  }

  newChat(): void {
    if (this.busy()) {
      return;
    }
    this.messages.set([]);
    this.analysis.set(null);
    this.error.set('');
    this.message = '';
    this.resetTextareaHeight();
    this.textareaRef?.nativeElement.focus();
  }

  useSuggestion(text: string): void {
    this.message = text;
    const ta = this.textareaRef?.nativeElement;
    if (ta) {
      ta.focus();
      // Resize to fit the pre-filled text
      ta.style.height = 'auto';
      ta.style.height = Math.min(ta.scrollHeight, 200) + 'px';
    }
  }

  onTextareaInput(event: Event): void {
    const ta = event.target as HTMLTextAreaElement;
    ta.style.height = 'auto';
    ta.style.height = Math.min(ta.scrollHeight, 200) + 'px';
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const el = this.conversationRef?.nativeElement;
      if (el) {
        el.scrollTop = el.scrollHeight;
      }
    }, 0);
  }

  private resetTextareaHeight(): void {
    const ta = this.textareaRef?.nativeElement;
    if (ta) {
      ta.style.height = 'auto';
    }
  }
}
