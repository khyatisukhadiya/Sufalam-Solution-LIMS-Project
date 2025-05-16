import { Component, ElementRef, ViewChild } from '@angular/core';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Modal } from 'bootstrap';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sample-register',
  imports: [SliderbarComponent,RouterModule,CommonModule,ReactiveFormsModule],
  templateUrl: './sample-register.component.html',
  styleUrl: './sample-register.component.css'
})
export class SampleRegisterComponent {



   @ViewChild('myModal') modal: ElementRef | undefined;
  sampleRegisterForm: FormGroup = new FormGroup({});
  isEditModal: boolean = false;
  modalInstance: any;
  searchClick: boolean = false;
submitted: boolean = false;

  // searchCriteria: { serviceId: string; serviceName: string; serviceCode: string; isActive: boolean; };



  openModal() {
      // this.isEditModal = false;
      const modalEl = document.getElementById('myModal');
  
      if (modalEl != null) {
        modalEl.style.display = "block";
      }
  
      if (this.modal?.nativeElement) {
        this.modalInstance = new Modal(this.modal.nativeElement, {
          backdrop: 'static',
          keyboard: false
        });
        this.modalInstance.show();
      }
    }
  
    closeModal() {
      this.isEditModal = false;
      (document.activeElement as HTMLElement)?.blur(); // Fix aria-hidden warning
  
      if (this.modalInstance) {
        this.modalInstance.hide();
      }
  
      // this.errorMessage = '';
      // this.validationErrors = [];
      // this.submitted = false;
      // this.selectedTests = [];
  
      // this.serviceForm.reset({
      //   ServiceId: 0,
      //   ServiceName: '',
      //   ServiceCode: '',
      //   B2BAmount: 0,
      //   B2CAmount: 0,
      //   isActive: true
      // });
    }
  
     clearSearch() {
    this.searchClick = false;
  //   this.searchCriteria = {
  //     sampleRegisterId: '',
  //     sampleRegisterName: '',
  //     Code: '',
  //     isActive: true
  //   };
  // }
}
}
