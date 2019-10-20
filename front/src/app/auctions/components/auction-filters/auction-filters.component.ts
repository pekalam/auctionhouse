import { Component, OnInit, Input } from '@angular/core';
import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';
import { FormGroup, FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-auction-filters',
  templateUrl: './auction-filters.component.html',
  styleUrls: ['./auction-filters.component.scss']
})
export class AuctionFiltersComponent implements OnInit {

  @Input()
  subcategories: CategoryTreeNode;

  form: FormGroup;

  constructor(private formBuilder: FormBuilder) {
    this.form = formBuilder.group({
      condition: 'all'
    });
  }

  private getConditionFormGroup() {
  }

  ngOnInit() {
  }

}
