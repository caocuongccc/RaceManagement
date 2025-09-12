using Microsoft.AspNetCore.Mvc;
using RaceManagement.Application.Services;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnhancedTestController : ControllerBase
    {
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly IRaceService _raceService;
        private readonly IRegistrationService _registrationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EnhancedTestController> _logger;

        public EnhancedTestController(
            IGoogleSheetsService googleSheetsService,
            IRaceService raceService,
            IRegistrationService registrationService,
            IUnitOfWork unitOfWork,
            ILogger<EnhancedTestController> logger)
        {
            _googleSheetsService = googleSheetsService;
            _raceService = raceService;
            _registrationService = registrationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Test enhanced Google Sheets reading with real column mapping
        /// </summary>
        [HttpGet("test-enhanced-sheets/{sheetId}")]
        public async Task<IActionResult> TestEnhancedSheets(string sheetId,
            [FromQuery] string? credentialPath = null,
            [FromQuery] int fromRow = 1)
        {
            try
            {
                var registrations = await _googleSheetsService
                    .ReadNewRegistrationsAsync(sheetId, fromRow, credentialPath);

                return Ok(new
                {
                    SheetId = sheetId,
                    CredentialPath = credentialPath ?? "default",
                    FromRow = fromRow,
                    Count = registrations.Count(),
                    Registrations = registrations.Take(5), // Show first 5 for testing
                    Summary = new
                    {
                        ValidEmails = registrations.Count(r => !string.IsNullOrEmpty(r.Email)),
                        WithBirthDates = registrations.Count(r => r.DateOfBirth.HasValue),
                        ShirtCategories = registrations
                            .Where(r => !string.IsNullOrEmpty(r.ShirtCategory))
                            .GroupBy(r => r.ShirtCategory)
                            .ToDictionary(g => g.Key!, g => g.Count()),
                        Distances = registrations
                            .Where(r => !string.IsNullOrEmpty(r.Distance))
                            .GroupBy(r => r.Distance)
                            .ToDictionary(g => g.Key!, g => g.Count())
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing enhanced sheets for {SheetId}", sheetId);
                return BadRequest(new { Error = ex.Message, SheetId = sheetId });
            }
        }

        /// <summary>
        /// Test creating race with shirt types
        /// </summary>
        //[HttpPost("test-create-enhanced-race")]
        //public async Task<IActionResult> TestCreateEnhancedRace([FromBody] CreateRaceDto dto)
        //{
        //    try
        //    {
        //        // Test connection first
        //        var isConnected = await _googleSheetsService.TestConnectionAsync(dto.SheetConfigId, dto.GoogleCredentialPath);
        //        if (!isConnected)
        //        {
        //            return BadRequest(new { Error = "Cannot connect to Google Sheet", SheetId = dto.SheetId });
        //        }

        //        // Create race
        //        var race = await _raceService.CreateRaceAsync(dto);

        //        return Ok(new
        //        {
        //            Message = "Enhanced race created successfully",
        //            Race = race,
        //            Features = new
        //            {
        //                MultipleCredentials = !string.IsNullOrEmpty(dto.GoogleCredentialPath),
        //                ShirtTypes = race.ShirtTypes.Count,
        //                Distances = race.Distances.Count
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating enhanced race: {RaceName}", dto.Name);
        //        return BadRequest(new { Error = ex.Message });
        //    }
        //}

        /// <summary>
        /// Test full workflow: create race → sync → check data
        /// </summary>
        [HttpPost("test-full-workflow")]
        public async Task<IActionResult> TestFullWorkflow([FromBody] CreateRaceDto dto)
        {
            try
            {
                _logger.LogInformation("Starting full workflow test for race: {RaceName}", dto.Name);

                // Step 1: Create race
                var race = await _raceService.CreateRaceAsync(dto);
                _logger.LogInformation("✅ Created race with ID: {RaceId}", race.Id);

                // Step 2: Sync registrations
                var syncResult = await _registrationService.SyncRegistrationsFromSheetAsync(race.Id);
                _logger.LogInformation("✅ Synced {Count} registrations", syncResult.Added);

                // Step 3: Get statistics
                var stats = await _raceService.GetRaceStatisticsAsync(race.Id);
                _logger.LogInformation("✅ Generated statistics");

                // Step 4: Get detailed registrations
                var registrations = await _registrationService.GetRegistrationsByRaceAsync(race.Id);
                _logger.LogInformation("✅ Retrieved {Count} detailed registrations", registrations.Count());

                return Ok(new
                {
                    Message = "Full workflow completed successfully",
                    WorkflowSteps = new
                    {
                        Step1_RaceCreated = race,
                        Step2_SyncResult = syncResult,
                        Step3_Statistics = stats,
                        Step4_SampleRegistrations = registrations.Take(3)
                    },
                    Summary = new
                    {
                        TotalRegistrations = stats.TotalRegistrations,
                        ShirtCategoryDistribution = registrations
                            .Where(r => !string.IsNullOrEmpty(r.ShirtCategory))
                            .GroupBy(r => r.ShirtCategory!)
                            .ToDictionary(g => g.Key, g => g.Count()),
                        AgeDistribution = registrations
                            .Where(r => r.DateOfBirth.HasValue)
                            .Select(r => r.Age)
                            .GroupBy(age => GetAgeGroup(age))
                            .ToDictionary(g => g.Key, g => g.Count()),
                        DistanceDistribution = registrations
                            .GroupBy(r => r.Distance)
                            .ToDictionary(g => g.Key, g => g.Count())
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in full workflow test for race: {RaceName}", dto.Name);
                return BadRequest(new { Error = ex.Message, Step = "Failed during workflow" });
            }
        }

        /// <summary>
        /// Test database connectivity and enhanced tables
        /// </summary>
        [HttpGet("test-database-enhanced")]
        public async Task<IActionResult> TestDatabaseEnhanced()
        {
            try
            {
                var raceCount = await _unitOfWork.Races.CountAsync(r => true);
                var registrationCount = await _unitOfWork.Registrations.CountAsync(r => true);
                var shirtTypeCount = await _unitOfWork.RaceShirtTypes.CountAsync(st => true);

                // Test new fields
                var registrationsWithBirthDate = await _unitOfWork.Registrations
                    .CountAsync(r => r.DateOfBirth.HasValue);
                var registrationsWithShirtInfo = await _unitOfWork.Registrations
                    .CountAsync(r => !string.IsNullOrEmpty(r.ShirtCategory));

                return Ok(new
                {
                    Status = "Connected to enhanced database",
                    TableCounts = new
                    {
                        Races = raceCount,
                        Registrations = registrationCount,
                        ShirtTypes = shirtTypeCount
                    },
                    EnhancedFeatures = new
                    {
                        RegistrationsWithBirthDate = registrationsWithBirthDate,
                        RegistrationsWithShirtInfo = registrationsWithShirtInfo,
                        NewTablesWorking = shirtTypeCount >= 0 // Confirms RaceShirtTypes table exists
                    },
                    DatabaseSchema = "Enhanced with shirt management support"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database enhanced test failed");
                return StatusCode(500, new { Error = ex.Message, Status = "Database connection failed" });
            }
        }

    private static string GetAgeGroup(int age)
            {
                return age switch
                {
                    < 18 => "Dưới 18",
                    >= 18 and < 30 => "18-29",
                    >= 30 and < 40 => "30-39",
                    >= 40 and < 50 => "40-49",
                    >= 50 and < 60 => "50-59",
                    >= 60 => "60+"
                };
            }
        }
    }
