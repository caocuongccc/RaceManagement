using Microsoft.Extensions.Logging;
using RaceManagement.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Services
{
    //public class PaymentSyncService
    //{
    //    private readonly AppDbContext _context;
    //    private readonly EmailService _emailService; // đã có trước đó
    //    private readonly ILogger<PaymentSyncService> _logger;

    //    public PaymentSyncService(AppDbContext context, EmailService emailService, ILogger<PaymentSyncService> logger)
    //    {
    //        _context = context;
    //        _emailService = emailService;
    //        _logger = logger;
    //    }

    //    public async Task SyncPaymentsAsync(int raceId)
    //    {
    //        var race = await _context.Races
    //            .Include(r => r.Registrations)
    //            .FirstOrDefaultAsync(r => r.Id == raceId);

    //        if (race == null)
    //        {
    //            _logger.LogWarning("Race {RaceId} not found", raceId);
    //            return;
    //        }

    //        // TODO: gọi GoogleSheetsService để lấy dữ liệu Payment sheet
    //        var paymentData = await FakeReadPaymentSheet(raceId); // demo

    //        foreach (var payment in paymentData)
    //        {
    //            var reg = race.Registrations.FirstOrDefault(r =>
    //                r.Email.Equals(payment.Email, StringComparison.OrdinalIgnoreCase));

    //            if (reg == null) continue;

    //            if (reg.PaymentStatus != PaymentStatus.Paid)
    //            {
    //                reg.PaymentStatus = PaymentStatus.Paid;
    //                reg.BibNumber ??= GenerateBibNumber(raceId, reg.DistanceId);
    //                reg.BibSentAt = DateTime.UtcNow;

    //                _context.Update(reg);

    //                // enqueue email job
    //                await _emailService.EnqueueBibConfirmationEmailAsync(reg);
    //            }
    //        }

    //        await _context.SaveChangesAsync();
    //    }

    //    private Task<List<(string Email, decimal Amount)>> FakeReadPaymentSheet(int raceId)
    //    {
    //        // TODO: gọi GoogleSheetsService đọc sheet thật
    //        return Task.FromResult(new List<(string Email, decimal Amount)>
    //        {
    //            ("user1@gmail.com", 200000),
    //            ("user2@gmail.com", 300000),
    //        });
    //    }

    //    private string GenerateBibNumber(int raceId, int distanceId)
    //    {
    //        // Rule: RaceId + DistanceId + Running count
    //        var count = _context.Registrations
    //            .Count(r => r.RaceId == raceId && r.DistanceId == distanceId && r.PaymentStatus == PaymentStatus.Paid);

    //        return $"{distanceId:D2}{count + 1:D04}";
    //    }
    //}
}
