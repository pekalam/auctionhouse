import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotAuthenticatedComponent } from './pages/not-authenticated/not-authenticated.component';
import { ErrorRoutingModule } from './error-routing.module';
import { RouterModule } from '@angular/router';
import { MaterialModule } from '../material.module';
import { ServerErrorPageComponent } from './pages/server-error-page/server-error-page.component';



@NgModule({
  declarations: [NotAuthenticatedComponent, ServerErrorPageComponent],
  imports: [
    ErrorRoutingModule,
    CommonModule,
    RouterModule,
    MaterialModule,
  ],
  exports: [NotAuthenticatedComponent]
})
export class ErrorModule { }
