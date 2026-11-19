using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectEntitiesModel.Reservation;
using FrontOfficeApplication.ProjectEntitiesModel.TaxCalculation;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FrontOfficeApplication.ProjectServiceLayer.TaxCalculation
{
    public class TaxCalculation_Service(CommonMethod commonMethod, IDbConnectionFactory factory)
    {
        private readonly CommonMethod _commonMethod = commonMethod;
        private readonly IDbConnectionFactory _factory = factory;

        public async Task<TaxBreakdownResponseDto1> CalculateAndCategorizeTaxes(TaxCalculationPayloadDto request)
        {
            var response = new TaxBreakdownResponseDto1
            {
                TariffAmount = request.CalculationRequest.TariffAmount
            };

            var allCalculatedTaxes = new List<TaxItemDto>();

            foreach (var taxGroup in request.TaxDetails)
            {
                // 1. Filter valid tax descriptions based on Amount Slabs and Date Range
                var applicableDescriptions = taxGroup.TaxDescriptions.Where(td =>
                {
                    // Assuming 0 or DateTime.MinValue means "no limit / no restriction"
                    bool isValidDate = (td.FromDate != default || request.CalculationRequest.CalculationDate.Date >= td.FromDate.Date) &&
                                       (td.ToDate != default || request.CalculationRequest.CalculationDate.Date <= td.ToDate.Date);

                    bool isValidAmount = (td.FromAmount > 0 || request.CalculationRequest.TariffAmount >= td.FromAmount) &&
                                         (td.ToAmount > 0 || request.CalculationRequest.TariffAmount <= td.ToAmount);

                    return isValidDate && isValidAmount;
                });

                foreach (var desc in applicableDescriptions)
                {
                    bool isCgst = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase);
                    bool isSgst = desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase);
                    bool isIgst = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

                    // Client Type Filtering Rule
                    if (request.CalculationRequest.IsForeignOrInterstateClient && (isCgst || isSgst)) continue;
                    if (!request.CalculationRequest.IsForeignOrInterstateClient && isIgst) continue;

                    decimal taxAmount = Math.Round(request.CalculationRequest.TariffAmount * (desc.TaxPercentage / 100m), 2);

                    var taxItem = new TaxItemDto
                    {
                        TaxCode = taxGroup.TaxCode,
                        TaxName = taxGroup.TaxName,
                        TaxDescriptionCode = desc.TaxDescriptionCode,
                        TaxDescriptionName = desc.TaxDescriptionName,
                        TaxPercentage = desc.TaxPercentage,
                        CalculatedTaxAmount = taxAmount
                    };

                    // 2. Categorize into specific buckets
                    if (isCgst)
                    {
                        response.CGST.Add(taxItem);
                    }
                    else if (isSgst)
                    {
                        response.SGST.Add(taxItem);
                    }
                    else if (isIgst)
                    {
                        response.IGST.Add(taxItem);
                    }
                    else
                    {
                        // Fallback for Cess, VAT, Service Charge, etc.
                        response.OtherTaxes.Add(taxItem);
                    }

                    allCalculatedTaxes.Add(taxItem);
                }
            }

            // 3. Calculate Totals
            response.TotalTaxAmount = allCalculatedTaxes.Sum(t => t.CalculatedTaxAmount);
            response.GrandTotal = response.TariffAmount + response.TotalTaxAmount;

            return response;
        }

        //public async Task<TaxBreakdownResponseDto1> CalculateDynamicTaxes(TaxCalculationPayloadDto request)
        //{
        //    var response = new TaxBreakdownResponseDto1
        //    {
        //        TariffAmount = request.calRequest.TariffAmount
        //    };

        //    var allCalculatedTaxes = new List<TaxItemDto>();

        //    foreach (var taxGroup in request.TaxDetails)
        //    {
        //        // 1. Filter valid tax descriptions based on Slabs and Dates
        //        var applicableDescriptions = taxGroup.TaxDescriptions.Where(td =>
        //        {
        //            bool isValidDate = (td.FromDate == default || request.calRequest.CalculationDate.Date >= td.FromDate.Date) &&
        //                               (td.ToDate == default || request.calRequest.CalculationDate.Date <= td.ToDate.Date);

        //            bool isValidAmount = (td.FromAmount == 0 || request.calRequest.TariffAmount >= td.FromAmount) &&
        //                                 (td.ToAmount == 0 || request.calRequest.TariffAmount <= td.ToAmount);

        //            return isValidDate && isValidAmount;
        //        });

        //        foreach (var desc in applicableDescriptions)
        //        {
        //            // Dynamic check: If client is foreign/interstate, skip local breakdown names if needed, 
        //            // or rely completely on what the tax admin configured.
        //            bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
        //                              desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase);
        //            bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

        //            if (request.calRequest.IsForeignOrInterstateClient && isLocalTax) continue;
        //            if (!request.calRequest.IsForeignOrInterstateClient && isInterstateTax) continue;

        //            decimal taxAmount = Math.Round(request.calRequest.TariffAmount * (desc.TaxPercentage / 100m), 2);

        //            var taxItem = new TaxItemDto
        //            {
        //                TaxCode = taxGroup.TaxCode,
        //                TaxName = taxGroup.TaxName,
        //                TaxDescriptionCode = desc.TaxDescriptionCode,
        //                TaxDescriptionName = desc.TaxDescriptionName,
        //                TaxPercentage = desc.TaxPercentage,
        //                CalculatedTaxAmount = taxAmount
        //            };

        //            // 2. Dynamically group by the TaxName (e.g., "GST5%", "VAT", etc.)
        //            // If the key doesn't exist in the dictionary yet, create it automatically.
        //            string groupKey = string.IsNullOrWhiteSpace(taxGroup.TaxName) ? "General Tax" : taxGroup.TaxName;

        //            if (!response.CategorizedTaxes.ContainsKey(groupKey))
        //            {
        //                response.CategorizedTaxes[groupKey] = [];
        //            }

        //            response.CategorizedTaxes[groupKey].Add(taxItem);
        //            allCalculatedTaxes.Add(taxItem);
        //        }
        //    }

        //    // 3. Compute Totals
        //    response.TotalTaxAmount = allCalculatedTaxes.Sum(t => t.CalculatedTaxAmount);
        //    response.GrandTotal = response.TariffAmount + response.TotalTaxAmount;

        //    return response;
        //}

        //public async Task<TaxBreakdownResponseDto1> CalculateDynamicTaxes(TaxCalculationRequestDto request)
        //{
        //    var response = new TaxBreakdownResponseDto1
        //    {
        //        TariffAmount = request.TariffAmount
        //    };

        //    // 1. Fetch Tax Details and Tax Rules Concurrently
        //    var taxDetails = await GetTaxDetails(request.PropertyCode, request.BranchCode);
        //    var taxRules = await GetTaxRulesDetails(request.BranchCode, request.PropertyCode);


        //    // 2. Dynamically calculate the Taxable Base Amount using TaxRulesDetails
        //    decimal taxableBaseAmount = request.TariffAmount;

        //    if (taxRules != null && taxRules.IsTaxRuleActive)
        //    {
        //        // Apply Type1 (e.g., Extra Bed) based on operation (+ or -)
        //        if (!string.IsNullOrWhiteSpace(taxRules.Type1) && taxRules.Type1.Equals("EXtraBed", StringComparison.OrdinalIgnoreCase))
        //        {
        //            decimal extraBedAmount = request.ExtraAdult + request.ExtraChild;

        //            if (taxRules.Operation1 == "+")
        //                taxableBaseAmount += extraBedAmount;
        //            else if (taxRules.Operation1 == "-")
        //                taxableBaseAmount -= extraBedAmount;
        //        }
        //    }

        //    var allCalculatedTaxes = new List<TaxItemDto>();

        //    // 3. Group the flat SQL rows by TaxCode and TaxName so we can iterate through them hierarchically
        //    var groupedTaxDetails = taxDetails.GroupBy(t => new { t.TaxCode, t.TaxName });

        //    foreach (var taxGroup in groupedTaxDetails)
        //    {
        //        // 4. Filter valid tax descriptions based on Slabs (FromAmount/ToAmount) and Dates
        //        var applicableDescriptions = taxGroup.Where(td =>
        //        {
        //            bool isValidDate = (td.FromDate == default || request.CalculationDate.Date >= td.FromDate.Date) &&
        //                                (td.ToDate == default || request.CalculationDate.Date <= td.ToDate.Date);

        //            // Slabs usually check against the base TariffAmount (or taxableBaseAmount depending on business rule)
        //            bool isValidAmount = (td.FromAmount == 0 || request.TariffAmount >= td.FromAmount) &&
        //                                  (td.ToAmount == 0 || request.TariffAmount <= td.ToAmount);

        //            return isValidDate && isValidAmount;
        //        });

        //        foreach (var desc in applicableDescriptions)
        //        {
        //            // Client type validation (CGST/SGST vs IGST)
        //            bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
        //                              desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase);
        //            bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

        //            if (request.IsForeignOrInterstateClient && isLocalTax) continue;
        //            if (!request.IsForeignOrInterstateClient && isInterstateTax) continue;

        //            // 5. Calculate tax amount using the derived taxableBaseAmount
        //            decimal taxAmount = Math.Round(taxableBaseAmount * (desc.TaxPercentage / 100m), 2);

        //            var taxItem = new TaxItemDto
        //            {
        //                TaxCode = taxGroup.Key.TaxCode,
        //                TaxName = taxGroup.Key.TaxName,
        //                TaxDescriptionCode = desc.TaxDescriptionCode,
        //                TaxDescriptionName = desc.TaxDescriptionName,
        //                TaxPercentage = desc.TaxPercentage,
        //                CalculatedTaxAmount = taxAmount
        //            };

        //            // 6. Group by TaxName dynamically for the final response dictionary
        //            string groupKey = string.IsNullOrWhiteSpace(taxGroup.Key.TaxName) ? "General Tax" : taxGroup.Key.TaxName;

        //            if (!response.CategorizedTaxes.ContainsKey(groupKey))
        //            {
        //                response.CategorizedTaxes[groupKey] = [];
        //            }

        //            response.CategorizedTaxes[groupKey].Add(taxItem);
        //            allCalculatedTaxes.Add(taxItem);
        //        }
        //    }

        //    // 7. Compute Final Totals
        //    response.TotalTaxAmount = allCalculatedTaxes.Sum(t => t.CalculatedTaxAmount);
        //    response.GrandTotal = taxableBaseAmount + response.TotalTaxAmount;

        //    return response;
        //}

        public async Task<TaxBreakdownResponseDto> CalculateDynamicTaxes1(TaxCalculationRequestDto request)
        {
            var response = new TaxBreakdownResponseDto
            {
                TariffAmount = request.TariffAmount
            };

            // 1. Fetch Tax Details and Tax Rules Concurrently
            var taxDetailsTask = GetTaxDetails(request.PropertyCode, request.BranchCode);
            var taxRulesTask = GetTaxRulesDetails(request.BranchCode, request.PropertyCode);

            await Task.WhenAll(taxDetailsTask, taxRulesTask);

            var rawTaxDetails = await taxDetailsTask;
            var taxRules = await taxRulesTask;

            // 2. Initially evaluate Tax Rules before calculating tax
            decimal tariffTaxableBase = request.TariffAmount;
            decimal extraBedAmount = request.ExtraAdult + request.ExtraChild;

            if (taxRules != null && taxRules.IsTaxRuleActive && taxRules.IsSeprateExtraBedTax && extraBedAmount > 0)
            {
                // Apply Type1 (e.g., ExtraBed) based on operation (+ or -)
                if (!string.IsNullOrWhiteSpace(taxRules.Type1) && taxRules.Type1.Equals("EXtraBed", StringComparison.OrdinalIgnoreCase))
                {
                    if (taxRules.Operation1 == "+")
                        tariffTaxableBase += extraBedAmount;
                    else if (taxRules.Operation1 == "-")
                        tariffTaxableBase -= extraBedAmount;
                }
            }

            var allCalculatedTaxes = new List<TaxItemDto>();
            var groupedTaxDetails = rawTaxDetails.GroupBy(t => new { t.TaxCode, t.TaxName });

            foreach (var taxGroup in groupedTaxDetails)
            {
                // 3. Filter valid tax descriptions for Main Tariff (Checking Slabs and Dates)
                var tariffDescriptions = taxGroup.Where(td =>
                {
                    bool isValidDate = (td.FromDate == default || request.CalculationDate.Date >= td.FromDate.Date) &&
                                        (td.ToDate == default || request.CalculationDate.Date <= td.ToDate.Date);

                    bool isValidAmount = (td.FromAmount == 0 || request.TariffAmount >= td.FromAmount) &&
                                          (td.ToAmount == 0 || request.TariffAmount <= td.ToAmount);

                    return isValidDate && isValidAmount;
                });


                foreach (var desc in tariffDescriptions)
                {
                    // Client type validation (CGST/SGST vs IGST)
                    bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
                                      desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase);
                    bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

                    if (request.IsForeignOrInterstateClient && isLocalTax) continue;
                    if (!request.IsForeignOrInterstateClient && isInterstateTax) continue;

                    decimal taxAmount = Math.Round(tariffTaxableBase * (desc.TaxPercentage / 100m), 2);

                    var taxItem = new TaxItemDto
                    {
                        TaxCode = taxGroup.Key.TaxCode,
                        TaxName = taxGroup.Key.TaxName,
                        TaxDescriptionCode = desc.TaxDescriptionCode,
                        TaxDescriptionName = desc.TaxDescriptionName,
                        TaxPercentage = desc.TaxPercentage,
                        CalculatedTaxAmount = taxAmount
                    };

                    string groupKey = string.IsNullOrWhiteSpace(taxGroup.Key.TaxName) ? "General Tax" : taxGroup.Key.TaxName;

                    //if (!response.CategorizedTaxes.ContainsKey(groupKey))
                    //{
                    //    response.CategorizedTaxes[groupKey] = [];
                    //}

                    if (!response.CategorizedTaxes.TryGetValue(groupKey, out var taxList))
                    {
                        taxList = [];
                        response.CategorizedTaxes[groupKey] = taxList;
                    }

                    response.CategorizedTaxes[groupKey].Add(taxItem);
                    allCalculatedTaxes.Add(taxItem);
                }

                // 4. If IsSeprateExtraBedTax is true, calculate separate tax for extra bed WITHOUT considering From and To amount range slabs
                if (taxRules != null && taxRules.IsTaxRuleActive && taxRules.IsSeprateExtraBedTax && extraBedAmount > 0)
                {
                    var extraBedDescriptions = taxGroup.Where(td =>
                    {
                        // Only check date validity, ignore FromAmount / ToAmount slabs
                        bool isValidDate = (td.FromDate == default || request.CalculationDate.Date >= td.FromDate.Date) &&
                                            (td.ToDate == default || request.CalculationDate.Date <= td.ToDate.Date);
                        return isValidDate;
                    });

                    foreach (var desc in extraBedDescriptions)
                    {
                        bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
                                          desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase);
                        bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

                        if (request.IsForeignOrInterstateClient && isLocalTax) continue;
                        if (!request.IsForeignOrInterstateClient && isInterstateTax) continue;

                        decimal extraBedTaxAmount = Math.Round(extraBedAmount * (desc.TaxPercentage / 100m), 2);

                        var extraBedTaxItem = new TaxItemDto
                        {
                            TaxCode = taxGroup.Key.TaxCode,
                            TaxName = $"{taxGroup.Key.TaxName} (Extra Bed)",
                            TaxDescriptionCode = desc.TaxDescriptionCode,
                            TaxDescriptionName = $"{desc.TaxDescriptionName} [Extra Bed]",
                            TaxPercentage = desc.TaxPercentage,
                            CalculatedTaxAmount = extraBedTaxAmount
                        };

                        string groupKey = $"{taxGroup.Key.TaxName} (Extra Bed)";

                        //if (!response.CategorizedTaxes.ContainsKey(groupKey))
                        //{
                        //    response.CategorizedTaxes[groupKey] = [];
                        //}

                        if (!response.CategorizedTaxes.TryGetValue(groupKey, out var taxList))
                        {
                            taxList = [];
                            response.CategorizedTaxes[groupKey] = taxList;
                        }

                        response.CategorizedTaxes[groupKey].Add(extraBedTaxItem);
                        allCalculatedTaxes.Add(extraBedTaxItem);
                    }
                }
            }

            // 5. Compute Final Totals
            response.TotalTaxAmount = allCalculatedTaxes.Sum(t => t.CalculatedTaxAmount);
            response.GrandTotal = tariffTaxableBase + extraBedAmount + response.TotalTaxAmount;

            return response;
        }

        public async Task<TaxBreakdownResponseDto> CalculateDynamicTaxes2(TaxCalculationRequestDto request)
        {
            var response = new TaxBreakdownResponseDto
            {
                TariffAmount = request.TariffAmount
            };

            // 1. Fetch Tax Details and Tax Rules Concurrently
            var taxDetailsTask = GetTaxDetails(request.PropertyCode, request.BranchCode);
            var taxRulesTask = GetTaxRulesDetails(request.BranchCode, request.PropertyCode);

            await Task.WhenAll(taxDetailsTask, taxRulesTask);

            var rawTaxDetails = await taxDetailsTask;
            var taxRules = await taxRulesTask;

            // 2. Initially evaluate Tax Rules before calculating tax
            decimal tariffTaxableBase = request.TariffAmount;
            decimal extraBedAmount = request.ExtraAdult + request.ExtraChild;

            if (taxRules != null && taxRules.IsTaxRuleActive && !taxRules.IsSeprateExtraBedTax && extraBedAmount > 0)
            {
                if (!string.IsNullOrWhiteSpace(taxRules.Type1) && taxRules.Type1.Equals("EXtraBed", StringComparison.OrdinalIgnoreCase))
                {
                    if (taxRules.Operation1 == "+")
                        tariffTaxableBase += extraBedAmount;
                    else if (taxRules.Operation1 == "-")
                        tariffTaxableBase -= extraBedAmount;
                }
            }

            var allCalculatedTaxes = new List<TaxItemDto>();
            var groupedTaxDetails = rawTaxDetails.GroupBy(t => new { t.TaxCode, t.TaxName });

            foreach (var taxGroup in groupedTaxDetails)
            {
                // 3. Find valid tax descriptions for the Main Tariff based on Slabs and Dates
                var tariffDescriptions = taxGroup.Where(td =>
                {
                    bool isValidDate = (td.FromDate == default || request.CalculationDate.Date >= td.FromDate.Date) &&
                                        (td.ToDate == default || request.CalculationDate.Date <= td.ToDate.Date);

                    bool isValidAmount = (td.FromAmount == 0 || request.TariffAmount >= td.FromAmount) &&
                                          (td.ToAmount == 0 || request.TariffAmount <= td.ToAmount);

                    return isValidDate && isValidAmount;
                }).ToList();

                // 4. Calculate Main Tariff Taxes
                foreach (var desc in tariffDescriptions)
                {
                    bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
                                      desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase);
                    bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

                    if (request.IsForeignOrInterstateClient && isLocalTax) continue;
                    if (!request.IsForeignOrInterstateClient && isInterstateTax) continue;

                    decimal taxAmount = Math.Round(tariffTaxableBase * (desc.TaxPercentage / 100m), 2);

                    var taxItem = new TaxItemDto
                    {
                        TaxCode = taxGroup.Key.TaxCode,
                        TaxName = taxGroup.Key.TaxName,
                        TaxDescriptionCode = desc.TaxDescriptionCode,
                        TaxDescriptionName = desc.TaxDescriptionName,
                        TaxPercentage = desc.TaxPercentage,
                        CalculatedTaxAmount = taxAmount
                    };

                    string groupKey = string.IsNullOrWhiteSpace(taxGroup.Key.TaxName) ? "General Tax" : taxGroup.Key.TaxName;

                    //if (!response.CategorizedTaxes.ContainsKey(groupKey))
                    //{
                    //    response.CategorizedTaxes[groupKey] = [];
                    //}

                    if (!response.CategorizedTaxes.TryGetValue(groupKey, out var taxList))
                    {
                        taxList = [];
                        response.CategorizedTaxes[groupKey] = taxList;
                    }

                    response.CategorizedTaxes[groupKey].Add(taxItem);
                    allCalculatedTaxes.Add(taxItem);
                }

                // 5. Calculate Extra Bed Tax by directly reusing tariffDescriptions
                if (tariffDescriptions != null && taxRules != null && taxRules.IsTaxRuleActive && taxRules.IsSeprateExtraBedTax && extraBedAmount > 0)
                {
                    foreach (var desc in tariffDescriptions)
                    {
                        bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
                                          desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase);
                        bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

                        if (request.IsForeignOrInterstateClient && isLocalTax) continue;
                        if (!request.IsForeignOrInterstateClient && isInterstateTax) continue;

                        decimal extraBedTaxAmount = Math.Round(extraBedAmount * (desc.TaxPercentage / 100m), 2);

                        var extraBedTaxItem = new TaxItemDto
                        {
                            TaxCode = taxGroup.Key.TaxCode,
                            TaxName = $"{taxGroup.Key.TaxName} (Extra Bed)",
                            TaxDescriptionCode = desc.TaxDescriptionCode,
                            TaxDescriptionName = $"{desc.TaxDescriptionName} [Extra Bed]",
                            TaxPercentage = desc.TaxPercentage,
                            CalculatedTaxAmount = extraBedTaxAmount
                        };

                        string groupKey = $"{taxGroup.Key.TaxName} (Extra Bed)";

                        //if (!response.CategorizedTaxes.ContainsKey(groupKey))
                        //{
                        //    response.CategorizedTaxes[groupKey] = [];
                        //}

                        if (!response.CategorizedTaxes.TryGetValue(groupKey, out var taxList))
                        {
                            taxList = [];
                            response.CategorizedTaxes[groupKey] = taxList;
                        }

                        response.CategorizedTaxes[groupKey].Add(extraBedTaxItem);
                        allCalculatedTaxes.Add(extraBedTaxItem);
                    }
                }
            }

            // 6. Compute Final Totals
            response.TotalTaxAmount = allCalculatedTaxes.Sum(t => t.CalculatedTaxAmount);
            response.GrandTotal = tariffTaxableBase + extraBedAmount + response.TotalTaxAmount;

            return response;
        }

        public async Task<TaxBreakdownResponseDto> CalculateDynamicTaxes(TaxCalculationRequestDto request)
        {
            var response = new TaxBreakdownResponseDto
            {
                TariffAmount = request.TariffAmount
            };

            // 1. Fetch Tax Details and Tax Rules Concurrently
            var taxDetailsTask = GetTaxDetails(request.PropertyCode, request.BranchCode);
            var taxRulesTask = GetTaxRulesDetails(request.BranchCode, request.PropertyCode);

            await Task.WhenAll(taxDetailsTask, taxRulesTask);

            var rawTaxDetails = await taxDetailsTask;
            var taxRules = await taxRulesTask;

            // 2. Dynamic Rule Engine
            decimal tariffTaxableBase = request.TariffAmount;
            decimal extraBedAmount = request.ExtraAdult + request.ExtraChild;
            decimal discountAmount = 0;

            // Calculate discount amount first
            //if (request.DiscountValue > 0)
            //{
            //    discountAmount = request.IsDiscountPercentage
            //        ? Math.Round(request.TariffAmount * (request.DiscountValue / 100m), 2)
            //        : request.DiscountValue;
            //}

            if (taxRules != null && taxRules.IsTaxRuleActive)
            {
                //if (!taxRules.IsSeprateExtraBedTax)
                //{
                //    // Case A: Extra bed is bundled into the taxable base
                //    // Formula: Tariff + ExtraBed - Discount
                //    tariffTaxableBase = request.TariffAmount + extraBedAmount - discountAmount;
                //}
                //else
                //{
                //    // Case B: Extra bed is separated (Taxed independently)
                //    // Formula: Tariff - Discount (Extra bed stays separate for its own tax calculation loop)
                //    tariffTaxableBase = request.TariffAmount - discountAmount;
                //}

                // --- Process Step 1 (Type1 & Operation1) ---
                decimal tType1 = 0;
                if (!string.IsNullOrWhiteSpace(taxRules.Type1))
                {
                    if (taxRules.Type1.Equals("ExtraBed", StringComparison.OrdinalIgnoreCase))
                    {
                        //tType1 = extraBedAmount;
                        tType1 = taxRules.IsSeprateExtraBedTax ? 0 : extraBedAmount;
                    }
                    else if (taxRules.Type1.Equals("Discount", StringComparison.OrdinalIgnoreCase))
                    {
                        tType1 = request.IsDiscountPercentage ? Math.Round(tariffTaxableBase * (request.DiscountValue / 100m), 2) : request.DiscountValue;
                        discountAmount = tType1;
                        //tType1 = discountAmount;
                    }
                }

                tariffTaxableBase = taxRules.TraiffOperation switch
                {
                    "+" => tariffTaxableBase + tType1,
                    "-" => tariffTaxableBase - tType1,
                    "*" => tariffTaxableBase * tType1,
                    "/" => tType1 != 0 ? tariffTaxableBase / tType1 : tariffTaxableBase,
                    _ => tariffTaxableBase
                };

                // --- Process Step 2 (Type2 & Operation2 - if your model has Type2/Operation2) ---
                // If your model includes Type2 and Operation2 properties, uncomment below:

                decimal tType2 = 0;
                if (!string.IsNullOrWhiteSpace(taxRules.Type2))
                {
                    if (taxRules.Type2.Equals("ExtraBed", StringComparison.OrdinalIgnoreCase))
                    {
                        //tType2 = extraBedAmount;
                        tType2 = taxRules.IsSeprateExtraBedTax ? 0 : extraBedAmount;
                    }
                    else if (taxRules.Type2.Equals("Discount", StringComparison.OrdinalIgnoreCase))
                    {
                        if (taxRules.IsSeprateExtraBedTax)
                        {
                            decimal baseForDiscount = request.TariffAmount;

                            tType2 = request.IsDiscountPercentage ? Math.Round(baseForDiscount * (request.DiscountValue / 100m), 2) : request.DiscountValue;
                            discountAmount = tType2;
                        }
                        else
                        {
                            decimal baseForDiscount = request.TariffAmount;

                            tType2 = request.IsDiscountPercentage ? Math.Round(tariffTaxableBase * (request.DiscountValue / 100m), 2) : request.DiscountValue;
                            discountAmount = tType2;
                        }
                        
                        //tType2 = discountAmount;
                    }
                }

                tariffTaxableBase = taxRules.Operation1 switch
                {
                    "+" => tariffTaxableBase + tType2,
                    "-" => tariffTaxableBase - tType2,
                    "*" => tariffTaxableBase * tType2,
                    "/" => tType2 != 0 ? tariffTaxableBase / tType2 : tariffTaxableBase,
                    _ => tariffTaxableBase
                };


                if (tariffTaxableBase <= 0)
                {
                    tariffTaxableBase = request.TariffAmount; // Fallback safeguard
                }
            }
            else
            {
                // Fallback if no active tax rules: Standard base is just Tariff - Discount
                discountAmount = request.IsDiscountPercentage ? Math.Round(request.TariffAmount * (request.DiscountValue / 100m), 2) : request.DiscountValue;

                tariffTaxableBase = request.TariffAmount - discountAmount;
            }

            var allCalculatedTaxes = new List<TaxItemDto>();
            var groupedTaxDetails = rawTaxDetails.GroupBy(t => new { t.TaxCode, t.TaxName });

            foreach (var taxGroup in groupedTaxDetails)
            {
                // 3. Find valid tax descriptions for the Main Tariff based on Slabs and Dates
                var tariffDescriptions = taxGroup.Where(td =>
                {
                    bool isValidDate = (td.FromDate == default || request.CalculationDate.Date >= td.FromDate.Date) &&
                                        (td.ToDate == default || request.CalculationDate.Date <= td.ToDate.Date);

                    bool isValidAmount = (td.FromAmount == 0 || tariffTaxableBase >= td.FromAmount) &&
                                          (td.ToAmount == 0 || tariffTaxableBase <= td.ToAmount);

                    return isValidDate && isValidAmount;
                }).ToList();

                // 4. Calculate Main Tariff Taxes
                foreach (var desc in tariffDescriptions)
                {
                    // Dynamic check for local vs interstate tax types
                    bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
                                      desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase) ||
                                      desc.TaxDescriptionName.Contains("VAT", StringComparison.OrdinalIgnoreCase);

                    bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

                    if (request.IsForeignOrInterstateClient && isLocalTax) continue;
                    if (!request.IsForeignOrInterstateClient && isInterstateTax) continue;

                    decimal taxAmount = Math.Round(tariffTaxableBase * (desc.TaxPercentage / 100m), 2);

                    var taxItem = new TaxItemDto
                    {
                        TaxCode = taxGroup.Key.TaxCode,
                        TaxName = taxGroup.Key.TaxName,
                        TaxDescriptionCode = desc.TaxDescriptionCode,
                        TaxDescriptionName = desc.TaxDescriptionName,
                        TaxPercentage = desc.TaxPercentage,
                        CalculatedTaxAmount = taxAmount
                    };

                    string groupKey = string.IsNullOrWhiteSpace(taxGroup.Key.TaxName) ? "General Tax" : taxGroup.Key.TaxName;

                    //if (!response.CategorizedTaxes.ContainsKey(groupKey))
                    //{
                    //    response.CategorizedTaxes[groupKey] = [];
                    //}

                    if (!response.CategorizedTaxes.TryGetValue(groupKey, out var taxList))
                    {
                        taxList = [];
                        response.CategorizedTaxes[groupKey] = taxList;
                    }

                    response.CategorizedTaxes[groupKey].Add(taxItem);
                    allCalculatedTaxes.Add(taxItem);
                }

                // 5. Calculate Separate Extra Bed Tax if enabled in rule configuration
                bool isSeparateExtraBedActive = taxRules != null && taxRules.IsTaxRuleActive && taxRules.IsSeprateExtraBedTax;

                if (tariffDescriptions != null && isSeparateExtraBedActive && extraBedAmount > 0)
                {
                    foreach (var desc in tariffDescriptions)
                    {
                        bool isLocalTax = desc.TaxDescriptionName.Contains("CGST", StringComparison.OrdinalIgnoreCase) ||
                                          desc.TaxDescriptionName.Contains("SGST", StringComparison.OrdinalIgnoreCase) ||
                                      desc.TaxDescriptionName.Contains("VAT", StringComparison.OrdinalIgnoreCase); 

                        bool isInterstateTax = desc.TaxDescriptionName.Contains("IGST", StringComparison.OrdinalIgnoreCase);

                        if (request.IsForeignOrInterstateClient && isLocalTax) continue;
                        if (!request.IsForeignOrInterstateClient && isInterstateTax) continue;

                        decimal extraBedTaxAmount = Math.Round(extraBedAmount * (desc.TaxPercentage / 100m), 2);

                        var extraBedTaxItem = new TaxItemDto
                        {
                            TaxCode = taxGroup.Key.TaxCode,
                            TaxName = $"{taxGroup.Key.TaxName} (Extra Bed)",
                            TaxDescriptionCode = desc.TaxDescriptionCode,
                            TaxDescriptionName = $"{desc.TaxDescriptionName} [Extra Bed]",
                            TaxPercentage = desc.TaxPercentage,
                            CalculatedTaxAmount = extraBedTaxAmount
                        };

                        string groupKey = $"{taxGroup.Key.TaxName} (Extra Bed)";

                        //if (!response.CategorizedTaxes.ContainsKey(groupKey))
                        //{
                        //    response.CategorizedTaxes[groupKey] = [];
                        //}
                        if (!response.CategorizedTaxes.TryGetValue(groupKey, out var taxList))
                        {
                            taxList = [];
                            response.CategorizedTaxes[groupKey] = taxList;
                        }
                        response.CategorizedTaxes[groupKey].Add(extraBedTaxItem);
                        allCalculatedTaxes.Add(extraBedTaxItem);
                    }
                }
            }

            // 6. Compute Final Totals
            response.DiscountAmount = discountAmount;
            response.TotalTaxAmount = allCalculatedTaxes.Sum(t => t.CalculatedTaxAmount);
            decimal baseForGrandTotal = tariffTaxableBase;

            if (taxRules != null && taxRules.IsSeprateExtraBedTax && extraBedAmount > 0)
            {
                baseForGrandTotal += extraBedAmount;
            }

            response.GrandTotal = baseForGrandTotal + response.TotalTaxAmount;
            //response.GrandTotal = tariffTaxableBase + (taxRules?.IsSeprateExtraBedTax == true ? extraBedAmount : 0) + response.TotalTaxAmount;

            return response;
        }

        public async Task<IEnumerable<TaxDetailsModel>> GetTaxDetails(string PropertyCode, string BranchCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            const string selectquery = @" Select TM.TaxCode, TM.TaxName, TDM.TaxDescriptionCode, TDM.TaxDescriptionName, TDM.TaxPercentage, 
                TDM.FromAmount, TDM.ToAmount, TDM.FromDate, TDM.ToDate, TDM.PropertyCode, TDM.BranchCode
                From Tbl_Tax_Master TM
                Inner Join Tbl_TaxDescription_Master TDM on TDM.TaxCode = TM.TaxCode And TM.PropertyCode = TDM.PropertyCode And TM.BranchCode = TDM.BranchCode
                Where TM.IsTaxActive = 1 And TDM.IsTaxDescriptionActive = 1 And TDM.PropertyCode = @propertycode And TDM.BranchCode = @branchcode 
                ORDER BY TM.TaxCode; ";

            var result = await connection.QueryAsync<TaxDetailsModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });

            return result;
        }

        public async Task<TaxRulesDetailsModel?> GetTaxRulesDetails(string BranchCode, string PropertyCode)
        {
            using var connection = await _factory.CreateConnection(DbNames.HMS);

            var selectquery = @"Select TaxRuleId, TaxRuleCode, Traiff, TraiffOperation, Type1, Operation1, Type2, 
                IsTaxRuleActive, IsSeprateExtraBedTax, PropertyCode, BranchCode
                From Tbl_TaxRule_Master
                Where IsTaxRuleActive = 1 And PropertyCode = @propertyCode And BranchCode = @branchCode Order By TaxRuleId; ";

            var TaxRulelist = await connection.QueryFirstOrDefaultAsync<TaxRulesDetailsModel>(selectquery, new { propertycode = PropertyCode, branchcode = BranchCode });
            return TaxRulelist;
        }
    }
}
