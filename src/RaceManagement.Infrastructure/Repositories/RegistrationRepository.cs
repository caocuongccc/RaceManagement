using Microsoft.EntityFrameworkCore;
using RaceManagement.Core.Entities;
using RaceManagement.Abstractions.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Repositories
{
    public class RegistrationRepository : Repository<Registration>, IRegistrationRepository
    {
        public RegistrationRepository(RaceManagementDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Registration>> GetPendingPaymentRegistrationsAsync(int raceId)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Distance)
                    .Include(r => r.Race)
                    .Include(r => r.ShirtType)
                    .Where(r => r.RaceId == raceId &&
                               r.PaymentStatus == PaymentStatus.Pending)
                    .OrderBy(r => r.RegistrationTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving pending payment registrations for race {raceId}", ex);
            }
        }

        public async Task<IEnumerable<Registration>> GetPaidRegistrationsAsync(int raceId)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Distance)
                    .Include(r => r.Race)
                    .Include(r => r.ShirtType)
                    .Where(r => r.RaceId == raceId &&
                               r.PaymentStatus == PaymentStatus.Paid)
                    .OrderBy(r => r.RegistrationTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving paid registrations for race {raceId}", ex);
            }
        }

        public async Task<IEnumerable<Registration>> GetByRaceIdAsync(int raceId)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Distance)
                    .Include(r => r.ShirtType)
                    .Include(r => r.Race)
                    .Where(r => r.RaceId == raceId)
                    .OrderBy(r => r.RegistrationTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving registrations for race {raceId}", ex);
            }
        }
        public async Task<Registration?> GetRegistrationWithDetailsAsync(int registrationId)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Race)
                        .ThenInclude(race => race.SheetConfig)
                            .ThenInclude(sc => sc.Credential)
                    .Include(r => r.Distance)
                    .Include(r => r.ShirtType)
                    .AsSplitQuery() // For performance with multiple includes
                    .FirstOrDefaultAsync(r => r.Id == registrationId);
            }
            catch (Exception ex)
            {
                // Log error if needed
                throw new InvalidOperationException($"Error retrieving registration {registrationId} with details", ex);
            }
        }
        public async Task<Registration?> GetByIdWithIncludesAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Race)
                .Include(r => r.Distance)
                .Include(r => r.Payments)
                .Include(r => r.EmailLogs)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Registration?> GetByTransactionReferenceAsync(string transactionReference)
        {
            return await _dbSet
                .Include(r => r.Race)
                .Include(r => r.Distance)
                .FirstOrDefaultAsync(r => r.TransactionReference == transactionReference);
        }

        public async Task<int> GetLastSheetRowIndexAsync(int raceId)
        {
            var lastRow = await _dbSet
                .Where(r => r.RaceId == raceId && r.SheetRowIndex.HasValue)
                .OrderByDescending(r => r.SheetRowIndex)
                .FirstOrDefaultAsync();

            return lastRow?.SheetRowIndex ?? 1; // Start from row 2 (after header)
        }

        //public async Task<IEnumerable<Registration>> GetByRaceIdAsync(int raceId)
        //{
        //    return await _dbSet
        //        .Include(r => r.Distance)
        //        .Where(r => r.RaceId == raceId)
        //        .OrderByDescending(r => r.RegistrationTime)
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<Registration>> GetPendingPaymentsAsync(int raceId)
        {
            return await _dbSet
                .Include(r => r.Distance)
                .Where(r => r.RaceId == raceId && r.PaymentStatus == PaymentStatus.Pending)
                .OrderBy(r => r.RegistrationTime)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Registration>> GetPaidRegistrationsAsync(int raceId)
        //{
        //    return await _dbSet
        //        .Include(r => r.Distance)
        //        .Where(r => r.RaceId == raceId && r.PaymentStatus == PaymentStatus.Paid)
        //        .OrderBy(r => r.BibNumber)
        //        .ToListAsync();
        //}
        // Additional helper methods that might be needed
        public async Task<Registration?> GetRegistrationByEmailAndRaceAsync(string email, int raceId)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Distance)
                    .Include(r => r.ShirtType)
                    .Include(r => r.Race)
                    .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower() &&
                                            r.RaceId == raceId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving registration by email {email} for race {raceId}", ex);
            }
        }

        public async Task<IEnumerable<Registration>> GetRegistrationsWithBibNumbersAsync(int raceId)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Distance)
                    .Include(r => r.ShirtType)
                    .Include(r => r.Race)
                    .Where(r => r.RaceId == raceId &&
                               !string.IsNullOrEmpty(r.BibNumber))
                    .OrderBy(r => r.BibNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving registrations with BIB numbers for race {raceId}", ex);
            }
        }

        public async Task<IEnumerable<Registration>> GetRegistrationsNeedingBibAssignmentAsync(int raceId)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Distance)
                    .Include(r => r.Race)
                    .Where(r => r.RaceId == raceId &&
                               r.PaymentStatus == PaymentStatus.Paid &&
                               string.IsNullOrEmpty(r.BibNumber))
                    .OrderBy(r => r.RegistrationTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving registrations needing BIB assignment for race {raceId}", ex);
            }
        }

        public async Task<bool> IsEmailAlreadyRegisteredAsync(string email, int raceId)
        {
            try
            {
                return await _context.Registrations
                    .AnyAsync(r => r.Email.ToLower() == email.ToLower() &&
                                  r.RaceId == raceId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking email registration status for {email} in race {raceId}", ex);
            }
        }

        public async Task<int> GetRegistrationCountByStatusAsync(int raceId, PaymentStatus status)
        {
            try
            {
                return await _context.Registrations
                    .CountAsync(r => r.RaceId == raceId && r.PaymentStatus == status);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error counting registrations by status for race {raceId}", ex);
            }
        }

        public async Task<Dictionary<PaymentStatus, int>> GetRegistrationStatusSummaryAsync(int raceId)
        {
            try
            {
                return await _context.Registrations
                    .Where(r => r.RaceId == raceId)
                    .GroupBy(r => r.PaymentStatus)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting registration status summary for race {raceId}", ex);
            }
        }
        // NEW: Get registrations by email (for duplicate check)
        public async Task<Registration?> GetByEmailAndRaceAsync(string email, int raceId)
        {
            return await _dbSet
                .Include(r => r.Distance)
                .FirstOrDefaultAsync(r => r.Email == email && r.RaceId == raceId);
        }

        // NEW: Get registrations by shirt category
        public async Task<IEnumerable<Registration>> GetByShirtCategoryAsync(int raceId, string shirtCategory)
        {
            return await _dbSet
                .Include(r => r.Distance)
                .Where(r => r.RaceId == raceId && r.ShirtCategory == shirtCategory)
                .OrderBy(r => r.ShirtSize)
                .ToListAsync();
        }

        // NEW: Get shirt size statistics
        public async Task<Dictionary<string, int>> GetShirtSizeStatisticsAsync(int raceId)
        {
            return await _dbSet
                .Where(r => r.RaceId == raceId && !string.IsNullOrEmpty(r.ShirtSize))
                .GroupBy(r => r.ShirtSize!)
                .Select(g => new { Size = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Size, x => x.Count);
        }

        // NEW: Get age group statistics
        public async Task<Dictionary<string, int>> GetAgeGroupStatisticsAsync(int raceId)
        {
            var registrations = await _dbSet
                .Where(r => r.RaceId == raceId && r.DateOfBirth.HasValue)
                .Select(r => r.DateOfBirth!.Value)
                .ToListAsync();

            var ageGroups = registrations
                .Select(dob => CalculateAge(dob))
                .GroupBy(age => GetAgeGroup(age))
                .ToDictionary(g => g.Key, g => g.Count());

            return ageGroups;
        }

        // NEW: Get gender statistics  
        public async Task<Dictionary<string, int>> GetGenderStatisticsAsync(int raceId)
        {
            return await _dbSet
                .Where(r => r.RaceId == raceId && r.Gender.HasValue)
                .GroupBy(r => r.Gender!.Value)
                .Select(g => new { Gender = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Gender, x => x.Count);
        }

        // NEW: Get registrations needing BIB numbers
        public async Task<IEnumerable<Registration>> GetRegistrationsNeedingBibAsync(int raceId)
        {
            return await _dbSet
                .Include(r => r.Distance)
                .Where(r => r.RaceId == raceId &&
                           r.PaymentStatus == PaymentStatus.Paid &&
                           string.IsNullOrEmpty(r.BibNumber))
                .OrderBy(r => r.DistanceId)
                .ThenBy(r => r.RegistrationTime)
                .ToListAsync();
        }

        // Helper methods
        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
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
