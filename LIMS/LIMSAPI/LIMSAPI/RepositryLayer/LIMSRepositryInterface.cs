using LIMSAPI.Models;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.Models.Master;
using LIMSAPI.Models.Transaction;
using LIMSAPI.Models.TransactionModal;

namespace LIMSAPI.RepositryLayer
{
    public interface LIMSRepositryInterface
    {

        //DUPLICATE
        public bool IsDuplicate(string table, string nameCol, string codeCol, string nameVal, string codeVal, int? excludeId = null, string idCol = "Id", Dictionary<string, object> additionalConditions = null);



        // COUNTRY 
        CountryModal AddUpdatedCountry(CountryModal countryModal);

        List<CountryModal> GetAllCountries(FilterModel filter);

        CountryModal DeleteCountryById(int CountryId);

        List<CountryModal> GetCountry();
        CountryModal GetCountryById(int CountryId);



        // STATE
        StateModal AddUpdatedState(StateModal stateModal);

        List<StateModal> GetAllStates(FilterModel filter);

        StateModal DeleteStateById(int StateId);

        List<StateModal> GetState();

        StateModal GetStateById(int StateId);



        // CITY

        CityModal AddUpdatedCity(CityModal cityModal);

        List<CityModal> GetAllCities(FilterModel filter);

        CityModal DeleteCityById(int CityId);

        List<CityModal> GetCityIsActive();

        CityModal GetCityById(int CityId);



        // DISTRICT

        AreaModal AddUpdatedArea(AreaModal areaModal);

        List<AreaModal> GetAllArea(FilterModel filterModel);

        AreaModal DeleteAreaById(int AreaId);

        List<AreaModal> GetAreaIsActive();

        AreaModal GetAreaById(int AreaId);



        // BRANCH

        BranchModal AddUpdatedBranch(BranchModal branchModal);

        List<BranchModal> GetBranches(FilterModel filterModel);

        BranchModal DeleteBranchById(int BranchId);

        List<BranchModal> GetBranchIsActive();

        BranchModal GetBranchById(int BranchId);


        // B2B 

        B2BModal AddUpdatedB2B(B2BModal b2BModal);

        List<B2BModal> GetB2Bs(FilterModel filterModel);

        B2BModal DeleteB2BById(int B2BId);

        List<B2BModal> GetB2BIsActive();

        B2BModal GetB2BById(int B2BId);


        // DOCTOR

        DoctorModal AddUpdatedDoctor(DoctorModal doctorModal);

        List<DoctorModal> GetDoctorByFilter(FilterModel filterModel);

        DoctorModal GetDoctorById(int DoctorId);

        DoctorModal DeleteDoctorById(int DoctorId);

        List<DoctorModal> GetDoctorIsActive();



        // TEST 

        TestModal AddUpdatedTest(TestModal testModal);

        List<TestModal> GetTestByFilter(FilterModel filterModel);

        TestModal GetTestById(int TestId);

        TestModal DeleteTestById(int TestId);

        List<TestModal> GetTestIsActive();




        // SERVICE

        ServiceModal AddUpdatedServiceModal(ServiceModal serviceModal, List<TestModal> testModals);

        List<ServiceModal> GetServiceByFilter(FilterModel filterModel);

        ServiceModal GetServiceById(int ServiceId);

        ServiceModal DeleteServiceById(int ServiceId);

        List<ServiceModal> GetServiceIsActive();



        // SERVICETESTMAP

        ServiceTestMap DeleteServiceMapTestById(int ServiceTestId);



        // PAYMENT

        PaymentModal AddUpdatedPayment(PaymentModal paymentModal);

        List<PaymentModal> GetPaymentByFilter(FilterModel filterModel);

        PaymentModal GetPaymentById(int PaymentId);

        PaymentModal DeletePaymentById(int PaymentId);

        List<PaymentModal> GetPaymentIsActive();


        // SAMPLE REGISTER

        SampleRegister AddUpdateSampleRegister(SampleRegister sampleRegister);

        //List<SampleRegister> GetSampleByFilter(FilterModel filterModel);

        List<SampleRegister> GetSampleByFilter();

        SampleRegister GetSampleRegisterById(int SampleRegisterId);

        //SampleRegister DeleteSampleRegisterById(int SampleRegisterId);



        // SAMPLE SERVICE MAP

        SampleServiceMap DeleteSampleServiceMapId(int SampleServiceMapId);
    }
}
