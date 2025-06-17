using LIMSAPI.Helpers;
using LIMSAPI.Models;
using LIMSAPI.Models.FinanceModal;
using LIMSAPI.Models.Master;
using LIMSAPI.Models.Transaction;
using LIMSAPI.Models.TransactionModal;
using LIMSAPI.RepositryLayer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace LIMSAPI.ServiceLayer
{
    public class LIMSService : LIMSServiceInterface
    {
        public readonly LIMSRepositryInterface _lIMSRepositryInterface;
        public LIMSService(LIMSRepositryInterface lIMSRepositryInterface)
        {
            _lIMSRepositryInterface = lIMSRepositryInterface;
        }




        // DUPLICATE CHECKER
        public bool IsDuplicate(string table, string nameCol,string codeCol, string nameVal,string codeVal, int? excludeId = null, string idCol = "Id", Dictionary<string, object> additionalConditions = null)
        {
            return _lIMSRepositryInterface.IsDuplicate(table, nameCol ,codeCol, nameVal, codeVal, excludeId, idCol, additionalConditions);
        }




        // COUNTRY
        public CountryModal AddUpdatedCountry(CountryModal countryModal)
        {

            return _lIMSRepositryInterface.AddUpdatedCountry(countryModal);

        }

        public List<CountryModal> GetAllCountries(FilterModel filter)
        {
            return _lIMSRepositryInterface.GetAllCountries(filter);
        }

        public CountryModal DeleteCountryById(int CountryId)
        {
            return _lIMSRepositryInterface.DeleteCountryById(CountryId);
        }

        public List<CountryModal> GetCountry()
        {
           return _lIMSRepositryInterface.GetCountry();
        }

        public CountryModal GetCountryById(int CountryId)
        {
            return _lIMSRepositryInterface.GetCountryById(CountryId);
        }




        // STATE
        public StateModal AddUpdatedState(StateModal stateModal)
        {
            return _lIMSRepositryInterface.AddUpdatedState(stateModal);
        }

        public List<StateModal> GetAllStates(FilterModel filter)
        {
            return _lIMSRepositryInterface.GetAllStates(filter);
        }

        public StateModal DeleteStateById(int StateId)
        {
            return _lIMSRepositryInterface.DeleteStateById(StateId);
        }

        public List<StateModal> GetState()
        {
            return _lIMSRepositryInterface.GetState();
        }

        public StateModal GetStateById(int StateId)
        {
            return _lIMSRepositryInterface.GetStateById(StateId);
        }




        // CITY
        public CityModal AddUpdatedCity(CityModal cityModal)
        {
           return _lIMSRepositryInterface.AddUpdatedCity(cityModal);
        }

        public List<CityModal> GetAllCities(FilterModel filter)
        {
            return _lIMSRepositryInterface.GetAllCities(filter);
        }

        public CityModal DeleteCityById(int CityId)
        {
            return _lIMSRepositryInterface.DeleteCityById(CityId);
        }

        public List<CityModal> GetCityIsActive()
        {
            return _lIMSRepositryInterface.GetCityIsActive();
        }

        public CityModal GetCityById(int CityId)
        {
            return _lIMSRepositryInterface.GetCityById(CityId);
        }



        // AREA

        public AreaModal AddUpdatedArea(AreaModal areaModal)
        {
            return _lIMSRepositryInterface.AddUpdatedArea(areaModal);
        }

        public List<AreaModal> GetAllArea(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetAllArea(filterModel);
        }

        public AreaModal DeleteAreaById(int AreaId)
        {
           return _lIMSRepositryInterface.DeleteAreaById(AreaId);
        }

        public List<AreaModal> GetAreaIsActive()
        {
            return _lIMSRepositryInterface.GetAreaIsActive();
        }

        public AreaModal GetAreaById(int AreaId)
        {
            return _lIMSRepositryInterface.GetAreaById(AreaId);
        }



        // BRANCH
        public BranchModal AddUpdatedBranch(BranchModal branchModal)
        {
            return _lIMSRepositryInterface.AddUpdatedBranch(branchModal);
        }

        public List<BranchModal> GetBranches(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetBranches(filterModel);
        }

        public BranchModal DeleteBranchById(int BranchId)
        {
            return _lIMSRepositryInterface.DeleteBranchById(BranchId);
        }

        public List<BranchModal> GetBranchIsActive()
        {
            return _lIMSRepositryInterface.GetBranchIsActive();
        }

        public BranchModal GetBranchById(int BranchId)
        {
            return _lIMSRepositryInterface.GetBranchById(BranchId);
        }



        // B2B

        public B2BModal AddUpdatedB2B(B2BModal b2BModal)
        {
            return _lIMSRepositryInterface.AddUpdatedB2B(b2BModal);
        }

        public List<B2BModal> GetB2Bs(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetB2Bs(filterModel);
        }

        public B2BModal DeleteB2BById(int B2BId)
        {
            return _lIMSRepositryInterface.DeleteB2BById(B2BId);
        }

        public List<B2BModal> GetB2BIsActive()
        {
            return _lIMSRepositryInterface.GetB2BIsActive();
        }

        public B2BModal GetB2BById(int B2BId)
        {
            return _lIMSRepositryInterface.GetB2BById(B2BId);
        }



        // DOCTOR

        public DoctorModal AddUpdatedDoctor(DoctorModal doctorModal)
        {
            return _lIMSRepositryInterface.AddUpdatedDoctor(doctorModal);
        }

        public List<DoctorModal> GetDoctorByFilter(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetDoctorByFilter(filterModel);
        }

        public DoctorModal GetDoctorById(int DoctorId)
        {
            return _lIMSRepositryInterface.GetDoctorById(DoctorId);
        }

        public DoctorModal DeleteDoctorById(int DoctorId)
        {
            return _lIMSRepositryInterface.DeleteDoctorById(DoctorId);
        }

        public List<DoctorModal> GetDoctorIsActive()
        {
            return _lIMSRepositryInterface.GetDoctorIsActive();
        }



        // TEST 

        public TestModal AddUpdatedTest(TestModal testModal)
        {
            return _lIMSRepositryInterface.AddUpdatedTest(testModal);
        }

        public List<TestModal> GetTestByFilter(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetTestByFilter(filterModel);
        }

        public TestModal GetTestById(int TestId)
        {
            return _lIMSRepositryInterface.GetTestById(TestId);
        }

        public TestModal DeleteTestById(int TestId)
        {
            return _lIMSRepositryInterface.DeleteTestById(TestId);
        }

        public List<TestModal> GetTestIsActive()
        {
            return _lIMSRepositryInterface.GetTestIsActive();
        }



        // SERVICE

        public ServiceModal AddUpdatedServiceModal(ServiceModal serviceModal, List<TestModal> testModals)
        {
           return _lIMSRepositryInterface.AddUpdatedServiceModal(serviceModal, testModals);
        }

        public List<ServiceModal> GetServiceByFilter(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetServiceByFilter(filterModel); 
        }

        public ServiceModal GetServiceById(int ServiceId)
        {
            return _lIMSRepositryInterface.GetServiceById(ServiceId);
        }

        public ServiceModal DeleteServiceById(int ServiceId)
        {
            return _lIMSRepositryInterface.DeleteServiceById(ServiceId);
        }

        public List<ServiceModal> GetServiceIsActive()
        {
            return _lIMSRepositryInterface.GetServiceIsActive();
        }

        public ServiceTestMap DeleteServiceMapTestById(int ServiceTestId)
        {
            return _lIMSRepositryInterface.DeleteServiceMapTestById(ServiceTestId);
        }



        // PAYMENT
        public PaymentModal AddUpdatedPayment(PaymentModal paymentModal)
        {
            return _lIMSRepositryInterface.AddUpdatedPayment(paymentModal);
        }

        public List<PaymentModal> GetPaymentByFilter(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetPaymentByFilter(filterModel);
        }

        public PaymentModal GetPaymentById(int PaymentId)
        {
            return _lIMSRepositryInterface.GetPaymentById(PaymentId);
        }

        public PaymentModal DeletePaymentById(int PaymentId)
        {
            return _lIMSRepositryInterface.DeletePaymentById(PaymentId);
        }

        public List<PaymentModal> GetPaymentIsActive()
        {
            return _lIMSRepositryInterface.GetPaymentIsActive();
        }



        // SAMPLE REGISTER
        public SampleRegister AddUpdateSampleRegister(SampleRegister sampleRegister)
        {
            return _lIMSRepositryInterface.AddUpdateSampleRegister(sampleRegister);
        }

        public List<SampleRegister> GetSampleByFilter(FilterModel filterModel)
        {
            return _lIMSRepositryInterface.GetSampleByFilter(filterModel);
        }

        //public List<SampleRegister> GetSampleByIsActive()
        //{
        //    return _lIMSRepositryInterface.GetSampleByIsActive();
        //}

        public SampleRegister GetSampleRegisterById(int SampleRegisterId)
        {
            return _lIMSRepositryInterface.GetSampleRegisterById(SampleRegisterId);
        }

        //public SampleRegister DeleteSampleRegisterById(int SampleRegisterId)
        //{
        //    return _lIMSRepositryInterface.DeleteSampleRegisterById(SampleRegisterId);
        //}

        public SampleServiceMap DeleteSampleServiceMapId(int SampleServiceMapId)
        {
            return _lIMSRepositryInterface.DeleteSampleServiceMapId(SampleServiceMapId);
        }

        public TestResultModal AddUpdateTestResult(TestResultModal resultModal)
        {
            return _lIMSRepositryInterface.AddUpdateTestResult(resultModal);
        }

        public TestResultDto AddUpdateTestResults(TestResultDto testResults)
        {
            return _lIMSRepositryInterface.AddUpdateTestResults(testResults);
        }

        public TestResultDto GetTestResultById(int SampleRegisterId)
        {
            return _lIMSRepositryInterface.GetTestResultById(SampleRegisterId);
        }
    }
}
