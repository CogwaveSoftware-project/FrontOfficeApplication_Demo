using CountryData.Standard;
using CountryStateCityLibrary.Services;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
using System.Text.Json;

namespace FrontOfficeApplication.ProjectAPI.Controllers.Location
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        //    private readonly CountryService _countryService;
        //    private readonly StateService _stateService;
        //    private readonly CityService _cityService;

        //    public LocationsController()
        //    {
        //        _countryService = new CountryService();
        //        _stateService = new StateService();
        //        _cityService = new CityService();
        //    }

        //    // 1. Get all countries
        //    [HttpGet("countries")]
        //    public async Task<IActionResult> GetCountries()
        //    {
        //        var countries = await _countryService.GetCountries();
        //        return Ok(countries);
        //    }

        //    // 2. Get states by Country ID
        //    [HttpGet("states/{countryId}")]
        //    public async Task<IActionResult> GetStates(int countryId)
        //    {
        //        var states = await _stateService.GetStatesByCountryId(countryId);
        //        return Ok(states);
        //    }

        //    // 3. Get cities by State ID
        //    [HttpGet("cities/{stateId}")]
        //    public async Task<IActionResult> GetCities(int stateId)
        //    {
        //        var cities = await _cityService.GetCitiesByStateId(stateId);
        //        return Ok(cities);
        //    }
        //}

        private readonly CountryHelper _countryHelper;
        private readonly IWebHostEnvironment _env;

        // Model for City JSON
        public class CityModel
        {
            public string CountryCode { get; set; } = string.Empty;
            public string StateName { get; set; } = string.Empty;
            public string CityName { get; set; } = string.Empty;
        }

        public LocationsController(IWebHostEnvironment env)
        {
            // Initializes the country, state, and region data in-memory
            _countryHelper = new CountryHelper();
            _env = env;
        }

        [HttpGet("countries")]
        public IActionResult GetCountries()
        {
            var countries = _countryHelper.GetCountryData()
                .Select(c => new { c.CountryName, c.CountryShortCode })
                .ToList();

            return Ok(countries);
        }

        // 2. Get states by country short code (e.g., "US", "IN")
        [HttpGet("states/{countryCode}")]
        public IActionResult GetStates(string countryCode)
        {
            var country = _countryHelper.GetCountryData()
                .FirstOrDefault(c => c.CountryShortCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

            if (country == null) return NotFound("Country not found");

            var states = country.Regions.Select(r => new { r.Name, r.ShortCode }).ToList();
            return Ok(states);
        }

        //[HttpGet("{countryCode}/{stateName}")]
        //public IActionResult GetCities(string countryCode, string stateName)
        //{
        //    // Maps to D:\Projects\FrontOfficeApplication\wwwroot\LocationCity\cities.json
        //    var filePath = Path.Combine(_env.WebRootPath, "LocationCity", "cities.json");

        //    if (!System.IO.File.Exists(filePath))
        //    {
        //        return NotFound("The cities.json file could not be found on the server.");
        //    }

        //    var jsonData = System.IO.File.ReadAllText(filePath);

        //    // Deserialize JSON into the model list
        //    var allCities = JsonSerializer.Deserialize<List<CityModel>>(jsonData, new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    });

        //    // Filter cities by Country and State
        //    var filteredCities = allCities?
        //        .Where(c => c.CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase) &&
        //                    c.StateName.Equals(stateName, StringComparison.OrdinalIgnoreCase))
        //        .Select(c => c.CityName)
        //        .Distinct()
        //        .OrderBy(c => c)
        //        .ToList();

        //    return Ok(filteredCities);
        //}
    }
}


