import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../core/auth/AuthGuard';
import { NotAuthenticatedComponent } from './pages/not-authenticated/not-authenticated.component';
import { ServerErrorPageComponent } from './pages/server-error-page/server-error-page.component';

const routes: Routes = [
  { path: 'error', component: ServerErrorPageComponent },
  { path: 'not-authenticated', component: NotAuthenticatedComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ErrorRoutingModule { }
