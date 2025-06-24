import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TestapprovalService {

  constructor(private http: HttpClient) { }

  getTestApprovalList(sampleRegisterId: number) {
    return this.http.get<any[]>('https://localhost:7161/api/TestResult/GetTestResultById?SampleRegisterId=' + sampleRegisterId);
  }

  getSampleregister(filter: any) {
    let params = new HttpParams();

    if (filter.name) {
      params = params.set('name', filter.name);
    }

    if (filter.phoneNumber) {
      params = params.set('code', filter.phoneNumber);
    }

    if (filter.id !== null && filter.id !== undefined) {
      params = params.set('id', filter.id.toString());
    }

    if (filter.isActive !== null && filter.isActive !== undefined) {
      params = params.set('isActive', filter.isActive.toString());
    }
    return this.http.get<any>('https://localhost:7161/api/SampleRegister/GetSampleByFilter', { params });
  }


  getServices() {
    return this.http.get<any>('https://localhost:7161/api/Service/GetServiceIsActive');
  }

  getTestApprovalById(sampleRegisterId: number) {
    return this.http.get<any>('https://localhost:7161/api/TestResult/GetTestResultsById?SampleRegisterId=' + sampleRegisterId);
  }

  addUpdatedTestResult(data: any) {
    return this.http.post<any>('https://localhost:7161/api/TestResult/AddUpdateTestResult', data);
  }

  getTestApprovalBySampleId(sampleRegisterId: number) {
    return this.http.get<any>('https://localhost:7161/api/TestApprovalResult?sampleRegisterId=' + sampleRegisterId);
  }


 sendResportByEmail(toEmail: string, subject: string, body: string, pdfBlob: Blob, fileName: string) {
  const formData = new FormData();
  formData.append("toEmail", toEmail);
  formData.append("subject", subject);
  formData.append("body", body);
  formData.append("attachments", pdfBlob, fileName); 

  return this.http.post<any>('https://localhost:7161/api/Email/SendEmail', formData);
}

sendSMS(toPhoneNumber: string , messageBody: string ){
  const formDatas = new FormData();
  formDatas.append("toPhoneNumber", toPhoneNumber);
  formDatas.append("messageBody", messageBody);
  return this.http.post<any>('https://localhost:7161/api/SMS/sendSMS', formDatas);
}

}
