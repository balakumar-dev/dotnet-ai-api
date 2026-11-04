import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout').then((m) => m.MainLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/dashboard/pages/dashboard-page/dashboard-page').then(
            (m) => m.DashboardPage,
          ),
      },
      {
        path: 'chat',
        loadComponent: () =>
          import('./features/ai-chat/pages/ai-chat-page/ai-chat-page').then(
            (m) => m.AiChatPage,
          ),
      },
      {
        path: 'knowledge',
        loadComponent: () =>
          import('./features/knowledge-base/pages/knowledge-base-page/knowledge-base-page').then(
            (m) => m.KnowledgeBasePage,
          ),
      },
      {
        path: 'search',
        loadComponent: () =>
          import('./features/semantic-search/pages/semantic-search-page/semantic-search-page').then(
            (m) => m.SemanticSearchPage,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
