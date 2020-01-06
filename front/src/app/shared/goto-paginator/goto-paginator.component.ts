import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'app-goto-paginator',
  templateUrl: './goto-paginator.component.html',
  styleUrls: ['./goto-paginator.component.scss']
})
export class GotoPaginatorComponent implements OnInit {

  @Output()
  pageChange = new EventEmitter<number>();

  @Input()
  page: number;

  @Input()
  length: number;

  constructor() { }

  ngOnInit() {
  }


  onGotoClick(newPage: number) {
    if (newPage > -1 && newPage * 10 < this.length) {
      this.pageChange.emit(newPage);
    }
  }
}
