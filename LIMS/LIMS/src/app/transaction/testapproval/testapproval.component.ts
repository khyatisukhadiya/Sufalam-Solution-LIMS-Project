import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { testresult } from '../../modal/Transaction/testresult';
import { serviceModal } from '../../modal/MasterModel/serviceModal';
import { SampleRegister } from '../../modal/Transaction/sampleRegister';
import { TestresultService } from '../../service/TransactionService/testresult/testresult.service';
import { ToastrService } from 'ngx-toastr';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { TestapprovalService } from '../../service/TransactionService/testapproval/testapproval.service';
import { Modal } from 'bootstrap';
import { testApproval } from '../../modal/Transaction/testApproval';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { testModal } from '../../modal/MasterModel/testModal';

@Component({
  selector: 'app-testapproval',
  imports: [CommonModule, SliderbarComponent, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './testapproval.component.html',
  styleUrl: './testapproval.component.css'
})
export class TestapprovalComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  @ViewChild('autofocus') autofocus: ElementRef | undefined;
  searchCriteria = { id: '', name: '', code: '' }
  testresultForm: FormGroup = new FormGroup({});
  testresultList: testresult[] = [];
  testapprovalList: testresult[] = [];
  selectedSample: any = null;
  services: serviceModal[] = [];
  filteredServices: serviceModal[] = [];
  serviceNameFilter: string = '';
  filteredTests: any[] = [];
  selectedTest: any[] = [];
  submitted: boolean = false;
  validationErrors: string[] = [];
  modalInstance: any;
  sampleRegisterList: SampleRegister[] = [];
  searchClick: boolean = false;
  selectedSampleRegisterId: string | null = null;
  sampleRegisterServices: any[] = [];
  selectedServices: any[] = [];
  errorMessage: string = '';

  testresultService = inject(TestresultService)
  testapprovalService = inject(TestapprovalService);


  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setFrom();
    this.loadServices();
  }

  loadTestApprovalList(sampleRegisterId: number) {
    this.testapprovalService.getTestApprovalById(sampleRegisterId).subscribe({
      next: (res) => {

        this.testresultList = res || [];
        console.log('Test Approval List response:', res);
        console.log('Test Approval List loaded:', this.testresultList);

        if (!sampleRegisterId) {
          console.error('sampleRegisterId is null or undefined');
          return;
        }

        if (this.testresultList.length === 0) {
          console.warn('No test results found for the given sampleRegisterId:', sampleRegisterId);
          this.selectedServices = [];
          this.testresultForm.reset();
          this.toastr.warning('No test results found for the selected sample.');
          return;
        }

      },
      error: (error) => {
        console.error('Error:', error);
      }
    });
  }

  loadServices() {
    this.testresultService.getServices().subscribe({
      next: (res) => {
        this.services = res || [];
      },
      error: (err) => {
        console.error('Error loading services:', err);
      }
    });
  }

  closeModal() {
    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    if (this.modal != null) {
      this.modal.nativeElement.style.display = "none";
    }

    this.submitted = false;

    this.testresultForm.reset(
      {
        testResultId: 0,
        sampleRegisterId: null,
        serviceId: null,
        testId: null,
        resultValue: '',
        validationStatus: '',
        createdBy: '',
        validateBy: '',
        isActive: true
      }
    );
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

  openModal(sampleRegisterId: number, event: Event) {
    const modal = document.getElementById('myModal');

    if (modal != null) {
      modal.style.display = "block";
    }

    if (this.modal?.nativeElement) {
      this.modalInstance = new Modal(this.modal.nativeElement, {
        backdrop: 'static',
        keyboard: false
      });
      this.modalInstance.show();
    }

    event.preventDefault();
    this.selectSample(sampleRegisterId);
    this.loadTestApprovalList(sampleRegisterId);
  }

  selectSample(sampleRegisterId: number): void {
    this.testresultService.getSampleRegisterById(sampleRegisterId).subscribe({
      next: (res) => {
        console.log('API response:', res);
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


  setFrom() {
    this.testresultForm = this.fb.group({
      testResultId: [{ value: 0, disabled: true }],
      sampleRegisterId: [null, Validators.required],
      serviceId: [null, Validators.required],
      testId: [null, Validators.required],
      resultValue: ['', Validators.required],
      validationStatus: ['', Validators.required],
      createdBy: ['', Validators.required],
      validateBy: ['', Validators.required],
      isActive: [true, Validators.required]
    });
  }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  getrowvalue() {
    const services = this.selectedServices.map(service => ({
      serviceId: service.serviceId,
      serviceName: service.serviceName,
      tests: service.tests.map((test: any) => ({
        testId: test.testId,
        testName: test.testName,
        resultValue: test.resultValue,
        validationStatus: test.validationStatus ? 'A' : 'V',
        createdBy: test.createdBy || '',
        validateBy: test.validateBy || '',
        isActive: test.isActive || true
      }))
    }));

    const formValues = {
      sampleregister: [{
        sampleRegisterId: this.selectedSample?.sampleRegisterId || null,
        services
      }]
    };

    console.log('formValues after mapping', formValues);
    console.log("selectedServices after mapping", this.selectedServices);
    console.log("selectedSampleRegisterId after mapping", this.selectedSample?.sampleRegisterId);
    console.log("testresultForm after mapping", this.testresultForm.value);
    console.log("testresultForm raw value", this.testresultForm.getRawValue());
    console.log("testresultForm value", this.testresultForm.value);

    return formValues;
  }


  onSubmit() {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.testresultForm.invalid) {
      this.showError('Please fill all testResult fields correctly.');
      return;
    }

    if (!this.selectedSample || !this.selectedSample.sampleRegisterId) {
      this.showError('No sample selected.');
      return;
    }

    const formValues = this.getrowvalue();

    this.testresultService.addUpdatedTestResult(formValues).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
        } else if (res.errors) {
          this.validationErrors = res.errors;
        }
      },
      error: (err) => {
        console.error('API error:', err);
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else {
          this.errorMessage = 'An unexpected error occurred.';
          this.showError(this.errorMessage);
        }
      }
    });
  }


  generatePDF(sampleRegisterId: number) {

    console.log("🔍 Generating PDF for Sample ID:", sampleRegisterId);

    if (!sampleRegisterId) {
      console.error("🚨 Sample ID is missing!");
      return;
    }

    console.log("📄 Sample Register ID:", sampleRegisterId);
    console.log("📄 Selected Sample:", this.selectedSample);

    // Check if jsPDF and autoTable are available
    if (typeof jsPDF === 'undefined' || typeof autoTable === 'undefined') {
      console.error("🚨 jsPDF or autoTable is not available!");
      return;
    }

    // Check if selectedSample is available
    if (!this.selectedSample) {
      this.showError("No sample selected! Please select a sample before generating the PDF.");
      console.error("🚨 No sample selected!");
      return;
    }


    // Find the sample and test data
    if (Array.isArray(this.selectedSample)) {
      var matchedSample: any = null;
      matchedSample = this.selectedSample.find((s: { sampleRegisterId: number }) => s.sampleRegisterId === sampleRegisterId);
    } else if (this.selectedSample && this.selectedSample.sampleRegisterId === sampleRegisterId) {
      matchedSample = this.selectedSample;
    }

    console.log("📄 Matched Sample:", matchedSample);

    // Generate PDF using jsPDF
    const doc = new jsPDF();
    const today = new Date().toISOString().split('T')[0];

    // Report Title
    doc.setFontSize(14).setFont("helvetica", "bold");
    doc.text("LABORATORY REPORT", 105, 24, { align: "center" });

    let finalrY = 17;
    doc.line(10, finalrY, 200, finalrY);
    finalrY += 10; // Adjusted to ensure a max 10px gap
    doc.line(10, finalrY, 200, finalrY);

    // Patient Details Table
    const patientDetails = [
      ["Name", `: ${matchedSample.title || ''} ${matchedSample.firstName || ''} ${matchedSample.middleName || ''}`, "Sex/Age", `: ${matchedSample.gender || ''} / ${matchedSample.age || ''} Years`],
      ["Case ID", `: ${matchedSample.sampleRegisterId || ''}`, "Mobile", `: ${matchedSample.phoneNumber || ''}`],
      ["Branch", `: ${matchedSample.branchName || ''}`, "B2B", `: ${matchedSample.b2BName || ''}`],
      ["Reg Date and Time", `: ${matchedSample.date || ''}`, "Report Date and Time", `: ${today}`],

    ];

    autoTable(doc, {
      startY: 31,
      body: patientDetails,
      theme: "plain",
      styles: { fontSize: 9, cellPadding: 1 },
      columnStyles: { 0: { fontStyle: "bold" }, 2: { fontStyle: "bold" } }
    });

    let finalY = (doc as any).lastAutoTable.finalY + 8;

    // Table Headers
    doc.setFontSize(9).setFont("helvetica", "bold");
    doc.text("Parameter", 10, finalY);
    doc.text("Result", 90, finalY);
    finalY += 2;


    doc.line(10, finalY, 200, finalY);
    finalY += 8;

    // Adding test results
    const pageHeight = doc.internal.pageSize.height;

    matchedSample?.serviceMapping?.forEach((serviceMapping: any) => {
      if (finalY > pageHeight - 20) {
        doc.addPage();
        finalY = 20;
      }

      doc.setFont("helvetica", "bold").text(serviceMapping.serviceName, 10, finalY);
      finalY += 6;

      const services = this.selectedServices.map(service => ({
        serviceId: service.serviceId,
        serviceName: service.serviceName,
        tests: service.tests.map((test: any) => ({
          testId: test.testId,
          testName: test.testName,
          resultValue: test.resultValue,
          validationStatus: test.validationStatus ? 'A' : 'V',
          createdBy: test.createdBy || '',
          validateBy: test.validateBy || '',
          isActive: test.isActive || true
        }))
      }));

      services.forEach((serviceId) => {
        serviceId.tests?.forEach((test: any) => {
          if (finalY > pageHeight - 20) {
            doc.addPage();
            finalY = 20;
          }

          doc.setFont("helvetica", "normal");
          doc.text(test.testName || "", 12, finalY);
          doc.text(test.resultValue?.toString() || "", 90, finalY);
          // If you have unit and refInterval, add them here; otherwise, remove these lines
          doc.text(test.unit || "", 140, finalY);
          doc.text(test.refInterval || "", 170, finalY);
          finalY += 8;
        });
      });
    });
    doc.text("------------------- End Of Report -------------------", 73, finalY);

    // Save PDF
    doc.save(`Test_Report_CaseID_${sampleRegisterId}.pdf`);
  }

}