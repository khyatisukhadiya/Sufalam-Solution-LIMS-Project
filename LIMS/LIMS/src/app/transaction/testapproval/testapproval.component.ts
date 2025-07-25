import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import jsPDF from 'jspdf';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { TestapprovalService } from '../../service/TransactionService/testapproval/testapproval.service';
import { Modal } from 'bootstrap';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-test-approval',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, SliderbarComponent],
  templateUrl: './testapproval.component.html',
  styleUrl: './testapproval.component.css'
})

export class TestapprovalComponent implements OnInit {

  @ViewChild('myModal') modal !: ElementRef;
  // @ViewChild('autofocus') autofocus!: ElementRef;

  searchClicked = false;
  sampleRegisterMaster: any[] = [];
  filteredSampleRegister: any[] = [];
  searchCriteria = { id: '', name: '', code: '' }
  selectedServices: string[] = [];
  selectTest: any[] = [];
  serviceMappings: any[] = [];
  filteredService: any[] = [];
  filteredTests: any[] = [];
  serviceModal: any;
  SampleRegister: any = null;
  approvalResults: any = null;
  isChecked = false;
  isEditModal = false;
  testresultForm: FormGroup = new FormGroup({});

  testApprovalResultService = inject(TestapprovalService);
  toastr = inject(ToastrService);
  selectedSampleId: number = 0;
  modalInstance: Modal | null = null;
  TestApproval: any;
  user: any;

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    this.loadService();
    this.setFrom();

    const userData = localStorage.getItem('loginDetails');
    if (userData) {
      this.user = JSON.parse(userData);
      this.testresultForm.get('validateBy')?.setValue(this.user.userName);
    }
  }

  loadSampleRegisters(): void {
    this.searchClicked = true;

    const filter = {
      name: this.searchCriteria.name?.trim() || '',
      id: this.searchCriteria.id || null,
      phoneNumber: this.searchCriteria.code
    };

    this.testApprovalResultService.getSampleregister(filter).subscribe({
      next: (response) => {
        this.sampleRegisterMaster = response.data || [];
        this.filteredSampleRegister = [...this.sampleRegisterMaster];
      },
      error: (err) => {
        console.error('Error fetching sample register:', err);
      },
    });
  }

  //  onToggle(event: any): void {
  //   const isChecked = event.target.checked;
  //   console.log('Toggle is now:', isChecked);
  // }


  loadService(): void {
    this.testApprovalResultService.getServices().subscribe({
      next: (response) => {
        this.serviceMappings = response.data || [];
        this.filteredService = [...this.serviceMappings];
      },
      error: (err) => {
        console.error('Error fetching service:', err);
      },
    })
  }

  search(): void {
    this.searchClicked = true;
    this.loadSampleRegisters();
  }

  clearFilter(): void {
    this.searchCriteria = { id: '', name: '', code: '' };
    this.filteredSampleRegister = [...this.sampleRegisterMaster];
    this.filteredService = [...this.serviceMappings];
    this.searchClicked = false;
    this.filteredTests = [];
    this.selectedServices = [];
  }


  openModal(selected: any): void {

    const modal = document.getElementById('myModal');

    if (modal != null) {
      modal.style.display = "block";
      // modal.addEventListener('shown.bs.modal', () => {
      //   if (this.autofocus) {
      //     this.autofocus.nativeElement.focus();
      //   }
      // });
    }

    if (this.modal?.nativeElement) {
      this.modalInstance = new Modal(this.modal.nativeElement, {
        backdrop: 'static',
        keyboard: false
      });
      this.modalInstance.show();
    }


    const sampleRegisterId = selected.sampleRegisterId;

    this.testApprovalResultService.getTestApprovalBySampleId(sampleRegisterId).subscribe({
      next: (response: any) => {
        this.SampleRegister = response.data || [];

        const hasTests = Array.isArray(this.SampleRegister?.tests) && this.SampleRegister.tests.length > 0;
        if (!hasTests) {
          this.showError("Data is not available for this sample");
          if (this.modalInstance) {
            this.modalInstance.hide();
          }
          if (this.modal != null) {
            this.modal.nativeElement.style.display = "none";
          }
          return;
        }

        this.selectedSampleId = sampleRegisterId;

        const sampleServiceIds = this.SampleRegister.serviceMappings?.map((s: any) => s.serviceId) || [];
        console.log('Sample Service IDs:', sampleServiceIds);


        const backendTestsMap = new Map<number, any>(
          (this.SampleRegister.tests || []).map((test: any) => [test.testId, test])
        );
        console.log('Backend Tests Map:', backendTestsMap);


        if (this.serviceMappings && this.SampleRegister && Array.isArray(this.SampleRegister.serviceMappings)) {
          this.selectTest = this.SampleRegister.serviceMappings
            .map((sampleService: any) => {
              const service = this.serviceMappings.find((s: any) => s.serviceId === sampleService.serviceId);
              return {
                serviceId: sampleService.serviceId,
                serviceName: service?.serviceName || sampleService.serviceName,
                tests: (this.SampleRegister?.tests || [])
                  .filter((test: any) => test.serviceId === sampleService.serviceId)
                  .map((test: any) => ({
                    testId: test.testId,
                    testName: test.testName ?? '',
                    resultValue: test.resultValue ?? '',
                    validationStatus: test.validationStatus ?? '',
                    isActive: test.isActive ?? true,
                    createdBy: test.createdBy ?? '',
                    validateBy: test.validateBy ?? '',
                  }))
              };
            });
        } else {
          this.selectTest = [];
        }

        console.log('Sample Register:', this.SampleRegister);
        console.log('servicename', this.selectTest.map((s: any) => s?.serviceName));
        console.log('testmappings', this.selectTest.flatMap((s: any) => s?.tests || []));
        console.log('Sample Register Tests:', this.SampleRegister.tests);
        console.log('Selected Sample ID:', this.selectedSampleId);
        console.log('selectedtest:', this.selectTest);


        this.selectedServices = this.selectTest.map(s => s.serviceName);


        this.isEditModal = (this.SampleRegister.tests?.length ?? 0) > 0;


        console.log('Grouped SelectTest:', this.selectTest);
      },
      error: (err) => {
        console.error('Error loading sample register by ID:', err);
      }
    });
  }


  closeModal() {
    this.SampleRegister = null;
    this.searchClicked = true;
    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    if (this.modal != null) {
      this.modal.nativeElement.style.display = "none";
    }
  }

  getrowvalue() {
    const serviceMapping = this.selectTest.map(service => ({
      serviceId: service.serviceId,
      serviceName: service.serviceName,
      tests: service.tests.map((test: any) => ({
        testId: test.testId,
        testName: test.testName,
        resultValue: test.resultValue,
        validationStatus: test.validationStatus,
        createdBy: test.createdBy || '',
        validateBy: this.user.userName || '',
        isActive: test.isActive || true
      }))
    }));

    const formValues = {
      sampleregister: [{
        sampleRegisterId: this.SampleRegister?.sampleRegisterId || null,
        services: serviceMapping,
      }]
    };

    console.log('formValues after mapping', formValues);
    console.log("selectedServices after mapping", this.selectedServices);
    console.log("selectedSampleRegisterId after mapping", this.SampleRegister?.sampleRegisterId);
    console.log("testresultForm after mapping", this.testresultForm.value);
    console.log("testresultForm raw value", this.testresultForm.getRawValue());
    console.log("testresultForm value", this.testresultForm.value);

    return formValues;
  }


  onSubmit() {
    if (this.SampleRegister.invalid) {
      this.showError('Please fill all testResult fields correctly.');
      return;
    }

    const formValues = this.getrowvalue();

    this.testApprovalResultService.addUpdatedTestResult(formValues).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
        }
      },
      error: (err) => {
        console.error('API error:', err);
        if (err.error && err.error.message) {
          this.showError(err.error.message);
        } else {
          this.showError('An error occurred while submitting the form.');
        }
      }
    });
  }


  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
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


  // generatePDF(selectedSampleId: number) {
  //   console.log("🔍 Generating PDF for Sample ID:", selectedSampleId);

  //   if (!selectedSampleId) {
  //     console.error("🚨 Sample ID is missing!");
  //     return;
  //   }

  //   // Check if jsPDF and autoTable are available
  //   if (typeof jsPDF === 'undefined' || typeof autoTable === 'undefined') {
  //     console.error("🚨 jsPDF or autoTable is not available!");
  //     return;
  //   }

  //   console.log("test Approval", this.sampleRegisterMaster)
  //   // Find the matched sample from sampleRegisterMaster
  //   const matchedSample = this.sampleRegisterMaster.find((s: { sampleRegisterId: number; }) => s.sampleRegisterId === selectedSampleId);

  //   if (!matchedSample) {
  //     this.showError("No sample data found for the selected ID.");
  //     console.error("🚨 No sample data found for ID:", selectedSampleId);
  //     return;
  //   }

  //   console.log("📄 Matched Sample:", matchedSample);

  //   const services = this.selectTest.map(service => ({
  //     serviceId: service.serviceId,
  //     serviceName: service.serviceName,
  //     tests: service.tests.map((test: any) => ({
  //       testId: test.testId,
  //       testName: test.testName,
  //       resultValue: test.resultValue,
  //       validationStatus: test.validationStatus ? 'A' : 'V',
  //       createdBy: test.createdBy || '',
  //       validateBy: test.validateBy || '',
  //       isActive: test.isActive || true
  //     }))
  //   }));


  //   if (!this.selectTest || this.selectTest.length === 0) {
  //     this.showError("Data is not available to generate the report.");
  //     return;
  //   }

  //   const allResult = this.selectTest.every(
  //     (selectTest: any) =>
  //     Array.isArray(selectTest.tests) &&
  //     selectTest.tests.every((test: any) => test.validationStatus === true)
  //   );
  //   if (!allResult) {
  //     this.showError("Report under process");
  //     return;
  //   }


  //   // Generate PDF using jsPDF
  //   const doc = new jsPDF()
  //   const today = new Date().toISOString().split('T')[0];

  //   // Report Title
  //   doc.setFontSize(14).setFont("helvetica", "bold");
  //   doc.text("LABORATORY REPORT", 105, 24, { align: "center" });

  //   let finalrY = 17;
  //   doc.line(10, finalrY, 200, finalrY);
  //   finalrY += 10; // Adjusted to ensure a max 10px gap
  //   doc.line(10, finalrY, 200, finalrY);

  //   // Patient Details Table
  //   const patientDetails = [
  //     ["Name", `: ${matchedSample.title || ''} ${matchedSample.firstName || ''} ${matchedSample.middleName || ''}`, "Sex/Age", `: ${matchedSample.gender || ''} / ${matchedSample.age || ''} Years`],
  //     ["Case ID", `: ${matchedSample.sampleRegisterId || ''}`, "Mobile", `: ${matchedSample.phoneNumber || ''}`],
  //     ["Branch", `: ${matchedSample.branchName || ''}`, "B2B", `: ${matchedSample.b2BName || ''}`],
  //     ["Reg Date", `: ${matchedSample.date || ''}`, "Report Date", `: ${today}`],

  //   ];

  //   autoTable(doc, {
  //     startY: 31,
  //     body: patientDetails,
  //     theme: "plain",
  //     styles: { fontSize: 9, cellPadding: 1 },
  //     columnStyles: { 0: { fontStyle: "bold" }, 2: { fontStyle: "bold" } }
  //   });

  //   let finalY = (doc as any).lastAutoTable.finalY + 8;

  //   // Table Headers
  //   doc.setFontSize(9).setFont("helvetica", "bold");
  //   doc.text("Parameter", 10, finalY);
  //   doc.text("Result", 90, finalY);
  //   finalY += 2;


  //   doc.line(10, finalY, 200, finalY);
  //   finalY += 8;

  //   // Adding test results
  //   const pageHeight = doc.internal.pageSize.height;

  //   matchedSample?.serviceMapping?.forEach((serviceMapping: any) => {
  //     if (finalY > pageHeight - 20) {
  //       doc.addPage();
  //       finalY = 20;
  //     }

  //     doc.setFont("helvetica", "bold").text(serviceMapping.serviceName, 10, finalY);
  //     finalY += 6;

  //     // const services = this.selectTest.map(service => ({
  //     //   serviceId: service.serviceId,
  //     //   serviceName: service.serviceName,
  //     //   tests: service.tests.map((test: any) => ({
  //     //     testId: test.testId,
  //     //     testName: test.testName,
  //     //     resultValue: test.resultValue,
  //     //     validationStatus: test.validationStatus ? 'A' : 'V',
  //     //     createdBy: test.createdBy || '',
  //     //     validateBy: test.validateBy || '',
  //     //     isActive: test.isActive || true
  //     //   }))
  //     // }));


  //     const currentService = services.find(s => s.serviceId === serviceMapping.serviceId);
  //     currentService?.tests?.forEach((test: any) => {
  //       if (finalY > pageHeight - 20) {
  //         doc.addPage();
  //         finalY = 20;
  //       }

  //       doc.setFont("helvetica", "normal");
  //       doc.text(test.testName || "", 12, finalY);
  //       doc.text(test.resultValue?.toString() || "", 90, finalY);
  //       // doc.text(test.unit || "", 140, finalY);
  //       // doc.text(test.refInterval || "", 170, finalY);
  //       finalY += 8;
  //     });

  //     doc.text("------------------- Validate By : " + test.validateBy + " -------------------", 73, finalY);
  //     // finalY += 10;

  //   });

  //   doc.text("------------------- End Of Report -------------------", 73, finalY);

  //   // Save PDF
  //   doc.save(`Test_Report_CaseID_${selectedSampleId}.pdf`);





  //   // SEND REPORT TO EMAIL

  //   const toEmail = matchedSample?.email;
  //   console.log("Email", matchedSample.email);
  //   const subject = `Test Report for Case ID ${selectedSampleId}`;
  //   const body = "Dear Customer,<br/><br/>I have attached your test report please check it.<br/><br/>Best regards,<br/>LIMS Team";
  //   const pdfBlob = doc.output('blob');

  //   const formData = new FormData();
  //   formData.append('toEmail', toEmail);
  //   formData.append('subject', subject);
  //   formData.append('body', body);
  //   formData.append('attachments', pdfBlob, `Test_Report_CaseID_${selectedSampleId}.pdf`);

  //   this.testApprovalResultService.sendResportByEmail(toEmail, subject, body, pdfBlob, `Test_Report_CaseID_${selectedSampleId}.pdf`).subscribe({
  //     next: (res) => {
  //       console.log(res);
  //       this.showSuccess(res.message);
  //     },
  //     error: (err) => {
  //       console.error(err);
  //       this.showError("Error sending report");
  //     }
  //   });





  //     // SEND SMS

  //     const toPhoneNumber = matchedSample?.phoneNumber;
  //     const messageBody = `Hello Dear Customer ${matchedSample.middleName} ${matchedSample.firstName}. Please Collect Your report.`;
  //     const formDatas = new FormData();
  //     formDatas.append("toPhoneNumber", toPhoneNumber);
  //     formDatas.append("messageBody", messageBody);
  //     this.testApprovalResultService.sendSMS(toPhoneNumber, messageBody).subscribe({
  //       next: (res) =>
  //       {
  //         console.log(res),
  //         this.showSuccess(res.message); 
  //       },
  //       error: (err) =>{
  //         console.error(err); 
  //         this.showError("Error sending SMS");
  //       }
  //     }); 
  // }


  generatePDF(selectedSampleId: number) {
    console.log("🔍 Generating PDF for Sample ID:", selectedSampleId);

    if (selectedSampleId !== this.selectedSampleId) {
      this.showError("Please open the correct modal to generate this report.");
      return;
    }


    if (!selectedSampleId) {
      console.error("🚨 Sample ID is missing!");
      return;
    }

    if (typeof jsPDF === 'undefined' || typeof autoTable === 'undefined') {
      console.error("🚨 jsPDF or autoTable is not available!");
      return;
    }

    const matchedSample = this.sampleRegisterMaster.find(
      (s: { sampleRegisterId: number }) => s.sampleRegisterId === selectedSampleId
    );

    if (!matchedSample) {
      this.showError("No sample data found for the selected ID.");
      return;
    }

    if (!this.selectTest || this.selectTest.length === 0) {
      this.showError("Data is not available to generate the report.");
      return;
    }

    const allResult = this.selectTest.every(
      (selectTest: any) =>
        Array.isArray(selectTest.tests) &&
        selectTest.tests.every((test: any) => test.validationStatus === true)
    );
    if (!allResult) {
      this.showError("Report under process");
      return;
    }

    const services = this.selectTest.map(service => ({
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

    // Extract first validator's name for the footer
    const firstValidator = services
      .flatMap(service => service.tests)
      .find(test => test.validateBy)?.validateBy || "Lab Technician";

    const doc = new jsPDF();
    const today = new Date().toISOString().split('T')[0];

    // Header Title
    doc.setFontSize(14).setFont("helvetica", "bold");
    doc.text("LABORATORY REPORT", 105, 24, { align: "center" });

    let finalrY = 17;
    doc.line(10, finalrY, 200, finalrY);
    finalrY += 10;
    doc.line(10, finalrY, 200, finalrY);

    // Patient Details
    const patientDetails = [
      ["Name", `: ${matchedSample.title || ''} ${matchedSample.firstName || ''} ${matchedSample.middleName || ''}`, "Sex/Age", `: ${matchedSample.gender || ''} / ${matchedSample.age || ''} Years`],
      ["Case ID", `: ${matchedSample.sampleRegisterId || ''}`, "Mobile", `: ${matchedSample.phoneNumber || ''}`],
      ["Branch", `: ${matchedSample.branchName || ''}`, "B2B", `: ${matchedSample.b2BName || ''}`],
      ["Reg Date", `: ${matchedSample.date || ''}`, "Report Date", `: ${today}`],
    ];

    autoTable(doc, {
      startY: 31,
      body: patientDetails,
      theme: "plain",
      styles: { fontSize: 9, cellPadding: 1 },
      columnStyles: { 0: { fontStyle: "bold" }, 2: { fontStyle: "bold" } }
    });

    let finalY = (doc as any).lastAutoTable.finalY + 8;

    // Column headers
    doc.setFontSize(9).setFont("helvetica", "bold");
    doc.text("Parameter", 10, finalY);
    doc.text("Result", 90, finalY);
    finalY += 2;
    doc.line(10, finalY, 200, finalY);
    finalY += 8;

    const pageHeight = doc.internal.pageSize.height;

    matchedSample?.serviceMapping?.forEach((serviceMapping: any) => {
      if (finalY > pageHeight - 30) {
        doc.addPage();
        finalY = 20;
      }

      doc.setFont("helvetica", "bold");
      doc.text(serviceMapping.serviceName, 10, finalY);
      finalY += 6;

      const currentService = services.find(s => s.serviceId === serviceMapping.serviceId);

      currentService?.tests?.forEach((test: any) => {
        if (finalY > pageHeight - 30) {
          doc.addPage();
          finalY = 20;
        }

        doc.setFont("helvetica", "normal");
        doc.text(test.testName || "", 12, finalY);
        doc.text(test.resultValue?.toString() || "", 90, finalY);
        finalY += 8;
      });
    });

    // End-of-report line
    doc.text("------------------- End Of Report -------------------", 73, finalY);

    //  Add validated-by footer to each page
    const totalPages = doc.getNumberOfPages();
    for (let i = 1; i <= totalPages; i++) {
      doc.setPage(i);
      doc.setFontSize(10).setFont("helvetica", "italic");
      const footerText = `Approved By : ${firstValidator}`;
      doc.text(footerText, doc.internal.pageSize.getWidth() / 2, doc.internal.pageSize.getHeight() - 10, { align: "center" });
    }

    // Save PDF
    doc.save(`Test_Report_CaseID_${selectedSampleId}.pdf`);



    //  Email Report
    const toEmail = matchedSample?.email;
    const subject = `Test Report for Case ID ${selectedSampleId}`;
    const body = "Dear Customer,<br/><br/>I have attached your test report. Please check it.<br/><br/>Best regards,<br/>LIMS Team";
    const pdfBlob = doc.output('blob');

    this.testApprovalResultService.sendResportByEmail(toEmail, subject, body, pdfBlob, `Test_Report_CaseID_${selectedSampleId}.pdf`).subscribe({
      next: (res) => {
        console.log(res);
        this.showSuccess(res.message);
      },
      error: (err) => {
        console.error(err);
        this.showError("Error sending report");
      }
    });



    //  Send SMS
    const toPhoneNumber = matchedSample?.phoneNumber;
    const messageBody = `Hello Dear Customer ${matchedSample.middleName} ${matchedSample.firstName}. Please collect your report.`;

    this.testApprovalResultService.sendSMS(toPhoneNumber, messageBody).subscribe({
      next: (res) => {
        console.log(res);
        this.showSuccess(res.message);
      },
      error: (err) => {
        console.error(err);
        this.showError("Error sending SMS");
      }
    });
  }


}