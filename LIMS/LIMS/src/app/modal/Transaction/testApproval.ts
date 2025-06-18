export interface testApproval {
  sampleregister: [{
        sampleregisterId: number,
        services: [{
            serviceId: number,
            serviceName: string,
            tests: [{
                testId: number,
                testName: string,
                resultValue: string,
                validationStatus: string,
                createdby: string,
                validateBy: string,
                isActive : boolean,
            }]
        }]
    }]

}

