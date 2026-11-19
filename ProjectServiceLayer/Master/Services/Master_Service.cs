using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectInfrastructure.Master;
using FrontOfficeApplication.ProjectServiceLayer.Master.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.Master.Interfaces;
using System.Numerics;
using System.Text.Json;

namespace FrontOfficeApplication.ProjectServiceLayer.Master.Services
{
    public class Master_Service : IMaster_Service
    {
        private readonly IMaster_Repository _masterRepository;
        private readonly CommonMethod _commonMethod;
        private readonly IDbConnectionFactory _factory;

        public Master_Service (IMaster_Repository masterRepository, CommonMethod commonMethod, IDbConnectionFactory factory)
        {
            _masterRepository = masterRepository;
            _commonMethod = commonMethod;
            _factory = factory;
        }

        #region FloorMaster

        public async Task<IEnumerable<FloorMasterModel>> GetFloorMasterList(string BranchCode, string PropertyCode)
        {
            var userMasters = await _masterRepository.GetFloorMasterList(BranchCode, PropertyCode);

            return userMasters;
        }

        public async Task<ServiceResult<int>> CreateFloorMasterDetail(FloorMasterRequestModel floormaster)
        {

            var result = await _masterRepository.CreateFloorMasterDetail(floormaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Floor Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Floor Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateFloorMasterDetail(FloorMasterRequestModel floormaster)
        {
            var Updatefloor = await _masterRepository.UpdateFloorMasterDetail(floormaster);
            if (Updatefloor > 0)
            {
                return ServiceResult<int>.Ok(Updatefloor, $"Floor Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Floor Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteFloorMasterDetail(int FloorId, string FloorCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteFloorMasterDetail(FloorId, FloorCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Floor Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Floor Detail not found");
            }
        }

        #endregion

        #region FinancialMaster

        public async Task<FinancialMasterModel> GetFinancialMasterList(string BranchCode, string PropertyCode)
        {
            var FinMasters = await _masterRepository.GetFinancialMasterList(BranchCode, PropertyCode);

            return FinMasters;
        }

        public async Task<ServiceResult<int>> SaveorUpdateFinancialMasterDetail(FinancialMasterModel financialmaster)
        {

            var result = await _masterRepository.SaveorUpdateFinancialMasterDetail(financialmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Financial Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Financial Detail");
            }
        }

        public async Task<ServiceResult<int>> DeleteFinancialMasterDetail(int FinId, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteFinancialMasterDetail(FinId, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Financial Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Financial Detail not found");
            }
        }

        #endregion

        #region RoomTypeMaster

        public async Task<IEnumerable<RoomTypeResponseDto>> GetRoomTypeMasterList(string BranchCode, string PropertyCode)
        {
            var roomMasterList = await _masterRepository.GetRoomTypeMasterList(BranchCode, PropertyCode);

            //var roomTariffList = await _masterRepository.GetRoomTariffMasterList(BranchCode, PropertyCode);

            //var tariffDictionary = roomMasterList.ToDictionary(
            //    x => $"{x.RoomCode}_{x.PropertyCode}_{x.BranchCode}",
            //    x => x);

            var response = roomMasterList.Select(room =>
            {
                //var key =
                //    $"{room.RoomCode}_{room.PropertyCode}_{room.BranchCode}";

                //tariffDictionary.TryGetValue(key, out var tariff);

                var dto = new RoomTypeResponseDto
                {
                    // RoomType information
                    RoomId = room.RoomId,
                    RoomCode = room.RoomCode,
                    RoomName = room.RoomName,
                    RoomDescription = room.RoomDescription,
                    RoomMode = room.RoomMode,
                    IsRoomActive = room.IsRoomActive,
                    TotalRooms = room.TotalRooms,
                    Allocated = room.Allocated,
                    UnAllocated = room.UnAllocated,
                    RoomPlanType = room.RoomPlanType,
                    PlanId = room.PlanId,
                    PlanName = room.PlanName,
                    ChannelManagerCode = room.ChannelManagerCode,

                    // Tariff information
                    DefaultTraiff = room.DefaultTraiff,
                    Pax = room.Pax,
                    ExtraAdult = room.ExtraAdult,
                    ExtraChild = room.ExtraChild,
                    Taxinclude = room.Taxinclude,
                    Package = room.Package,
                    PaxRates = JsonSerializer.Deserialize<List<PlanRateDto>>(room.PaxRates) ?? [],

                    //PaxRates = string.IsNullOrWhiteSpace(tariff?.PaxRates)
                    //    ? new List<PaxRateDto>()
                    //    : JsonSerializer.Deserialize<List<PaxRateDto>>(
                    //        tariff.PaxRates
                    //      ) ?? new List<PaxRateDto>(),

                    //Common
                    UserCode = room.UserCode,
                    IPAddress = room.IPAddress,
                    PropertyCode = room.PropertyCode,
                    BranchCode = room.BranchCode
                };

                return dto;
            });

            return response;
        }

        public async Task<ServiceResult<int>> CreateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster)
        {

            var result = await _masterRepository.CreateRoomTypeMasterDetail(roomtypemaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"RoomType Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert RoomType Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateRoomTypeMasterDetail(RoomTypeMasterRequestModel roomtypemaster)
        {
            var Updatetype = await _masterRepository.UpdateRoomTypeMasterDetail(roomtypemaster);
            if (Updatetype > 0)
            {
                return ServiceResult<int>.Ok(Updatetype, $"RoomType Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"RoomType Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteRoomTypeMasterDetail(int RoomId, string RoomCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteRoomTypeMasterDetail(RoomId, RoomCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"RoomType Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"RoomType Detail not found");
            }
        }

        #endregion

        #region PlanMaster

        public async Task<IEnumerable<PlanMasterModel>> GetPlanMasterList(string BranchCode, string PropertyCode)
        {
            var PlanList = await _masterRepository.GetPlanMasterList(BranchCode, PropertyCode);
            
            return PlanList;
        }

        public async Task<ServiceResult<int>> CreatePlanMasterDetail(PlanMasterModel planmaster)
        {

            var result = await _masterRepository.CreatePlanMasterDetail(planmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Plan Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Plan Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdatePlanMasterDetail(PlanMasterModel planmaster)
        {
            var Updateplan = await _masterRepository.UpdatePlanMasterDetail(planmaster);
            if (Updateplan > 0)
            {
                return ServiceResult<int>.Ok(Updateplan, $"Plan Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Plan Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeletePlanMasterDetail(int PlanId, string PlanCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeletePlanMasterDetail(PlanId, PlanCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Plan Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Plan Detail not found");
            }
        }

        #endregion

        #region RoomDetailMaster

        public async Task<IEnumerable<RoomDetailMasterModel>> GetRoomDetailMasterList(string BranchCode, string PropertyCode)
        {
            var roomdetailList = await _masterRepository.GetRoomDetailMasterList(BranchCode, PropertyCode);

            return roomdetailList;
        }

        public async Task<ServiceResult<int>> CreateRoomDetailMasterDetail(List<RoomDetailMasterRequestModel> detailmaster)
        {
            int successCount = 0;
            foreach (var item in detailmaster)
            {
                var result = await _masterRepository.CreateRoomDetailMasterDetail(item);
                if (result > 0)
                {
                    successCount++;
                }
                else if (result == -1)
                {
                    return ServiceResult<int>.Fail( result, $"Room {item.RoomNo} already exists.");
                }
                else if (result == -2)
                {
                    return ServiceResult<int>.Fail( result, $"Floor {item.FloorCode} has no available rooms.");
                }
                else if (result == -3)
                {
                    return ServiceResult<int>.Fail( result, $"Room type {item.RoomCode} has no available rooms.");
                }
            }

            return ServiceResult<int>.Created( successCount, $"{successCount} Room detail(s) inserted successfully.");
        }

        public async Task<ServiceResult<int>> UpdateRoomDetailMasterDetail(List<RoomDetailMasterRequestModel> detailmaster)
        {
            int totalUpdated = 0;
            foreach (var item in detailmaster)
            {
                var Updateresult = await _masterRepository.UpdateRoomDetailMasterDetail(item);
                if (Updateresult > 0)
                {
                    totalUpdated += Updateresult;
                }
                else if (Updateresult == -1)
                {
                    return ServiceResult<int>.Fail(Updateresult, $"Room {item.RoomNo} Not exists.");
                }
                else if (Updateresult == -2)
                {
                    return ServiceResult<int>.Fail(Updateresult, $"Floor {item.FloorCode} has no available rooms.");
                }
                else if (Updateresult == -3)
                {
                    return ServiceResult<int>.Fail(Updateresult, $"Room type {item.RoomCode} has no available rooms.");
                }
                else if (Updateresult == -4)
                {
                    return ServiceResult<int>.Fail(Updateresult, $"Room No {item.RoomNo} with Room Code {item.RoomCode} already exists.");
                }
            }

            if (totalUpdated > 0)
            {
                return ServiceResult<int>.Ok( totalUpdated, "Room Detail Updated successfully");
            }

            return ServiceResult<int>.NotFound( "Room Detail not found");
        }

        public async Task<ServiceResult<int>> DeleteRoomDetailMasterDetail(int RoomDetailId, string RoomNo, string RoomCode, string FloorCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteRoomDetailMasterDetail(RoomDetailId, RoomNo, RoomCode, FloorCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Room Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Room Detail not found");
            }
        }

        #endregion

        #region PurposeMaster

        public async Task<IEnumerable<PurposeMasterModel>> GetPurposeMasterList(string BranchCode, string PropertyCode)
        {
            var PurposeList = await _masterRepository.GetPurposeMasterList(BranchCode, PropertyCode);

            return PurposeList;
        }

        public async Task<ServiceResult<int>> CreatePurposeMasterDetail(PurposeMasterModel purposemaster)
        {

            var result = await _masterRepository.CreatePurposeMasterDetail(purposemaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Purpose Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Purpose Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdatePurposeMasterDetail(PurposeMasterModel purposemaster)
        {
            var Updatepurpose = await _masterRepository.UpdatePurposeMasterDetail(purposemaster);
            if (Updatepurpose > 0)
            {
                return ServiceResult<int>.Ok(Updatepurpose, $"Purpose Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Purpose Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeletePurposeMasterDetail(int PurposeId, string PurposeCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeletePurposeMasterDetail(PurposeId, PurposeCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Purpose Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Purpose Detail not found");
            }
        }

        #endregion

        #region PaymentMaster

        public async Task<IEnumerable<PaymentMainModeModel>> GetPaymentMainMasterList(string PropertyCode, string BranchCode)
        {
            var MainList = await _masterRepository.GetPaymentMainMasterList(PropertyCode, BranchCode);

            return MainList;
        }

        public async Task<IEnumerable<MainandSubModeResponseDto>> GetPaymentSubMasterList(string PropertyCode, string BranchCode)
        {
            var subList = await _masterRepository.GetPaymentSubMasterList(PropertyCode, BranchCode);

            var groupedList = subList
                .GroupBy(x => x.MainModeId)
                .Select(g => new MainandSubModeResponseDto
                {
                    MainModeId = g.Key,
                    SubModes = g.ToList()
                })
                .ToList();

            return groupedList;
        }

        public async Task<ServiceResult<int>> SaveMainandSubPaymentMode(PaymentModeRequestModel requestmodel)
        {
            var Details = await _masterRepository.SaveMainandSubPaymentMode(requestmodel);
            if (Details > 0)
            {
                return ServiceResult<int>.Created(Details, $"Main and Sub PaymentMode inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(Details, $"Failed to insert Main and Sub PaymentMode");
            }
        }

        public async Task<ServiceResult<bool>> DeleteMainPaymentModeDetail(int MainModeId, string PropertyCode, string BranchCode)
        {
            var success = await _masterRepository.DeleteMainPaymentModeDetail(MainModeId, PropertyCode, BranchCode);
            if (!success)
            {
                return ServiceResult<bool>.NotFound($"MainPaymentMode not found");
            }
            else
            {
                return ServiceResult<bool>.Ok(success, $"MainPaymentMode Removed successfully");
            }
        }

        public async Task<ServiceResult<bool>> DeleteSubPaymentModeDetail(int SubModeId, string PropertyCode, string BranchCode)
        {
            var success = await _masterRepository.DeleteSubPaymentModeDetail(SubModeId, PropertyCode, BranchCode);
            if (!success)
            {
                return ServiceResult<bool>.NotFound($"SubPaymentMode not found");
            }
            else
            {
                return ServiceResult<bool>.Ok(success, $"SubPaymentMode Removed successfully");
            }
        }

        #endregion

        #region IdentityproofMaster

        public async Task<IEnumerable<IdentityproofMasterModel>> GetIdentityproofMasterList(string BranchCode, string PropertyCode)
        {
            var ProofList = await _masterRepository.GetIdentityproofMasterList(BranchCode, PropertyCode);

            return ProofList;
        }

        public async Task<ServiceResult<int>> CreateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster)
        {

            var result = await _masterRepository.CreateIdentityproofMasterDetail(proofmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Identityproof Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Identityproof Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateIdentityproofMasterDetail(IdentityproofMasterModel proofmaster)
        {
            var Updateproof = await _masterRepository.UpdateIdentityproofMasterDetail(proofmaster);
            if (Updateproof > 0)
            {
                return ServiceResult<int>.Ok(Updateproof, $"Identityproof Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Identityproof Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteIdentityproofMasterDetail(int IdentityproofId, string IdentityproofCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteIdentityproofMasterDetail(IdentityproofId, IdentityproofCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Identityproof Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Identityproof Detail not found");
            }
        }

        #endregion

        #region TaxMaster

        public async Task<IEnumerable<TaxMasterModel>> GetTaxMasterList(string BranchCode, string PropertyCode)
        {
            var TaxList = await _masterRepository.GetTaxMasterList(BranchCode, PropertyCode);

            return TaxList;
        }

        public async Task<ServiceResult<int>> CreateTaxMasterDetail(TaxMasterModel taxmaster)
        {

            var result = await _masterRepository.CreateTaxMasterDetail(taxmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Tax Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Tax Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateTaxMasterDetail(TaxMasterModel taxmaster)
        {
            var Updatetax = await _masterRepository.UpdateTaxMasterDetail(taxmaster);
            if (Updatetax > 0)
            {
                return ServiceResult<int>.Ok(Updatetax, $"Tax Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Tax Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteTaxMasterDetail(int TaxId, string TaxCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteTaxMasterDetail(TaxId, TaxCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Tax Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Tax Detail not found");
            }
        }

        #endregion

        #region TaxDescriptionMaster

        public async Task<IEnumerable<TaxDescriptionMasterModel>> GetTaxDescriptionMasterList(string BranchCode, string PropertyCode)
        {
            var TaxDescriptionList = await _masterRepository.GetTaxDescriptionMasterList(BranchCode, PropertyCode);

            return TaxDescriptionList;
        }

        public async Task<ServiceResult<int>> CreateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster)
        {

            var result = await _masterRepository.CreateTaxDescriptionMasterDetail(descriptionmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"TaxDescription Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert TaxDescription Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateTaxDescriptionMasterDetail(TaxDescriptionMasterModel descriptionmaster)
        {
            var Updatedescription = await _masterRepository.UpdateTaxDescriptionMasterDetail(descriptionmaster);
            if (Updatedescription > 0)
            {
                return ServiceResult<int>.Ok(Updatedescription, $"TaxDescription Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"TaxDescription Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteTaxDescriptionMasterDetail(int TaxDescriptionId, string TaxDescriptionCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteTaxDescriptionMasterDetail(TaxDescriptionId, TaxDescriptionCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"TaxDescription Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"TaxDescription Detail not found");
            }
        }

        #endregion

        #region CompanyMaster

        public async Task<IEnumerable<CompanyMasterModel>> GetCompanyMasterList(string BranchCode, string PropertyCode)
        {
            var CompanyList = await _masterRepository.GetCompanyMasterList(BranchCode, PropertyCode);

            return CompanyList;
        }

        public async Task<ServiceResult<int>> CreateCompanyMasterDetail(CompanyMasterModel companymaster)
        {

            var result = await _masterRepository.CreateCompanyMasterDetail(companymaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Company Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Company Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateCompanyMasterDetail(CompanyMasterModel companymaster)
        {
            var Updatecompany = await _masterRepository.UpdateCompanyMasterDetail(companymaster);
            if (Updatecompany > 0)
            {
                return ServiceResult<int>.Ok(Updatecompany, $"Company Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Company Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteCompanyMasterDetail(int CompanyId, string CompanyCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteCompanyMasterDetail(CompanyId, CompanyCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Company Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Company Detail not found");
            }
        }

        #endregion

        #region CompanyTraiffMaster

        public async Task<IEnumerable<CompanyTraiffResponseDto>> GetCompanyTraiffMasterList(string BranchCode, string PropertyCode)
        {
            var CompanyTraiffList = await _masterRepository.GetCompanyTraiffMasterList(BranchCode, PropertyCode);

            var response = CompanyTraiffList.Select(room =>
            {
                //var key =
                //    $"{room.RoomCode}_{room.PropertyCode}_{room.BranchCode}";

                //tariffDictionary.TryGetValue(key, out var tariff);

                var dto = new CompanyTraiffResponseDto
                {
                    // RoomType information
                    CompanyTraiffId = room.CompanyTraiffId,
                    CompanyCode = room.CompanyCode,
                    RoomCode = room.RoomCode,
                    TraiffCategory = room.TraiffCategory,
                    RoomPlanType = room.RoomPlanType,
                    PlanId = room.PlanId,
                    PlanName = room.PlanName,

                    // Tariff information
                    DefaultTraiff = room.DefaultTraiff,
                    Pax = room.Pax,
                    PaxRates = JsonSerializer.Deserialize<List<PlanRateDto>>(room.PaxRates) ?? [],
                    ExtraAdult = room.ExtraAdult,
                    ExtraChild = room.ExtraChild,
                    TaxInclude = room.TaxInclude,
                    ServiceTax = room.ServiceTax,
                    Package = room.Package,

                    //Common
                    UserCode = room.UserCode,
                    IPAddress = room.IPAddress,
                    PropertyCode = room.PropertyCode,
                    BranchCode = room.BranchCode
                };
                return dto;
            });

            return response;
        }

        public async Task<ServiceResult<int>> CreateCompanyTraiffMasterDetail(List<CompanyTraiffRequestModel> cmptraiffmaster)
        {
            int successCount = 0;
            foreach (var item in cmptraiffmaster)
            {
                var result = await _masterRepository.CreateCompanyTraiffMasterDetail(item);
                if (result > 0)
                {
                    successCount++;
                }
                else if (result == -1)
                {
                    return ServiceResult<int>.Fail(result, $"Room {item.RoomCode} already exists.");
                }
            }

            return ServiceResult<int>.Created(successCount, $"{successCount} Room detail(s) inserted successfully.");
        }

        public async Task<ServiceResult<int>> UpdateCompanyTraiffMasterDetail(List<CompanyTraiffRequestModel> cmptraiffmaster)
        {
            int totalUpdated = 0;
            foreach (var item in cmptraiffmaster)
            {
                var Updateresult = await _masterRepository.UpdateCompanyTraiffMasterDetail(item);
                if (Updateresult > 0)
                {
                    totalUpdated += Updateresult;
                }
            }

            if (totalUpdated > 0)
            {
                return ServiceResult<int>.Ok(totalUpdated, "Room Detail Updated successfully");
            }

            return ServiceResult<int>.NotFound("Room Detail not found");
        }

        public async Task<ServiceResult<int>> DeleteCompanyTraiffMasterDetail(int CompanyTraiffId, string RoomCode, string CompanyCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteCompanyTraiffMasterDetail(CompanyTraiffId, RoomCode, CompanyCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Room Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Room Detail not found");
            }
        }

        #endregion

        #region RevenueMaster

        public async Task<IEnumerable<RevenueMasterModel>> GetRevenueMasterList(string BranchCode, string PropertyCode)
        {
            var RevenueList = await _masterRepository.GetRevenueMasterList(BranchCode, PropertyCode);

            return RevenueList;
        }

        public async Task<ServiceResult<int>> CreateRevenueMasterDetail(RevenueMasterModel revenuemaster)
        {

            var result = await _masterRepository.CreateRevenueMasterDetail(revenuemaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Revenue Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Revenue Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateRevenueMasterDetail(RevenueMasterModel revenuemaster)
        {
            var UpdateRevenue = await _masterRepository.UpdateRevenueMasterDetail(revenuemaster);
            if (UpdateRevenue > 0)
            {
                return ServiceResult<int>.Ok(UpdateRevenue, $"Revenue Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Revenue Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteRevenueMasterDetail(int RevenueId, string RevenueCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteRevenueMasterDetail(RevenueId, RevenueCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Revenue Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Revenue Detail not found");
            }
        }

        #endregion

        #region StewardMaster

        public async Task<IEnumerable<StewardMasterModel>> GetStewardMasterList(string BranchCode, string PropertyCode)
        {
            var StewardList = await _masterRepository.GetStewardMasterList(BranchCode, PropertyCode);

            return StewardList;
        }

        public async Task<ServiceResult<int>> CreateStewardMasterDetail(StewardMasterModel stewardmaster)
        {

            var result = await _masterRepository.CreateStewardMasterDetail(stewardmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Steward Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Steward Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateStewardMasterDetail(StewardMasterModel stewardmaster)
        {
            var UpdateSteward = await _masterRepository.UpdateStewardMasterDetail(stewardmaster);
            if (UpdateSteward > 0)
            {
                return ServiceResult<int>.Ok(UpdateSteward, $"Steward Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Steward Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteStewardMasterDetail(int StewardId, string StewardCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteStewardMasterDetail(StewardId, StewardCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Steward Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Steward Detail not found");
            }
        }

        #endregion

        #region ItemMaster

        public async Task<IEnumerable<ItemMasterModel>> GetItemMasterList(string BranchCode, string PropertyCode)
        {
            var ItemList = await _masterRepository.GetItemMasterList(BranchCode, PropertyCode);

            return ItemList;
        }

        public async Task<ServiceResult<int>> CreateItemMasterDetail(ItemMasterModel itemmaster)
        {

            var result = await _masterRepository.CreateItemMasterDetail(itemmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Item Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Item Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateItemMasterDetail(ItemMasterModel itemmaster)
        {
            var UpdateItem = await _masterRepository.UpdateItemMasterDetail(itemmaster);
            if (UpdateItem > 0)
            {
                return ServiceResult<int>.Ok(UpdateItem, $"Item Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Item Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteItemMasterDetail(int ItemId, string ItemCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteItemMasterDetail(ItemId, ItemCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Item Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Item Detail not found");
            }
        }

        #endregion

        #region SegmentMaster

        public async Task<IEnumerable<SegmentMasterModel>> GetSegmentMasterList(string BranchCode, string PropertyCode)
        {
            var SegmentList = await _masterRepository.GetSegmentMasterList(BranchCode, PropertyCode);

            return SegmentList;
        }

        public async Task<ServiceResult<int>> CreateSegmentMasterDetail(SegmentMasterModel Segmentmaster)
        {

            var result = await _masterRepository.CreateSegmentMasterDetail(Segmentmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Segment Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Segment Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateSegmentMasterDetail(SegmentMasterModel Segmentmaster)
        {
            var UpdateSegment = await _masterRepository.UpdateSegmentMasterDetail(Segmentmaster);
            if (UpdateSegment > 0)
            {
                return ServiceResult<int>.Ok(UpdateSegment, $"Segment Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Segment Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteSegmentMasterDetail(int SegmentId, string SegmentCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteSegmentMasterDetail(SegmentId, SegmentCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Segment Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Segment Detail not found");
            }
        }

        #endregion

        #region SubSegmentMaster

        public async Task<IEnumerable<SubSegmentMasterModel>> GetSubSegmentMasterList(string BranchCode, string PropertyCode)
        {
            var SubSegmentList = await _masterRepository.GetSubSegmentMasterList(BranchCode, PropertyCode);

            return SubSegmentList;
        }

        public async Task<ServiceResult<int>> CreateSubSegmentMasterDetail(SubSegmentMasterModel SubSegmentmaster)
        {

            var result = await _masterRepository.CreateSubSegmentMasterDetail(SubSegmentmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"SubSegment Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert SubSegment Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateSubSegmentMasterDetail(SubSegmentMasterModel SubSegmentmaster)
        {
            var UpdateSubSegment = await _masterRepository.UpdateSubSegmentMasterDetail(SubSegmentmaster);
            if (UpdateSubSegment > 0)
            {
                return ServiceResult<int>.Ok(UpdateSubSegment, $"SubSegment Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"SubSegment Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteSubSegmentMasterDetail(int SubSegmentId, string SubSegmentCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteSubSegmentMasterDetail(SubSegmentId, SubSegmentCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"SubSegment Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"SubSegment Detail not found");
            }
        }

        #endregion

        #region GuestMaster

        public async Task<IEnumerable<GuestMasterModel>> GetGuestMasterList(string BranchCode, string PropertyCode)
        {
            var GuestList = await _masterRepository.GetGuestMasterList(BranchCode, PropertyCode);

            return GuestList;
        }

        public async Task<ServiceResult<int>> CreateGuestMasterDetail(GuestMasterModel Guestmaster)
        {

            var result = await _masterRepository.CreateGuestMasterDetail(Guestmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Guest Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Guest Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateGuestMasterDetail(GuestMasterModel Guestmaster)
        {
            var UpdateGuest = await _masterRepository.UpdateGuestMasterDetail(Guestmaster);
            if (UpdateGuest > 0)
            {
                return ServiceResult<int>.Ok(UpdateGuest, $"Guest Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Guest Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteGuestMasterDetail(int GuestId, string GuestCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteGuestMasterDetail(GuestId, GuestCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Guest Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Guest Detail not found");
            }
        }

        #endregion

        #region FormCMaster

        public async Task<IEnumerable<FormCResponseDto>> GetFormCMasterList( string BranchCode, string PropertyCode)
        {
            var formCList = await _masterRepository.GetFormCMasterList(BranchCode, PropertyCode);

            var result = formCList.Select(x => new FormCResponseDto
            {
                FormCId = x.FormCId,
                FormCCode = x.FormCCode,
                GuestCode = x.GuestCode,
                CompanyCode = x.CompanyCode ?? string.Empty,
                CheckinNo = x.CheckinNo ?? string.Empty,

                IndiaDetails = new IndianAddressDetail
                {
                    IndiaAddress = x.IndiaAddress ?? string.Empty,
                    IndiaCity = x.IndiaCity ?? string.Empty,
                    IndiaState = x.IndiaState ?? string.Empty,
                    IndiaPinCode = x.IndiaPinCode ?? string.Empty,
                    IndiaCountry = x.IndiaCountry ?? string.Empty
                },

                PermanentDetails = new PermanentAddressDetail
                {
                    PermanentAddress = x.PermanentAddress ?? string.Empty,
                    PermanentCity = x.PermanentCity ?? string.Empty,
                    PermanentState = x.PermanentState ?? string.Empty,
                    PermanentPinCode = x.PermanentPinCode ?? string.Empty,
                    PermanentCountry = x.PermanentCountry ?? string.Empty
                },

                PassportDetails = new PassportDetail
                {
                    PassportNo = x.PassportNo,
                    PassportIssueCity = x.PassportIssueCity ?? string.Empty,
                    PassportIssueCountry = x.PassportIssueCountry ?? string.Empty,
                    PassportIssueDate = x.PassportIssueDate,
                    PassportExpiryDate = x.PassportExpiryDate
                },

                VisaDetails = new VisaDetail
                {
                    VisaNo = x.VisaNo,
                    VisaIssueCity = x.VisaIssueCity ?? string.Empty,
                    VisaIssueCountry = x.VisaIssueCountry ?? string.Empty,
                    VisaIssueDate = x.VisaIssueDate,
                    VisaExpiryDate = x.VisaExpiryDate,
                    VisaType = x.VisaType ?? string.Empty
                },

                ArrivalDetails = new ArrivalDetail
                {
                    ArrivalCity = x.ArrivalCity ?? string.Empty,
                    ArrivalCountry = x.ArrivalCountry ?? string.Empty,
                    ArrivalDate = x.ArrivalDate,
                    HotelArrivalDate = x.HotelArrivalDate,
                    HotelArrivalTime = x.HotelArrivalTime,
                    NoofStay = x.NoofStay,
                    VisitPurpose = x.VisitPurpose ?? string.Empty,
                    DepartureDate = x.DepartureDate
                },

                NextDestinationDetails = new NextDestinationDetail
                {
                    NextDestinationPlace = x.NextDestinationPlace ?? string.Empty,
                    NextDestinationCity = x.NextDestinationCity ?? string.Empty,
                    NextDestinationState = x.NextDestinationState ?? string.Empty,
                    NextDestinationCountry = x.NextDestinationCountry ?? string.Empty
                },

                ContactDetails = new ContactDetail
                {
                    MobileNo = x.MobileNo ?? string.Empty,
                    Email = x.Email ?? string.Empty
                },

                UserCode = x.UserCode,
                IPAddress = x.IPAddress ?? string.Empty,
                PropertyCode = x.PropertyCode,
                BranchCode = x.BranchCode
            });

            return result;
        }

        public async Task<ServiceResult<int>> CreateFormCMasterDetail(FormCRequestModel FormCmaster)
        {

            var result = await _masterRepository.CreateFormCMasterDetail(FormCmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"FormC Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert FormC Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateFormCMasterDetail(FormCRequestModel FormCmaster)
        {
            var UpdateFormC = await _masterRepository.UpdateFormCMasterDetail(FormCmaster);
            if (UpdateFormC > 0)
            {
                return ServiceResult<int>.Ok(UpdateFormC, $"FormC Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"FormC Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteFormCMasterDetail(int FormCId, string GuestCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteFormCMasterDetail(FormCId, GuestCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"FormC Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"FormC Detail not found");
            }
        }

        #endregion

        #region LedgerMaster

        public async Task<IEnumerable<LedgerMasterModel>> GetLedgerMasterList( string BranchCode, string PropertyCode)
        {
            var LedgerList = await _masterRepository.GetLedgerMasterList(BranchCode, PropertyCode);

            return LedgerList;
        }

        public async Task<ServiceResult<int>> CreateLedgerMasterDetail(LedgerMasterModel Ledgermaster)
        {

            var result = await _masterRepository.CreateLedgerMasterDetail(Ledgermaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Ledger Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Ledger Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateLedgerMasterDetail(LedgerMasterModel Ledgermaster)
        {
            var UpdateLedger = await _masterRepository.UpdateLedgerMasterDetail(Ledgermaster);
            if (UpdateLedger > 0)
            {
                return ServiceResult<int>.Ok(UpdateLedger, $"Ledger Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Ledger Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteLedgerMasterDetail(int LedgerId, string LedgerCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteLedgerMasterDetail(LedgerId, LedgerCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Ledger Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Ledger Detail not found");
            }
        }

        #endregion

        #region TaxRulesMaster

        public async Task<IEnumerable<TaxRulesMasterModel>> GetTaxRulesMasterList( string BranchCode, string PropertyCode)
        {
            var TaxRulesList = await _masterRepository.GetTaxRulesMasterList(BranchCode, PropertyCode);

            return TaxRulesList;
        }

        public async Task<ServiceResult<int>> CreateTaxRulesMasterDetail(TaxRulesMasterModel TaxRulesmaster)
        {

            var result = await _masterRepository.CreateTaxRulesMasterDetail(TaxRulesmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"TaxRules Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert TaxRules Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateTaxRulesMasterDetail(TaxRulesMasterModel TaxRulesmaster)
        {
            var UpdateTaxRules = await _masterRepository.UpdateTaxRulesMasterDetail(TaxRulesmaster);
            if (UpdateTaxRules > 0)
            {
                return ServiceResult<int>.Ok(UpdateTaxRules, $"TaxRules Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"TaxRules Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteTaxRulesMasterDetail(int TaxRulesId, string TaxRulesCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteTaxRulesMasterDetail(TaxRulesId, TaxRulesCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"TaxRules Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"TaxRules Detail not found");
            }
        }

        #endregion

        #region DriverMaster

        public async Task<IEnumerable<DriverMasterModel>> GetDriverMasterList( string BranchCode, string PropertyCode)
        {
            var DriverList = await _masterRepository.GetDriverMasterList(BranchCode, PropertyCode);

            return DriverList;
        }

        public async Task<ServiceResult<int>> CreateDriverMasterDetail(DriverMasterModel Drivermaster)
        {

            var result = await _masterRepository.CreateDriverMasterDetail(Drivermaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"Driver Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert Driver Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateDriverMasterDetail(DriverMasterModel Drivermaster)
        {
            var UpdateDriver = await _masterRepository.UpdateDriverMasterDetail(Drivermaster);
            if (UpdateDriver > 0)
            {
                return ServiceResult<int>.Ok(UpdateDriver, $"Driver Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Driver Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteDriverMasterDetail(int DriverId, string DriverCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteDriverMasterDetail(DriverId, DriverCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"Driver Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Driver Detail not found");
            }
        }

        #endregion

        #region BookingSourceMaster

        public async Task<IEnumerable<BookingSourceMasterModel>> GetBookingSourceMasterList( string BranchCode, string PropertyCode)
        {
            var BookingSourceList = await _masterRepository.GetBookingSourceMasterList(BranchCode, PropertyCode);

            return BookingSourceList;
        }

        public async Task<ServiceResult<int>> CreateBookingSourceMasterDetail(BookingSourceMasterModel BookingSourcemaster)
        {

            var result = await _masterRepository.CreateBookingSourceMasterDetail(BookingSourcemaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"BookingSource Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert BookingSource Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateBookingSourceMasterDetail(BookingSourceMasterModel BookingSourcemaster)
        {
            var UpdateBookingSource = await _masterRepository.UpdateBookingSourceMasterDetail(BookingSourcemaster);
            if (UpdateBookingSource > 0)
            {
                return ServiceResult<int>.Ok(UpdateBookingSource, $"BookingSource Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"BookingSource Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteBookingSourceMasterDetail(int BookingSourceId, string BookingSourceCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteBookingSourceMasterDetail(BookingSourceId, BookingSourceCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"BookingSource Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"BookingSource Detail not found");
            }
        }

        #endregion

        #region InstructionDepartmentMaster

        public async Task<IEnumerable<InstructionDepartmentMasterModel>> GetInstructionDepartmentMasterList( string BranchCode, string PropertyCode)
        {
            var InstructionDepartmentList = await _masterRepository.GetInstructionDepartmentMasterList(BranchCode, PropertyCode);

            return InstructionDepartmentList;
        }

        public async Task<ServiceResult<int>> CreateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel InstructionDepartmentmaster)
        {

            var result = await _masterRepository.CreateInstructionDepartmentMasterDetail(InstructionDepartmentmaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"InstructionDepartment Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert InstructionDepartment Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateInstructionDepartmentMasterDetail(InstructionDepartmentMasterModel InstructionDepartmentmaster)
        {
            var UpdateInstructionDepartment = await _masterRepository.UpdateInstructionDepartmentMasterDetail(InstructionDepartmentmaster);
            if (UpdateInstructionDepartment > 0)
            {
                return ServiceResult<int>.Ok(UpdateInstructionDepartment, $"InstructionDepartment Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"InstructionDepartment Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteInstructionDepartmentMasterDetail(int InstructionDepartmentId, string InstructionDepartmentCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteInstructionDepartmentMasterDetail(InstructionDepartmentId, InstructionDepartmentCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"InstructionDepartment Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"InstructionDepartment Detail not found");
            }
        }

        #endregion

        #region SalesExecutiveMaster

        public async Task<IEnumerable<SalesExecutiveMasterModel>> GetSalesExecutiveMasterList( string BranchCode, string PropertyCode)
        {
            var SalesExecutiveList = await _masterRepository.GetSalesExecutiveMasterList(BranchCode, PropertyCode);

            return SalesExecutiveList;
        }

        public async Task<ServiceResult<int>> CreateSalesExecutiveMasterDetail(SalesExecutiveMasterModel SalesExecutivemaster)
        {

            var result = await _masterRepository.CreateSalesExecutiveMasterDetail(SalesExecutivemaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"SalesExecutive Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert SalesExecutive Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateSalesExecutiveMasterDetail(SalesExecutiveMasterModel SalesExecutivemaster)
        {
            var UpdateSalesExecutive = await _masterRepository.UpdateSalesExecutiveMasterDetail(SalesExecutivemaster);
            if (UpdateSalesExecutive > 0)
            {
                return ServiceResult<int>.Ok(UpdateSalesExecutive, $"SalesExecutive Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"SalesExecutive Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteSalesExecutiveMasterDetail(int SalesExecutiveId, string SalesExecutiveCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteSalesExecutiveMasterDetail(SalesExecutiveId, SalesExecutiveCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"SalesExecutive Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"SalesExecutive Detail not found");
            }
        }

        #endregion

        #region ReservationPayModeMaster

        public async Task<IEnumerable<ReservationPayModeMasterModel>> GetReservationPayModeMasterList( string BranchCode, string PropertyCode)
        {
            var ReservationPayModeList = await _masterRepository.GetReservationPayModeMasterList(BranchCode, PropertyCode);

            return ReservationPayModeList;
        }

        public async Task<ServiceResult<int>> CreateReservationPayModeMasterDetail(ReservationPayModeMasterModel ReservationPayModemaster)
        {

            var result = await _masterRepository.CreateReservationPayModeMasterDetail(ReservationPayModemaster);

            if (result > 0)
            {
                return ServiceResult<int>.Created(result, $"ReservationPayMode Detail inserted successfully");
            }
            else
            {
                return ServiceResult<int>.Fail(result, $"Failed to insert ReservationPayMode Detail");
            }
        }

        public async Task<ServiceResult<int>> UpdateReservationPayModeMasterDetail(ReservationPayModeMasterModel ReservationPayModemaster)
        {
            var UpdateReservationPayMode = await _masterRepository.UpdateReservationPayModeMasterDetail(ReservationPayModemaster);
            if (UpdateReservationPayMode > 0)
            {
                return ServiceResult<int>.Ok(UpdateReservationPayMode, $"ReservationPayMode Detail Updated successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"ReservationPayMode Detail not found");
            }
        }

        public async Task<ServiceResult<int>> DeleteReservationPayModeMasterDetail(int ReservationPayModeId, string ReservationPayModeCode, string PropertyCode, string BranchCode)
        {
            var result = await _masterRepository.DeleteReservationPayModeMasterDetail(ReservationPayModeId, ReservationPayModeCode, PropertyCode, BranchCode);
            if (result > 0)
            {
                return ServiceResult<int>.Ok(result, $"ReservationPayMode Detail deleted successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"ReservationPayMode Detail not found");
            }
        }

        #endregion

        #region Common Method

        public async Task<ServiceResult<string>> GetNextMasterCode(string TableName, string CodeColumn, string Prefix, string PropertyCode, string BranchCode)
        {
            var nextRowId = await _commonMethod.GetNextMasterCode(TableName, CodeColumn, Prefix, PropertyCode, BranchCode);

            return ServiceResult<string>.Ok(nextRowId, $"Next {CodeColumn} Master ID Generated successfully");
        }

        #endregion
    }
}
