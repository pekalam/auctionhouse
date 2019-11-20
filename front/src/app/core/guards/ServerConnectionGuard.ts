import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { ServerMessageService } from '../services/ServerMessageService';
import { first } from 'rxjs/operators';


@Injectable({
  providedIn: 'root',
})
export class ServerConnectionGuard implements CanActivate {
  constructor(private serverMessageService: ServerMessageService, private router: Router) { }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {

    return new Promise<boolean>((resolve, reject ) => {
      this.serverMessageService.connectionStarted.pipe(
        first()
      ).subscribe((connected) => {
        if (!connected) {
          this.router.navigateByUrl('/error');
          reject(false);
        }
        resolve(true);
      }, (err) => reject(err));
      this.serverMessageService.ensureConnected();
    });
  }


}
