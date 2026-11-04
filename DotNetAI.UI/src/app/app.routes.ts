import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'knowledge',
    pathMatch: 'full'
  },
  {
    path: 'knowledge',
    loadComponent: () =>
      import('./feature/knowledge/pages/knowledge-page/knowledge-page.component')
        .then(component => component.KnowledgePageComponent)
  },
  {
    path: '**',
    redirectTo: 'knowledge'
  }
];
