import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class B2BService {

  constructor(private http: HttpClient) { }

  addUpdatedB2B(data: any) {
    return this.http.post<any>('https://localhost:7161/api/B2B/AddUpdatedB2B', data);
  }

  fetchB2B(filter: any) {
    let params = new HttpParams();

    if (filter.name) {
      params = params.set('name', filter.name);
    }

    if (filter.code) {
      params = params.set('code', filter.code);
    }

    if (filter.id !== null && filter.id !== undefined) {
      params = params.set('id', filter.id.toString());
    }

    if (filter.isActive !== null && filter.isActive !== undefined) {
      params = params.set('isActive', filter.isActive.toString());
    }

    return this.http.get<any>('https://localhost:7161/api/B2B/GetB2BByFilter', { params });
  }

  deleteB2B(b2BId: number) {
    return this.http.delete('https://localhost:7161/api/B2B/DeleteB2BById?B2BId=' + b2BId);
  }

  getB2BById(b2BId: number) {
    return this.http.get<any>('https://localhost:7161/api/B2B/GetB2BsById?B2BId=' + b2BId);
  }
}
