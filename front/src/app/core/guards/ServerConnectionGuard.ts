import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { WSCommandStatusService } from '../services/WSCommandStatusService';
import { first, tap } from 'rxjs/operators';
import { LoadingService } from '../services/LoadingService';


@Injectable({
  providedIn: 'root',
})
export class ServerConnectionGuard implements CanActivate {
  constructor(private serverMessageService: WSCommandStatusService, private router: Router, private loadingService: LoadingService) { }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {

    return new Promise<boolean>((resolve, reject) => {
      this.serverMessageService.connectionStarted.pipe(
        first()
      ).subscribe((connected) => {
        if (!connected) {
          this.loadingService.resetLoading();
          this.router.navigateByUrl('/error');
          reject(false);
        } else {
          this.loadingService.setLoading(false);
          resolve(true);
        }
      }, (err) => {
        this.loadingService.resetLoading();
        this.router.navigateByUrl('/error');
        reject(false);
      });
      this.loadingService.setLoading(true);
      try {
        this.serverMessageService.ensureConnected();        
      } catch (error) {
        this.loadingService.setLoading(false);
      }
    });
  }


}
