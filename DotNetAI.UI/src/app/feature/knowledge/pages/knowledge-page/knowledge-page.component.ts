import { Component, ViewChild } from '@angular/core';
import { KnowledgeUploadComponent } from '../../components/knowledge-upload/knowledge-upload.component';
import { KnowledgeDocumentListComponent } from '../../components/knowledge-document-list/knowledge-document-list.component';
import { KnowledgeSearchComponent } from '../../components/knowledge-search/knowledge-search.component';
import { DocumentIngestionResult } from '../../models/knowledge.models';

@Component({
  selector: 'app-knowledge-page',
  imports: [
    KnowledgeUploadComponent,
    KnowledgeDocumentListComponent,
    KnowledgeSearchComponent
  ],
  templateUrl: './knowledge-page.component.html',
  styleUrl: './knowledge-page.component.css'
})
export class KnowledgePageComponent {
  @ViewChild(KnowledgeDocumentListComponent)
  private documentList?: KnowledgeDocumentListComponent;

  protected message = '';
  protected error = '';

  protected onUploaded(result: DocumentIngestionResult): void {
    this.error = '';
    this.message = `${result.fileName} uploaded with ${result.chunkCount} chunks.`;
    this.documentList?.refresh();
  }

  protected onFailed(message: string): void {
    this.message = '';
    this.error = message;
  }
}
