using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.Master;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FrontOfficeApplication.ProjectServiceLayer
{
    public class CommonMethod(IDbConnectionFactory factory)
    {
        private readonly IDbConnectionFactory _factory = factory;

        public DateTime ConvertUtcToIst()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Ensure input is treated as UTC
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            }

            TimeZoneInfo istZone;

            // Handle Windows & Linux
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }

        public async Task<FinancialMasterModel?> GetFinancialMaster(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select FinId, FinFromDate, FinToDate, FinCurrentYear, FinNewYear, CurrentStatus, BackUpTook, FinalClose, 
                FinCode, UserCode, IPAddress, PropertyCode, BranchCode
                From Tbl_Financial_Master
                Where CurrentStatus = 1 And PropertyCode = @propertyCode And BranchCode = @branchCode; ";

            var FinMaster = await connection.QueryFirstOrDefaultAsync<FinancialMasterModel>(selectquery, new { PropertyCode = PropertyCode, BranchCode = BranchCode });
            return FinMaster;
        }

        public async Task<string> GetNextMasterCode(string tableName, string codeColumn, string prefix, string propertyCode, string branchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var sql = $""" SELECT MAX( TRY_CONVERT( INT, SUBSTRING( [{codeColumn}], @PrefixLength + 1, LEN([{codeColumn}]) ) ) ) FROM [{tableName}] WHERE PropertyCode = @PropertyCode AND BranchCode = @BranchCode AND [{codeColumn}] LIKE @Prefix + '%' """;

            var result = await connection.ExecuteScalarAsync<int?>(
                sql,
                new
                {
                    PrefixLength = prefix.Length,
                    Prefix = prefix,
                    PropertyCode = propertyCode,
                    BranchCode = branchCode
                });

            var nextNumber = (result ?? 0) + 1;

            return $"{prefix}{nextNumber:D3}";
        }

        public async Task<string> GetNextTransactionCode(string tableName, string codeColumn, string prefix, string propertyCode, string branchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var sql = $"""
                 SELECT MAX( TRY_CONVERT( INT, SUBSTRING( [{codeColumn}], @PrefixLength + 1, LEN([{codeColumn}]) ) ) ) 
                 FROM [{tableName}] WITH (UPDLOCK, HOLDLOCK) 
                WHERE PropertyCode = @PropertyCode AND BranchCode = @BranchCode AND [{codeColumn}] LIKE @Prefix + '%' 
                """;

            var result = await connection.ExecuteScalarAsync<int?>(
                sql,
                new
                {
                    PrefixLength = prefix.Length,
                    Prefix = prefix,
                    PropertyCode = propertyCode,
                    BranchCode = branchCode
                });

            var nextNumber = (result ?? 0) + 1;

            return $"{prefix}{nextNumber:D6}";
        }

        public class FinancialConditionsResponse
        {
            public string FinWhereCondition { get; set; } = string.Empty;
            public string FinAndCondition { get; set; } = string.Empty;
        }

        public async Task<string?> GetNextTransactionFinCode(string tableName, string codeColumn, string prefix, string propertyCode, string branchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var FinDetails = await GetFinancialMaster(branchCode, propertyCode);
            if (FinDetails != null)
            {
                FinancialConditionsResponse FinCondition = await GetActiveFinancialPeriod(FinDetails.FinFromDate, FinDetails.FinToDate);

                var sql = $"""
                 SELECT MAX( TRY_CONVERT( INT, SUBSTRING( [{codeColumn}], @PrefixLength + 1, LEN([{codeColumn}]) ) ) ) 
                 FROM [{tableName}] WITH (UPDLOCK, HOLDLOCK) 
                WHERE PropertyCode = @PropertyCode AND BranchCode = @BranchCode AND [{codeColumn}] LIKE @Prefix + '%' 
                [{FinCondition.FinAndCondition}]
                """;

                var result = await connection.ExecuteScalarAsync<int?>(
                    sql,
                    new
                    {
                        PrefixLength = prefix.Length,
                        Prefix = prefix,
                        PropertyCode = propertyCode,
                        BranchCode = branchCode
                    });

                var nextNumber = (result ?? 0) + 1;

                return $"{FinDetails.FinCode}{prefix}{nextNumber:D6}";
            }
            else
            {
                return null;
            }
        }

        public async Task<FinancialConditionsResponse> GetActiveFinancialPeriod(DateTime FinFromDate, DateTime FinToDate)
        {
            try
            {
                // Format dates safely to prevent SQL injection and culture issues
                string fromStr = FinFromDate.ToString("yyyy-MM-dd");
                string toStr = FinToDate.ToString("yyyy-MM-dd");
                //var response = new
                //{
                //    FinWhereCondition = $" Where CAST(CreatedDate AS DATE) >= CAST('{FinFromDate}' AS DATE) AND CAST(CreatedDate AS DATE) <= CAST('{FinToDate}' AS DATE) ",
                //    FinAndCondition = $" And CAST(CreatedDate AS DATE) >= CAST('{FinFromDate}' AS DATE) AND CAST(CreatedDate AS DATE) <= CAST('{FinToDate}' AS DATE)"
                //};

                var response = new FinancialConditionsResponse
                {
                    FinWhereCondition = $" Where CAST(CreatedDate AS DATE) >= '{fromStr}' AND CAST(CreatedDate AS DATE) <= '{toStr}' ",
                    FinAndCondition = $" And CAST(CreatedDate AS DATE) >= '{fromStr}' AND CAST(CreatedDate AS DATE) <= '{toStr}'"
                };

                return (response);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
