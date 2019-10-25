import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatExpansionModule, MatRadioModule, MatMenuModule, MatProgressSpinnerModule, MatTabsModule, MatPaginatorModule, MatDialogModule, MatInputModule, MatFormFieldModule, MatButtonModule, MatListModule, MatSelectModule, NativeDateModule, MatDatepickerModule, MatDialogRef, MAT_DIALOG_DATA, MAT_DATE_FORMATS } from '@angular/material';
import { MatCardModule } from '@angular/material/card';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSliderModule } from '@angular/material/slider';

export const MY_FORMATS = {
  parse: {
    dateInput: 'DD/MM/YYYY'
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM-YYYY'
  }
};

@NgModule({
  declarations: [],
  exports: [
    MatTableModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatPaginatorModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatListModule,
    MatRadioModule,
    MatSelectModule,
    MatMenuModule,
    NativeDateModule,
    MatDatepickerModule,
    MatCardModule,
    MatTabsModule,
    MatSliderModule,
    MatExpansionModule
  ],
  providers: [
    { provide: MatDialogRef, useValue: {} },
    { provide: MAT_DIALOG_DATA, useValue: [] },
    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS }
  ]
})
export class MaterialModule { }
