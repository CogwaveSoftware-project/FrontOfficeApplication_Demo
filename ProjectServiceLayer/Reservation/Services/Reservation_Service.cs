using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.GST;
using FrontOfficeApplication.ProjectEntitiesModel.Master;
using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectEntitiesModel.Responses;
using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.DTOs;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Transactions;

namespace FrontOfficeApplication.ProjectServiceLayer.Reservation.Services
{
    public class Reservation_Service(IReservation_Repository reservationRepository, CommonMethod commonMethod, IDbConnectionFactory factory) : IReservation_Service
    {
        private readonly IReservation_Repository _reservationRepository = reservationRepository;
        private readonly CommonMethod _commonMethod = commonMethod;
        private readonly IDbConnectionFactory _factory = factory;

        //public Reservation_Service(IReservation_Repository reservationRepository, CommonMethod commonMethod, IDbConnectionFactory factory)
        //{
        //    _reservationRepository = reservationRepository;
        //    _commonMethod = commonMethod;
        //    _factory = factory;
        //}

        #region ReservationTransaction

        public List<string> GetChartAvailability(int nDays, DateTime checkInDate, string PropertyCode, string BranchCode)
        {
            if (nDays <= 0)
                throw new ArgumentException("NDays must be greater than zero.");

            var dates = new List<string>();

            for (int i = 0; i < nDays; i++)
            {
                dates.Add(checkInDate.Date.AddDays(i).ToString("dd/MM/yyyy"));
            }

            return dates;
        }

        public async Task<IEnumerable<RoomAvailability>> GetRoomAvailability( int noDays, DateTime checkInDate, string PropertyCode, string BranchCode)
        {
            if (noDays <= 0)
                throw new ArgumentException( "Number of days must be greater than zero.");

            var rooms = await _reservationRepository.GetRoomAllocated(PropertyCode, BranchCode);

            var result = new List<RoomAvailability>();

            foreach (var room in rooms)
            {
                var roomAvailability = new RoomAvailability
                {
                    RoomCode = room.RoomCode,
                    TotalRooms = room.Allocated
                };

                for (int j = 0; j < noDays; j++)
                {
                    DateTime currentDate = checkInDate.Date.AddDays(j);

                    DateTime nextDate = currentDate.AddDays(1);

                    int occupiedRooms = await _reservationRepository.GetOccupiedRooms( room.RoomCode, currentDate, nextDate, PropertyCode, BranchCode);

                    int availableRooms = room.Allocated - occupiedRooms;

                    if (availableRooms < 0)
                        availableRooms = 0;

                    roomAvailability.Dates.Add(
                        new RoomAvailabilityDate
                        {
                            Date = currentDate,
                            AvailableRooms = availableRooms
                        });
                }

                result.Add(roomAvailability);
            }

            return result;
        }

        public async Task<IEnumerable<TaxDetailsResponseDto>> GetTaxDetails(string PropertyCode, string BranchCode)
        {
            var flatTaxDetails = await _reservationRepository.GetTaxDetails(PropertyCode, BranchCode);

            var groupedTaxes = flatTaxDetails
                .GroupBy(t => new { t.TaxCode, t.TaxName, t.PropertyCode, t.BranchCode })
                .Select(g => new TaxDetailsResponseDto
                {
                    TaxCode = g.Key.TaxCode,
                    TaxName = g.Key.TaxName,
                    PropertyCode = g.Key.PropertyCode,
                    BranchCode = g.Key.BranchCode,
                    TaxDescriptions = g.Select(td => new TaxDescriptionDto
                    {
                        TaxDescriptionCode = td.TaxDescriptionCode,
                        TaxDescriptionName = td.TaxDescriptionName,
                        TaxPercentage = td.TaxPercentage,
                        FromAmount = td.FromAmount,
                        ToAmount = td.ToAmount,
                        FromDate = td.FromDate,
                        ToDate = td.ToDate
                    }).ToList()
                })
                .ToList();

            return groupedTaxes;
        }

        public async Task<TaxRulesDetailsModel> GetTaxRulesDetails(string BranchCode, string PropertyCode)
        {
            var TaxRulesDetails = await _reservationRepository.GetTaxRulesDetails(PropertyCode, BranchCode);

            return TaxRulesDetails;
        }

        public async Task<IEnumerable<PlanDetailsModel>> GetPlanDetails(string PropertyCode, string BranchCode)
        {
            var PlanDetails = await _reservationRepository.GetPlanDetails(PropertyCode, BranchCode);

            return PlanDetails;
        }

        public async Task<IEnumerable<RoomTraiffDetailResponseDto>> GetRoomTraiffDetails(string PropertyCode, string BranchCode)
        {
            var RoomsDetails = await _reservationRepository.GetRoomTraiffDetails(PropertyCode, BranchCode);

            var response = RoomsDetails.Select(room =>
            {
                var dto = new RoomTraiffDetailResponseDto
                {
                    RoomCode = room.RoomCode,
                    RoomName = room.RoomName,
                    Allocated = room.Allocated,
                    RoomPlanType = room.RoomPlanType,
                    DefaultTraiff = room.DefaultTraiff,
                    Pax = room.PAX,
                    ExtraAdult = room.ExtraAdult,
                    ExtraChild = room.ExtraChild,
                    PaxRates = JsonSerializer.Deserialize<List<PlanRatedetailsDto>>(room.PAXRates) ?? [],
                    PropertyCode = room.PropertyCode,
                    BranchCode = room.BranchCode
                };

                return dto;
            });

            return response;
        }

        public async Task<IEnumerable<CompanyTraiffDetailResponseDto>> GetCompanyTraiffDetails(string PropertyCode, string BranchCode)
        {
            var RoomsDetails = await _reservationRepository.GetCompanyTraiffDetails(PropertyCode, BranchCode);

            var response = RoomsDetails.Select(room =>
            {
                var dto = new CompanyTraiffDetailResponseDto
                {
                    CompanyCode = room.CompanyCode,
                    RoomCode = room.RoomCode,
                    TraiffCategory = room.TraiffCategory,
                    RoomPlanType = room.RoomPlanType,
                    DefaultTraiff = room.DefaultTraiff,
                    Pax = room.PAX,
                    ExtraAdult = room.ExtraAdult,
                    ExtraChild = room.ExtraChild,
                    PaxRates = JsonSerializer.Deserialize<List<PlanRatedetailsDto>>(room.PAXRates) ?? [],
                    PropertyCode = room.PropertyCode,
                    BranchCode = room.BranchCode
                };

                return dto;
            });

            return response;
        }

        public async Task<ReservationDetailsCombinedResponseDto> GetCombinedReservationDetails(string PropertyCode, string BranchCode)
        {
            // 1. Fetch all three data sources concurrently for better performance
            var planDetailsTask = _reservationRepository.GetPlanDetails(PropertyCode, BranchCode);
            var TaxDetailsTask = _reservationRepository.GetTaxDetails(PropertyCode, BranchCode);
            var TaxRulesDetailsTask = _reservationRepository.GetTaxRulesDetails(PropertyCode, BranchCode);
            var roomDetailsTask = _reservationRepository.GetRoomTraiffDetails(PropertyCode, BranchCode);
            var companyDetailsTask = _reservationRepository.GetCompanyTraiffDetails(PropertyCode, BranchCode);

            await Task.WhenAll(planDetailsTask, TaxDetailsTask, TaxRulesDetailsTask, roomDetailsTask, companyDetailsTask);

            // 2. Map Room Tariff Details
            var groupedTaxes = TaxDetailsTask.Result
                .GroupBy(t => new { t.TaxCode, t.TaxName, t.PropertyCode, t.BranchCode })
                .Select(g => new TaxDetailsResponseDto
                {
                    TaxCode = g.Key.TaxCode,
                    TaxName = g.Key.TaxName,
                    PropertyCode = g.Key.PropertyCode,
                    BranchCode = g.Key.BranchCode,
                    TaxDescriptions = g.Select(td => new TaxDescriptionDto
                    {
                        TaxDescriptionCode = td.TaxDescriptionCode,
                        TaxDescriptionName = td.TaxDescriptionName,
                        TaxPercentage = td.TaxPercentage,
                        FromAmount = td.FromAmount,
                        ToAmount = td.ToAmount,
                        FromDate = td.FromDate,
                        ToDate = td.ToDate
                    }).ToList()
                });

            // 3. Map Room Tariff Details
            var roomTariffResponse = roomDetailsTask.Result.Select(room => new RoomTraiffDetailResponseDto
            {
                RoomCode = room.RoomCode,
                RoomName = room.RoomName,
                Allocated = room.Allocated,
                RoomPlanType = room.RoomPlanType,
                DefaultTraiff = room.DefaultTraiff,
                Pax = room.PAX,
                ExtraAdult = room.ExtraAdult,
                ExtraChild = room.ExtraChild,
                PaxRates = JsonSerializer.Deserialize<List<PlanRatedetailsDto>>(room.PAXRates) ?? [],
                PropertyCode = room.PropertyCode,
                BranchCode = room.BranchCode
            });

            // 4. Map Company Tariff Details
            var companyTariffResponse = companyDetailsTask.Result.Select(room => new CompanyTraiffDetailResponseDto
            {
                CompanyCode = room.CompanyCode,
                RoomCode = room.RoomCode,
                TraiffCategory = room.TraiffCategory,
                RoomPlanType = room.RoomPlanType,
                DefaultTraiff = room.DefaultTraiff,
                Pax = room.PAX,
                ExtraAdult = room.ExtraAdult,
                ExtraChild = room.ExtraChild,
                PaxRates = JsonSerializer.Deserialize<List<PlanRatedetailsDto>>(room.PAXRates) ?? [],
                PropertyCode = room.PropertyCode,
                BranchCode = room.BranchCode
            });

            // 5. Return the combined container object
            return new ReservationDetailsCombinedResponseDto
            {
                PlanDetails = planDetailsTask.Result,
                TaxRulesDetails = TaxRulesDetailsTask.Result,
                TaxDetails = groupedTaxes,
                RoomTariffDetails = roomTariffResponse,
                CompanyTariffDetails = companyTariffResponse
            };
        }

        //public async Task<IEnumerable<ReservationPayModeMasterModel>> GetReservationPayModeMasterList(string BranchCode, string PropertyCode)
        //{
        //    var ReservationPayModeList = await _reservationRepository.GetReservationPayModeMasterList(BranchCode, PropertyCode);

        //    return ReservationPayModeList;
        //}

        //public async Task<ServiceResult<string>> CreateReservationDetail(ReservationRequestModel ReservationDetail)
        //{
        //    using var connection = await _factory.CreateConnection(DbNames.HMS);
        //    connection.Open();

        //    using var transaction = connection.BeginTransaction();

        //    string currentGuestCode = string.Empty;
        //    string currentReservationCode = string.Empty;
        //    string currentReservationSlaveCode = string.Empty;

        //    try
        //    {
        //        if (ReservationDetail.IsExistingReservation == false)
        //        {
        //            currentGuestCode = await _commonMethod.GetNextMasterCode("Tbl_Guest_Master", "GuestCode", "GC", ReservationDetail.PropertyCode, ReservationDetail.BranchCode);
        //            currentReservationCode = await _commonMethod.GetNextMasterCode("Tbl_Reservation_Master", "ReservationNo", "RC", ReservationDetail.PropertyCode, ReservationDetail.BranchCode);

        //        }

        //        FinancialMasterModel FinDetails = await _commonMethod.GetFinancialMasterList(ReservationDetail.BranchCode, ReservationDetail.PropertyCode);

        //        ReservationDetail.GuestDetail.GuestCode = ReservationDetail.GuestDetail.GuestCode == string.Empty ? $"{FinDetails.FinCode}{currentGuestCode}" : ReservationDetail.GuestDetail.GuestCode;
        //        var Guestresult = await _reservationRepository.SaveUpdate_GuestProcedure(ReservationDetail, connection, transaction);

        //        // 1. Handle Deletion / Amending Mode
        //        if (ReservationDetail.IsAmend == true)
        //        {
        //           await _reservationRepository.DeleteReservation(ReservationDetail.ReservationNo, ReservationDetail.PropertyCode, ReservationDetail.BranchCode, connection, transaction);

        //        }
        //        else
        //        {
        //            ReservationDetail.ReservationNo = ReservationDetail.ReservationNo == string.Empty ? $"{FinDetails.FinCode}{currentReservationCode}" : ReservationDetail.ReservationNo;

        //            //currentReservationCode = $"{FinDetails.FinCode}{ReservationDetail.ReservationNo}{nextRowId:D6}";
        //        }

        //        // Check if record exists for new inserts
        //        if (await _reservationRepository.CheckExistingReservation(ReservationDetail.ReservationNo, ReservationDetail.PropertyCode, ReservationDetail.BranchCode, connection, transaction))
        //        {
        //            transaction.Rollback();
        //            return ServiceResult<string>.Fail(ReservationDetail.ReservationNo, $"Reservation Information Already Exists");
        //        }

        //        // 2. Execute Stored Procedure
        //        var Reservationresult = await _reservationRepository.SaveUpdate_ReservationProcedure(ReservationDetail, connection, transaction);

        //        var ReservationSlaveresult = await _reservationRepository.SaveUpdate_ReservationSlaveProcedure(ReservationDetail, connection, transaction);

        //        var ForeverDataresult = await _reservationRepository.SaveUpdate_ForeverDataProcedure(ReservationDetail, connection, transaction);

        //        var ChannelManagerresult = await _reservationRepository.SaveUpdate_ChannelManagerProcedure(ReservationDetail, connection, transaction);

        //        var AdvanceReceiptresult = await _reservationRepository.SaveUpdate_AdvanceReceiptProcedure(ReservationDetail, connection, transaction);

        //        foreach (var booking in ReservationDetail.BookingDetail)
        //        {
        //            if (booking.TaxCalculationDetails != null)
        //            {
        //                var taxResult = await _reservationRepository.SaveUpdate_TaxDetailsProcedure( booking.TaxCalculationDetails, ReservationDetail, connection, transaction );
        //            }
        //        }

        //        var ReservationChargesresult = await _reservationRepository.SaveUpdate_ReservationChargesProcedure(ReservationDetail, connection, transaction);

        //        var SpecialInstructionresult = await _reservationRepository.SaveUpdate_SpecialInstructionProcedure(ReservationDetail, connection, transaction);


        //        if (Guestresult.StatusCode > 0 && Reservationresult.StatusCode > 0)
        //        {
        //            string conresult = $"{Guestresult.InsertId}{Reservationresult.InsertId}";

        //            transaction.Commit();
        //            return ServiceResult<string>.Created(conresult, $"Reservation Detail inserted successfully");

        //        }
        //        else
        //        {
        //            transaction.Rollback();
        //            bool  result = await _reservationRepository.DeleteMasterRecord(ReservationDetail.ReservationNo, ReservationDetail.PropertyCode, ReservationDetail.BranchCode, connection, transaction);
        //            return ServiceResult<string>.Fail(ReservationDetail.ReservationNo, $"Roll-Back Operation Performed due to execution failure");
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        transaction.Rollback();
        //        return ServiceResult<string>.Fail(null, $"Error: {ex.Message}");
        //    }
        //}

        public async Task<ServiceResult<string>> CreateReservationDetail(ReservationRequestModel reservationDetail)
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

                // Helper local function to validate each step immediately and abort on failure
                static void ValidateResult(ReservationSaveResult result, string stepName)
                {
                    if (result == null || result.StatusCode <= 0)
                    {
                        throw new InvalidOperationException($"Failed at {stepName}: {result?.Message ?? "Unknown database error."}");
                    }
                }

                // 1. Initialize Codes if New Reservation
                if (!reservationDetail.IsExistingReservation && (reservationDetail.ReservationNo == null || reservationDetail.ReservationId == 0))
                {
                    var finDetails = await _commonMethod.GetFinancialMaster(reservationDetail.BranchCode, reservationDetail.PropertyCode);
                    int finCode = finDetails?.FinCode ?? 0;

                    if (string.IsNullOrWhiteSpace(reservationDetail.GuestDetail.GuestCode) || reservationDetail.GuestDetail.GuestCode.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        string guestCode = await _commonMethod.GetNextMasterCode("Tbl_Guest_Master", "GuestCode", "GC", reservationDetail.PropertyCode, reservationDetail.BranchCode);
                        reservationDetail.GuestDetail.GuestCode = $"{guestCode}";
                    }

                    if (string.IsNullOrEmpty(reservationDetail.ReservationNo)|| reservationDetail.ReservationNo.Equals("string", StringComparison.OrdinalIgnoreCase))
                    {
                        string? reservationCode = await _commonMethod.GetNextTransactionFinCode("Tbl_Reservation_Master", "ReservationNo", "RC", reservationDetail.PropertyCode, reservationDetail.BranchCode);
                        if (reservationCode != null)
                        {
                            reservationDetail.ReservationNo = reservationCode;
                        }
                        else
                        {
                            return ServiceResult<string>.Fail(null, $"No FinancialYear found for this Branch");
                        }
                    }
                }

                // 2. Handle Amendment / Deletion Mode
                if (reservationDetail.IsAmend == true)
                {
                    await _reservationRepository.DeleteReservation(reservationDetail.ReservationNo, reservationDetail.PropertyCode, reservationDetail.BranchCode, connection, transaction);
                }
                else if(!reservationDetail.IsExistingReservation && reservationDetail.ReservationId == 0)
                {
                    // 3. Pre-check for Existing Reservations
                    reservationExists = await _reservationRepository.CheckExistingReservation(reservationDetail.ReservationNo, reservationDetail.PropertyCode, reservationDetail.BranchCode, connection, transaction);
                }
                
                if (reservationExists)
                {
                    transaction.Rollback();
                    return ServiceResult<string>.Fail(reservationDetail.ReservationNo, "Reservation Information Already Exists");
                }

                // 4. Execute Core Persistence Operations
                // 1. Guest Procedure
                var guestResult = await _reservationRepository.SaveUpdate_GuestProcedure(reservationDetail, connection, transaction);
                ValidateResult(guestResult, "Guest Save");

                // 2. Reservation Master Procedure
                var reservationResult = await _reservationRepository.SaveUpdate_ReservationProcedure(reservationDetail, connection, transaction);
                ValidateResult(reservationResult, "Reservation Master Save");

                // 3. Reservation Slave Procedure
                var reservationSlaveResult = await _reservationRepository.SaveUpdate_ReservationSlaveProcedure(reservationDetail, connection, transaction);
                ValidateResult(reservationSlaveResult, "Reservation Slave Save");

                // 4. Forever Data Procedure
                var foreverDataResult = await _reservationRepository.SaveUpdate_ForeverDataProcedure(reservationDetail, connection, transaction);
                ValidateResult(foreverDataResult, "Forever Data Save");

                // 5. Channel Manager Procedure
                var channelManagerResult = await _reservationRepository.SaveUpdate_ChannelManagerProcedure(reservationDetail, connection, transaction);
                ValidateResult(channelManagerResult, "Channel Manager Save");

                // 6. Advance Receipt Procedure
                var advanceReceiptResult = await _reservationRepository.SaveUpdate_AdvanceReceiptProcedure(reservationDetail, connection, transaction);
                ValidateResult(advanceReceiptResult, "Advance Receipt Save");

                // 7. Tax Details
                if (reservationDetail.BookingDetail != null)
                {
                    foreach (var booking in reservationDetail.BookingDetail)
                    {
                        if (booking.TaxCalculationDetails != null)
                        {
                            var taxResult = await _reservationRepository.SaveUpdate_TaxDetailsProcedure(booking.TaxCalculationDetails, reservationDetail, connection, transaction);
                            ValidateResult(taxResult, "Tax Details Save");
                        }
                    }
                }

                if (reservationDetail.IsReservationExtraCharged)
                {
                    // 8. Reservation Charges
                    var reservationChargesResult = await _reservationRepository.SaveUpdate_ReservationChargesProcedure(reservationDetail, connection, transaction);
                    ValidateResult(reservationChargesResult, "Reservation Charges Save");
                }

                if (reservationDetail.IsSpecialInstruction)
                {
                    // 9. Special Instructions
                    var specialInstructionResult = await _reservationRepository.SaveUpdate_SpecialInstructionProcedure(reservationDetail, connection, transaction);
                    ValidateResult(specialInstructionResult, "Special Instructions Save");
                }
                
                if (guestResult.StatusCode > 0 && reservationResult.StatusCode > 0)
                {
                    string conresult = $"{guestResult.InsertId}{reservationResult.InsertId}";
                    transaction.Commit();
                    return ServiceResult<string>.Created(conresult, $"Reservation Detail inserted successfully");
                }
                else
                {
                    transaction.Rollback();
                    bool result = await _reservationRepository.DeleteMasterRecord(reservationDetail.ReservationNo, reservationDetail.PropertyCode, reservationDetail.BranchCode, connection, transaction);
                    return ServiceResult<string>.Fail(reservationDetail.ReservationNo, $"Roll-Back Operation Performed due to execution failure");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ServiceResult<string>.Fail(null, $"Error: {ex.Message}");
            }
        }

        public async Task<ServiceResult<int>> BookingCancellationDetail(BookingCancellationModel requestDetail)
        {
            var UpdateResult = await _reservationRepository.BookingCancellationDetail(requestDetail);
            if (UpdateResult > 0)
            {
                return ServiceResult<int>.Ok(UpdateResult, $"Cancelled Reservation successfully");
            }
            else
            {
                return ServiceResult<int>.NotFound($"Reservation Detail not found");
            }
        }

        public async Task<IEnumerable<GuestSearchDetailModel>> GetGuestSearchList(string SearchingName, string PropertyCode, string BranchCode)
        {
            var SearchDetails = await _reservationRepository.GetGuestSearchList(SearchingName, PropertyCode, BranchCode);

            return SearchDetails;
        }

        public async Task<ReservationCompleteDetailsDto> GetReservationCompleteDetails(string GuestCode, string ReservationNo, string PropertyCode, string BranchCode)
        {
            // 1. Fetch all data sources concurrently for better performance
            var GuestDetailsTask = _reservationRepository.GetGuestDetails(GuestCode, PropertyCode, BranchCode);
            var ReservationDetailsTask = _reservationRepository.GetReservationMasterDetails(GuestCode, ReservationNo, PropertyCode, BranchCode);
            var SlaveDetailsTask = _reservationRepository.GetReservationSlaveDetails(GuestCode, ReservationNo, PropertyCode, BranchCode);
            var ForEverDetailsTask = _reservationRepository.GetForEverDataDetails(ReservationNo, PropertyCode, BranchCode);
            var ReceiptDetailsTask = _reservationRepository.GetAdvanceReceiptDetails(GuestCode, ReservationNo, PropertyCode, BranchCode);
            var ChargeDetailsTask = _reservationRepository.GetReservationChargeDetails(GuestCode, ReservationNo, PropertyCode, BranchCode);
            var InstructionDetailsTask = _reservationRepository.GetSpecialInstructionDetails(GuestCode, ReservationNo, PropertyCode, BranchCode);
            var TaxDetailsTask = _reservationRepository.GetTaxDetailsDetails(GuestCode, PropertyCode, BranchCode);

            await Task.WhenAll(GuestDetailsTask, ReservationDetailsTask, SlaveDetailsTask, ForEverDetailsTask, ReceiptDetailsTask,
                ChargeDetailsTask, InstructionDetailsTask, TaxDetailsTask);

            // 2. Return the combined container object
            return new ReservationCompleteDetailsDto
            {
                GuestDetails = GuestDetailsTask.Result,
                ReservationMaster = ReservationDetailsTask.Result,
                ReservationSlaves = SlaveDetailsTask.Result,
                ForEverData = ForEverDetailsTask.Result,
                AdvanceReceipts = ReceiptDetailsTask.Result,
                ReservationCharges = ChargeDetailsTask.Result,
                SpecialInstructions = InstructionDetailsTask.Result,
                TaxDetails = TaxDetailsTask.Result
            };
        }
        #endregion
    }
}
