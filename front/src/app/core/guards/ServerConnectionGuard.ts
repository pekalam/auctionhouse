import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { WSCommandStatusService } from '../services/WSCommandStatusService';
import { first } from 'rxjs/operators';
import { LoadingService } from '../services/LoadingService';


@Injectable({
  providedIn: 'root',
})
export class ServerConnectionGuard implements CanActivate {
  constructor(private serverMessageService: WSCommandStatusService, private router: Router, private loadingService: LoadingService) { }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {

    return new Promise<boolean>((resolve, reject ) => {
      this.serverMessageService.connectionStarted.pipe(
        first()
      ).subscribe((connected) => {
        this.loadingService.setLoading(false);
        if (!connected) {
          this.router.navigateByUrl('/error');
          reject(false);
        }
        resolve(true);
      }, (err) => {
        this.loadingService.setLoading(false);
        reject(err);
      });

      this.loadingService.setLoading(true);
      this.serverMessageService.ensureConnected();
    });
  }


}
