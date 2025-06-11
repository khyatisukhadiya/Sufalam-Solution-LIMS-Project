export interface serviceModal {
  serviceId : number;
  serviceCode : string;
  serviceName : string;
  b2BAmount : number;
  b2CAmount : number;
  isActive : boolean;
  ServiceTestId : number;
  test : [{
    isActive: true;
    testId : number;
    testName : string;
    testCode : string;
  }]
}