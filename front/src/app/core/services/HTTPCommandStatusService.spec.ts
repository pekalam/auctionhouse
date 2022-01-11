import { TestBed, inject } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HTTPCommandStatusService, INTERVAL_SEC, MAX_RETRY } from './HTTPCommandStatusService';
import { HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';

describe('HttpcommandStatusService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule
      ],
      providers: [HTTPCommandStatusService]
    });

    httpMock = TestBed.get(HttpTestingController);
    jasmine.clock().install();
  });

  afterEach(() => {
    jasmine.clock().uninstall();
  });

  it('should be created', inject([HTTPCommandStatusService], (service: HTTPCommandStatusService) => {
    expect(service).toBeTruthy();
  }));

  it('setupServerMessage should retry sending requests to server in case of 404 and return null error after max retry',
    inject([HTTPCommandStatusService], (service: HTTPCommandStatusService) => {
      service.setupServerMessageHandler('123').subscribe((status) => {
        fail();
      }, (err) => {
        expect(err).toBeNull();
      });
      for (let i = 0; i < MAX_RETRY; i++) {
        jasmine.clock().tick(INTERVAL_SEC * 300);
        const req = httpMock.match('/api/c/command/123');
        req.forEach(r => {
          r.error(new ErrorEvent('test'), {
            status: 404
          });
        });
        httpMock.verify();
        expect(req.length).toBe(1);
      }

      jasmine.clock().tick(INTERVAL_SEC * 300);
      const req = httpMock.match('/api/c/command/123');
      req.forEach(r => {
        r.error(new ErrorEvent('test'), {
          status: 404
        });
      });
      httpMock.verify();
      expect(req.length).toBe(0);
    }));

  it('setupServerMessage should retry sending requests to server in case of pending response and return null error after max retry',
    inject([HTTPCommandStatusService], (service: HTTPCommandStatusService) => {
      service.setupServerMessageHandler('123').subscribe((status) => {
        fail();
      }, (err) => {
        expect(err).toBeNull();
      });
      for (let i = 0; i < MAX_RETRY; i++) {
        jasmine.clock().tick(INTERVAL_SEC * 300);
        const req = httpMock.match('/api/c/command/123');
        req.forEach(r => {
          r.event(new HttpResponse({body: {correlationId: '123', status: 'PENDING', values: null}, status: 200}));
        });
        httpMock.verify();
        expect(req.length).toBe(1);
      }

      jasmine.clock().tick(INTERVAL_SEC * 300);
      const req = httpMock.match('/api/c/command/123');
      req.forEach(r => {
        r.error(new ErrorEvent('test'), {
          status: 404
        });
      });
      httpMock.verify();
      expect(req.length).toBe(0);
    }));

  it('setupServerMessage should return error in case of http error',
    inject([HTTPCommandStatusService], (service: HTTPCommandStatusService) => {
      service.setupServerMessageHandler('123').subscribe((status) => {
        fail();
      }, (err: HttpErrorResponse) => {
        expect(err.status).toBe(500);
      });
      jasmine.clock().tick(INTERVAL_SEC - 50);
      const req = httpMock.match('/api/c/command/123');
      req.forEach((r) => {
        r.error(new ErrorEvent('test'), {
          status: 500
        });
      });

      httpMock.verify();
      expect(req.length).toBe(1);
    }));
});
