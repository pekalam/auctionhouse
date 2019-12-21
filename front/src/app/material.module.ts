import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatExpansionModule, MatRadioModule, MatMenuModule, MatProgressSpinnerModule, MatTabsModule, MatPaginatorModule,
  MatDialogModule, MatInputModule, MatFormFieldModule, MatButtonModule, MatListModule, MatSelectModule, ErrorStateMatcher, MAT_DATE_LOCALE, MatDatepickerModule, MatCheckboxModule,
  MAT_DATE_FORMATS } from '@angular/material';
import { MatCardModule } from '@angular/material/card';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSliderModule } from '@angular/material/slider';
import { MatTooltipModule } from '@angular/material/tooltip';
import {MAT_MOMENT_DATE_ADAPTER_OPTIONS, MatMomentDateModule} from '@angular/material-moment-adapter';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

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
    MatDatepickerModule,
    MatMomentDateModule,
    MatCardModule,
    MatTabsModule,
    MatSliderModule,
    MatExpansionModule,
    MatTooltipModule,
    MatCheckboxModule,
  ],
  providers: [
    { provide: MatDialogRef, useValue: {} },
    { provide: MAT_DIALOG_DATA, useValue: [] },
    { provide: MAT_DATE_FORMATS, useValue: MY_FORMATS },
/*     { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
    {provide: MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { useUtc: true }} */
  ]
})
export class MaterialModule { }
