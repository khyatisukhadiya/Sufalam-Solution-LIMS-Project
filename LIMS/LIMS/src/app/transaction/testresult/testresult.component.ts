import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { SliderbarComponent } from '../../component/sliderbar/sliderbar.component';
import { RouterModule } from '@angular/router';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TestresultService } from '../../service/TransactionService/testresult/testresult.service';
import { TestService } from '../../service/MasterService/test/test.service';
import { testModal } from '../../modal/MasterModel/testModal';
import { serviceModal } from '../../modal/MasterModel/serviceModal';
import { Modal } from 'bootstrap';
import { SampleRegister } from '../../modal/Transaction/sampleRegister';

@Component({
  selector: 'app-testresult',
  imports: [CommonModule, SliderbarComponent, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './testresult.component.html',
  styleUrl: './testresult.component.css'
})
export class TestresultComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  @ViewChild('autofocus') autofocus: ElementRef | undefined;
  searchCriteria = { id: '', name: '', code: '' }
  testresultForm: FormGroup = new FormGroup({});
  testresultList: testModal[] = [];
  selectedSample: any = null;
  services: serviceModal[] = [];
  filteredServices: serviceModal[] = [];
  serviceNameFilter: string = '';
  filteredTests: any[] = [];
  selectedTest : any[] = [];
  submitted: boolean = false;
  validationErrors: string[] = [];
  modalInstance: any;
  sampleRegisterList: SampleRegister[] = [];
  searchClick: boolean = false;
  selectedSampleRegisterId: string | null = null;
  sampleRegisterServices: any[] = [];
  selectedServices : any[] = [];

  testresultService = inject(TestresultService)

  ngOnInit(): void {
    this.loadServices();
  }

  loadServices() {
    this.testresultService.getServices().subscribe({
      next: (res) => {
        this.services = res || [];
        // this.selectedTest = this.services.filter(service => (service.test || []).map((test: any) => test.testName)) || [];
        // console.log('test',this.selectedTest);
      },
      error: (err) => {
        console.error('Error loading services:', err);
      }
    });
  }
  


  closeModal() {
    // (document.activeElement as HTMLElement)?.blur();

    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    if (this.modal != null) {
      this.modal.nativeElement.style.display = "none";
    }
    this.submitted = false;
  }

  getSampleregister() {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.name?.trim() || '',
      id: this.searchCriteria.id || null,
      phoneNumber: this.searchCriteria.code
    };

    this.testresultService.getSampleregister(filter).subscribe({
      next: (res) => {
        this.sampleRegisterList = res.data || [];
      },
    })
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      id: '',
      name: '',
      code: '',
    }
  }

  openModal(sampleRegisterId : number, event: Event) {
    event.preventDefault();
    this.selectSample(sampleRegisterId);
  }

 selectSample(sampleRegisterId : number): void {
    this.testresultService.getSampleRegisterById(sampleRegisterId).subscribe({
      next: async (res) =>
        {
          console.log('API response:', res);
          console.log('Response keys:', Object.keys(res));
          this.selectedSample = res || null;
            this.selectedServices = (this.selectedSample?.serviceMapping || []).map((service: any) => ({
            ...service,
            tests: this.services.find(s => s.serviceId === service.serviceId)?.test || []
            }));
            
          console.log('selectedSample:', this.selectedSample);
          console.log('selectedServices with tests:', this.selectedServices);
        },
    })
  }
}
