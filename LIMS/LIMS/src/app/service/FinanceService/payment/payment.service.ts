import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {

  constructor(private http : HttpClient) { }


  addUpdatePayment(data:any){
    return this.http.post('https://localhost:7161/api/Payment/AddUpdatedPayment',data);
  }

 

   fetchPayment(filter: any) {
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

    return this.http.get<any>('https://localhost:7161/api/Payment/GetPaymentByFilter', { params });
  }


  getPaymentId(paymentId : number){
    return this.http.get('https://localhost:7161/api/Payment/GetPaymentById?PaymentId='+paymentId);
  }

  deletePaymentId(paymentId : number){
    return this.http.delete('https://localhost:7161/api/Payment/DeletePaymentById?PaymentId='+paymentId);
  }

}
