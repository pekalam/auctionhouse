import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../core/auth/AuthGuard';
import { NotAuthenticatedComponent } from './pages/not-authenticated/not-authenticated.component';
import { ServerErrorPageComponent } from './pages/server-error-page/server-error-page.component';
import { NotFoundPageComponent } from './pages/not-found-page/not-found-page.component';

const routes: Routes = [
  { path: 'error', component: ServerErrorPageComponent },
  { path: 'not-authenticated', component: NotAuthenticatedComponent },
  { path: 'not-found', component: NotFoundPageComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ErrorRoutingModule { }
