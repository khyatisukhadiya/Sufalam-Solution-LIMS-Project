import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TestapprovalService {

  constructor(private http : HttpClient) { }

  getTestApprovalList(sampleRegisterId : number) {
    return this.http.get<any[]>('https://localhost:7161/api/TestResult/GetTestResultById?SampleRegisterId='+ sampleRegisterId);
  }
}
