import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationComponent } from './navigation/navigation.component';
import { RouterModule } from '@angular/router';
import { CoreModule } from '../core/core.module';
import { MaterialModule } from '../material.module';
import { VisibleDirective } from './visible.directive';
import { SearchBarComponent } from './search-bar/search-bar.component';
import { CarouselComponent } from './carousel/carousel.component';



@NgModule({
  declarations: [NavigationComponent, VisibleDirective, SearchBarComponent, CarouselComponent],
  imports: [
    CoreModule,
    CommonModule,
    MaterialModule,
    RouterModule
  ],
  exports: [NavigationComponent, VisibleDirective, CarouselComponent]
})
export class SharedModule { }
