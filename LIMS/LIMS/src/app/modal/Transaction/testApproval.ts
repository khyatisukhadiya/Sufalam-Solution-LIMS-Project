export interface testApproval {
    testResultId : number;
    sampleRegisterId : number;
    serviceId : number;
    testId : number;
    resultValue : string;
    validationStatus : boolean;
    createdBy : string;
    validateBy : string;
    isActive : boolean;
}


// export interface testApproval {
//   sampleregister: [{
//         sampleregisterId: number,
//         services: [{
//             serviceId: number,
//             serviceName: string,
//             tests: [{
//                 testId: number,
//                 testName: string,
//                 resultValue: string,
//                 validationStatus: boolean,
//                 createdby: string,
//                 validateBy: string,
//                 isActive : boolean,
//             }]
//         }]
//     }]
// }

