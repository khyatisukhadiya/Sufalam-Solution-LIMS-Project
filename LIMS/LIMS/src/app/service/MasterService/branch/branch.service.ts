import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BranchService {

  constructor(private http: HttpClient) { }

  addUpdatedBranch(data: any) {
    return this.http.post<any>('https://localhost:7161/api/Branch/AddUpdatedBranch', data);
  }

  fetchBranch(filter: any) {
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

    return this.http.get<any>('https://localhost:7161/api/Branch/GetBranchesByFilter', { params });
  }

  deleteBranch(branchId: number) {
    return this.http.delete('https://localhost:7161/api/Branch/DeleteBranchById?branchId=' + branchId);
  }

  getBranchById(branchId: number) {
    return this.http.get<any>('https://localhost:7161/api/Branch/GetBranchById?BranchId=' + branchId);
  }
}
