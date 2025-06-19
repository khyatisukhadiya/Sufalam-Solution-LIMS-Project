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

  getSampleRegister(){
    return this.http.get<any>('https://localhost:7161/api/SampleRegister/GetSampleByFilter');
  }

    getServices() {
    return this.http.get<any>('https://localhost:7161/api/Service/GetServiceIsActive');
  }

  getTestApprovalById(sampleRegisterId : number) {
    return this.http.get<any>('https://localhost:7161/api/TestResult/GetTestResultsById?SampleRegisterId=' + sampleRegisterId);
  }

    addUpdatedTestResult(data: any) {
    return this.http.post<any>('https://localhost:7161/api/TestResult/AddUpdateTestResult', data);
  }

  getTestApprovalBySampleId(sampleRegisterId: number) {
    return this.http.get<any>('https://localhost:7161/api/TestApprovalResult?sampleRegisterId=' + sampleRegisterId);
  }
}
