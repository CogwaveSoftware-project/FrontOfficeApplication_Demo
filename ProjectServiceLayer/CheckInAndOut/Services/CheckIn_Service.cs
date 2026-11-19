using DocumentFormat.OpenXml.Office2016.Excel;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.CheckInAndOut;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.Master.Interfaces;
using System.Data;

namespace FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Services
{
    public class CheckIn_Service(ICheckIn_Repository checkInRepository, CommonMethod commonMethod, IDbConnectionFactory factory, IMaster_Repository masterRepository) : ICheckIn_Service
    {
        private readonly ICheckIn_Repository _checkInRepository = checkInRepository;

        private readonly IMaster_Repository _masterRepository = masterRepository;

        private readonly CommonMethod _commonMethod = commonMethod;
        private readonly IDbConnectionFactory _factory = factory;

        public async Task<ServiceResult<string>> CreateCheckInDetail(CheckInRequestModel ChechinDetail)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);
            // Use async open if it's a DbConnection
            if (connection is System.Data.Common.DbConnection dbConnection)
            {
                await dbConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();

            try
            {
                bool reservationExists = false;
                string PrimaryGuestCode = string.Empty;
                string GRCNo = string.Empty;

                // Helper local function to validate each step immediately and abort on failure
                static void ValidateResult(CheckInSaveResult result, string stepName)
                {
                    if (result == null || result.StatusCode <= 0)
                    {
                        throw new InvalidOperationException($"Failed at {stepName}: {result?.Message ?? "Unknown database error."}");
                    }
                }

                var finDetails = await _commonMethod.GetFinancialMaster(ChechinDetail.BranchCode, ChechinDetail.PropertyCode);
                int finCode = finDetails?.FinCode ?? 0;

                var primary = ChechinDetail.GuestDetails.FirstOrDefault(g => g.IsPrimaryGuest) ?? ChechinDetail.GuestDetails.FirstOrDefault();
                if (primary == null || string.IsNullOrWhiteSpace(primary.GuestName))
                    return ServiceResult<string>.Fail(null, "Guest name is required.");

                if (ChechinDetail.BookingDetail == null || ChechinDetail.BookingDetail.Count == 0)
                    return ServiceResult<string>.Fail(null, "Select at least one room.");
                if (string.IsNullOrWhiteSpace(primary.GuestNation) || primary.GuestNation.Equals("string", StringComparison.OrdinalIgnoreCase))
                    primary.GuestNation = "INDIA";

                // 1. Codes, same helpers reservation uses
                if (!ChechinDetail.IsExistingCheckIn && ChechinDetail.CheckInId == 0)
                {
                    //if (string.IsNullOrWhiteSpace(primary.GuestCode) || primary.GuestCode.Equals("string", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    primary.GuestCode = await _commonMethod.GetNextMasterCode( 
                    //        "Tbl_Guest_Master", "GuestCode", "GC", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);
                    //}
                    if (string.IsNullOrWhiteSpace(ChechinDetail.CheckInNo) || ChechinDetail.CheckInNo.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        var checkInNo = await _commonMethod.GetNextTransactionFinCode(
                            "Tbl_CheckIn_Master", "CheckInNo", "CN", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);
                        if (checkInNo == null)
                            return ServiceResult<string>.Fail(null, "No FinancialYear found for this Branch");
                        ChechinDetail.CheckInNo = checkInNo;
                    }
                    if (string.IsNullOrWhiteSpace(ChechinDetail.GRCNo) || ChechinDetail.GRCNo.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        var grcNo = await _commonMethod.GetNextTransactionFinCode(
                            "Tbl_GRC_Bookings", "GRCNo", "GRC", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);
                        if (grcNo == null)
                            return ServiceResult<string>.Fail(null, "No FinancialYear found for this Branch");
                        ChechinDetail.GRCNo = grcNo;
                    }
                }

                if (string.IsNullOrWhiteSpace(ChechinDetail.GroupCode))
                {
                    var name = primary.GuestName.Trim();
                    var prefix = name.Length >= 2 ? name[..2] : name;
                    ChechinDetail.GroupCode = $"{prefix.ToUpperInvariant()}GP{ChechinDetail.CheckInNo}";
                }

                // 2. Duplicate check on the check-in number
                if (!ChechinDetail.IsExistingCheckIn && ChechinDetail.CheckInId == 0)
                {
                    var exists = await _checkInRepository.CheckExistingCheckIn( ChechinDetail.CheckInNo, ChechinDetail.PropertyCode,
                        ChechinDetail.BranchCode, connection, transaction);
                    if (exists)
                    {
                        transaction.Rollback();
                        return ServiceResult<string>.Fail(ChechinDetail.CheckInNo, "Check-in information already exists");
                    }
                }

                CheckInSaveResult guestResult = null;
                // 1. Initialize Codes if New CheckIn
                foreach (var guest in ChechinDetail.GuestDetails)
                {
                    //if (!ChechinDetail.IsExistingCheckIn && (string.IsNullOrEmpty(item.GuestCode) || item.GuestCode.Equals("string", StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    string guestCode = await _commonMethod.GetNextMasterCode("Tbl_Guest_Master", "GuestCode", "GC", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);

                    //    item.GuestCode = guestCode;

                    //    if (item.IsPrimaryGuest)
                    //    {
                    //        PrimaryGuestCode = guestCode;
                    //    }

                    //     1. Guest Procedure
                    //    var guestResult = await _checkInRepository.SaveUpdate_GuestProcedure(item, ChechinDetail, connection, transaction);
                    //    ValidateResult(guestResult, "Guest Save");

                    //}

                    if (guest.IsPrimaryGuest)
                    {
                        PrimaryGuestCode = guest.GuestCode;
                    }

                    // 1. Guest Procedure
                    guestResult = await _checkInRepository.SaveUpdate_GuestProcedure(guest, ChechinDetail, connection, transaction);
                    ValidateResult(guestResult, "Guest details Saved");

                    if (guest.IsPrimaryGuest && (string.IsNullOrEmpty(guest.CompanyName) || guest.CompanyName.Equals("string", StringComparison.OrdinalIgnoreCase)))
                    {
                        string companyCode = await _commonMethod.GetNextMasterCode("Tbl_Company_Master", "CompanyCode", "CC", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);

                        guest.CompanyCode = companyCode;

                        var companyResult = await _checkInRepository.SaveUpdate_CompanyMasterProcedure(guest, ChechinDetail, connection, transaction);
                        ValidateResult(companyResult, "Company details Saved");
                    }
                }

                // 2. Form C, only when the client sent rows
                foreach (var form in ChechinDetail.FormCDetails)
                {
                    if (string.IsNullOrWhiteSpace(form.FormCCode) || form.FormCCode.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        string FormCCode = await _commonMethod.GetNextMasterCode("Tbl_FormC_Master", "FormCCode", "FC", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);

                        form.FormCCode = FormCCode;
                    }

                    form.GuestCode = string.IsNullOrWhiteSpace(form.GuestCode) ? primary.GuestCode : form.GuestCode;
                    form.CheckinNo = ChechinDetail.CheckInNo;

                    var FormCResult = await _checkInRepository.SaveUpdate_FormCProcedure(form, ChechinDetail, connection, transaction);
                    ValidateResult(FormCResult, "FormC details Saved");
                }

                // 3. GRC Master Procedure
                var GRCResult = await _checkInRepository.SaveUpdate_GRCBookingProcedure(ChechinDetail.GRCNo, PrimaryGuestCode, ChechinDetail, connection, transaction);
                ValidateResult(GRCResult, "GRCResult Master Saved");

                // 4. Driver stays on the existing master call
                if (ChechinDetail.DriverDetails != null && !string.IsNullOrWhiteSpace(ChechinDetail.DriverDetails.DriverName) 
                    && !ChechinDetail.DriverDetails.DriverName.Equals("string", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(ChechinDetail.DriverDetails.DriverCode) ||
                        ChechinDetail.DriverDetails.DriverCode.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        ChechinDetail.DriverDetails.DriverCode = await _commonMethod.GetNextTransactionFinCode(
                            "Tbl_Driver_Master", "DriverCode", "DRV", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);
                    }
                    await _masterRepository.CreateDriverMasterDetail(ChechinDetail.DriverDetails);
                }

                //if (!ChechinDetail.IsExistingCheckIn && string.IsNullOrEmpty(ChechinDetail.CheckInNo) || ChechinDetail.CheckInNo.Equals("string", StringComparison.OrdinalIgnoreCase))
                //{
                //    string? NextCode = await _commonMethod.GetNextTransactionFinCode("Tbl_CheckIn_Master", "CheckInNo", "CN", ChechinDetail.PropertyCode, ChechinDetail.BranchCode);
                //    if (NextCode != null)
                //    {
                //        ChechinDetail.CheckInNo = NextCode;
                //    }
                //    else
                //    {
                //        return ServiceResult<string>.Fail(null, $"No FinancialYear found for this Branch");
                //    }
                //}

                // 5. CheckIn, slaves, inventory, channel
                var checkInResult = await _checkInRepository.SaveUpdate_CheckInProcedure(ChechinDetail, connection, transaction);
                ValidateResult(checkInResult, "Check-in Master Save");

                if(!string.IsNullOrWhiteSpace(ChechinDetail.ReservationNo) && ChechinDetail.DiscountValue > 0)
                {
                    var DiscountId = await _checkInRepository.InsertDiscountValue(ChechinDetail, connection, transaction);
                }

                var slaveResult = await _checkInRepository.SaveUpdate_CheckInSlaveProcedure(ChechinDetail, connection, transaction);
                ValidateResult(slaveResult, "Check-in Slave Save");

                var foreverResult = await _checkInRepository.SaveUpdate_ForeverDataProcedure(ChechinDetail, connection, transaction);
                ValidateResult(foreverResult, "Forever Data Save");

                var channelResult = await _checkInRepository.SaveUpdate_ChannelManagerProcedure(ChechinDetail, connection, transaction);
                ValidateResult(channelResult, "Channel Manager Save");

                // 6. Advance Payment Details
                if (ChechinDetail.PaymentDetail.Any(p => p.AmountPaid > 0))
                {
                    var advanceResult = await _checkInRepository.SaveUpdate_AdvanceReceiptProcedure(ChechinDetail, connection, transaction);
                    ValidateResult(advanceResult, "Advance Receipt Save");
                }

                // 7. Tax Details
                if (ChechinDetail.BookingDetail != null)
                {
                    foreach (var booking in ChechinDetail.BookingDetail)
                    {
                        if (booking.TaxCalculationDetails != null)
                        {
                            var taxResult = await _checkInRepository.SaveUpdate_TaxDetailsProcedure(booking.TaxCalculationDetails, ChechinDetail, connection, transaction);
                            ValidateResult(taxResult, "Tax Details Save");
                        }
                    }
                }

                if (ChechinDetail.IsCheckInExtraCharged)
                {
                    var chargeResult = await _checkInRepository.SaveUpdate_CheckInChargesProcedure(ChechinDetail, connection, transaction);
                    ValidateResult(chargeResult, "Check-in Charges Save");
                }
                if (ChechinDetail.IsSpecialInstruction)
                {
                    var instructionResult = await _checkInRepository.SaveUpdate_SpecialInstructionProcedure(ChechinDetail, connection, transaction);
                    ValidateResult(instructionResult, "Special Instructions Save");
                }


                if (guestResult.StatusCode > 0 && checkInResult.StatusCode > 0)
                {
                    string conresult = $"{guestResult.InsertId}{checkInResult.InsertId}";
                    transaction.Commit();
                    return ServiceResult<string>.Created(conresult, $"Checkin Detail inserted successfully");
                }
                else
                {
                    transaction.Rollback();
                    //bool result = await _checkInRepository.DeleteMasterRecord(ChechinDetail.ReservationNo, ChechinDetail.PropertyCode, ChechinDetail.BranchCode, connection, transaction);
                    return ServiceResult<string>.Fail(ChechinDetail.ReservationNo, $"Roll-Back Operation Performed due to execution failure");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ServiceResult<string>.Fail(null, $"Error: {ex.Message}");
            }
        }
    }
}
