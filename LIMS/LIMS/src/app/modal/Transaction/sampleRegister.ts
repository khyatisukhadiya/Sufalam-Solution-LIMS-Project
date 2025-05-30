export interface SampleRegister {
    sampleRegisterId: number;
    date: string;
    branchId: number;
    totalAmount: number;
    isB2B: boolean;
    b2BId: number;
    phoneNumber: string;
    title: string;
    firstName: string;
    middleName: string;
    lastName: string;
    dob: string;
    age: number;
    gender: string;
    email: string;
    cityId: number;
    areaId: number;
    address: string;
    // doctorId: number;
    isActive: boolean;
    amount: number;
    chequeNo: string;
    chequeDate: string;
    transactionId: string;
    serviceId: number;
    paymentId: number;
    paymentName: string;
    serviceMapping: [{
        serviceId: number;
        serviceCode: string;
        serviceName: string;
        b2BAmount: number;
        b2CAmount: number;
        isActive: boolean;
        sampleServiceMapId: number;
    }],
    paymentMapping: [{
        paymentId: number;
        paymentName: string;
        isCash: boolean;
        isCheque: boolean;
        isOnline: boolean;
    }]
}