import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-server-error-page',
  templateUrl: './server-error-page.component.html',
  styleUrls: ['./server-error-page.component.scss']
})
export class ServerErrorPageComponent implements OnInit {

  msg = null;

  constructor(private router: Router) {
      if (this.router.getCurrentNavigation().extras.state) {
        this.msg = this.router.getCurrentNavigation().extras.state.msg;
      }
   }

  ngOnInit() {
  }

}
