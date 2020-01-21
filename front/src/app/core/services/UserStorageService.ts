import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserStorageService {

  private cache: Map<string, any> = new Map<string, any>();

  constructor() { }

  getData(userId: string): any {
    if (this.cache.has(userId)) {
      return this.cache.get(userId);
    }
    let data = localStorage.getItem(userId);
    if (!data) {
      return null;
    }
    let dataObj = JSON.parse(data);
    this.cache.set(userId, dataObj);
    return dataObj;
  }

  setData(userId: string, data: any) {
    let json = JSON.stringify(data);
    localStorage.setItem(userId, json);
    this.cache.set(userId, data);
  }
}
