using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMCS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Services
{
    public interface IClaimAutomationService
    {
        Task<decimal> CalculateTotalAmount(decimal hoursWorked, decimal hourlyRate);
        Task<bool> ValidateClaim(Claim claim);
        Task<bool> MeetsApprovalCriteria(Claim claim);
        Task ProcessAutoApproval(Claim claim);
        Task GenerateMonthlyReport(DateTime month);
        Task<string> GenerateInvoice(int claimId);
    }

    public class ClaimAutomationService : IClaimAutomationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ClaimAutomationService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<decimal> CalculateTotalAmount(decimal hoursWorked, decimal hourlyRate)
        {
            // Auto-calculation with validation
            if (hoursWorked <= 0 || hourlyRate <= 0)
                throw new ArgumentException("Hours worked and hourly rate must be positive");

            decimal total = hoursWorked * hourlyRate;

            // Apply maximum limit if configured
            var maxAmount = _configuration.GetValue<decimal>("ClaimSettings:MaximumAmount", 50000);
            return Math.Min(total, maxAmount);
        }

        public async Task<bool> ValidateClaim(Claim claim)
        {
            var validationErrors = new List<string>();

            // Hours validation
            if (claim.HoursWorked < 0.1m || claim.HoursWorked > 200m)
                validationErrors.Add("Hours worked must be between 0.1 and 200");

            // Rate validation
            if (claim.HourlyRate < 50 || claim.HourlyRate > 1000)
                validationErrors.Add("Hourly rate must be between 50 and 1000");

            // Monthly limit check
            var monthlyTotal = await GetMonthlyTotal(claim.LecturerId, DateTime.Now);
            var monthlyLimit = _configuration.GetValue<decimal>("ClaimSettings:MonthlyLimit", 1000);
            if (monthlyTotal + claim.HoursWorked > monthlyLimit)
                validationErrors.Add($"Monthly hours limit exceeded. Current: {monthlyTotal}, Limit: {monthlyLimit}");

            return !validationErrors.Any();
        }

        public async Task<bool> MeetsApprovalCriteria(Claim claim)
        {
            // Automated criteria checking
            var criteria = new List<bool>
            {
                claim.HoursWorked <= 160, // Maximum hours per month
                claim.HourlyRate <= 500, // Maximum rate
                claim.TotalAmount <= 50000, // Maximum amount
                await HasValidContract(claim.LecturerId), // Active contract
                !await HasDuplicateClaim(claim) // No duplicate claims
            };

            return criteria.All(c => c);
        }

        public async Task ProcessAutoApproval(Claim claim)
        {
            if (await MeetsApprovalCriteria(claim) && claim.TotalAmount < 10000)
            {
                claim.Status = ClaimStatus.Approved;
                claim.ProcessedDate = DateTime.Now;
                claim.ProcessedBy = "System Auto-Approval";

                _context.Claims.Update(claim);
                await _context.SaveChangesAsync();
            }
        }

        public async Task GenerateMonthlyReport(DateTime month)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.SubmissionDate >= startDate && c.SubmissionDate <= endDate)
                .ToListAsync();

            var reportData = new
            {
                Month = month.ToString("yyyy-MM"),
                TotalClaims = claims.Count,
                ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved),
                TotalAmount = claims.Where(c => c.Status == ClaimStatus.Approved).Sum(c => c.TotalAmount),
                ClaimsByStatus = claims.GroupBy(c => c.Status).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            // Save report to database or generate file
            var report = new HRReport
            {
                ReportType = "Monthly",
                GeneratedDate = DateTime.Now,
                GeneratedBy = "System",
                ReportData = System.Text.Json.JsonSerializer.Serialize(reportData)
            };

            _context.HRReports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerateInvoice(int claimId)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null || claim.Status != ClaimStatus.Approved)
                throw new InvalidOperationException("Claim not found or not approved");

            // Generate invoice data
            var invoiceData = new
            {
                InvoiceNumber = $"INV-{claimId}-{DateTime.Now:yyyyMMdd}",
                ClaimId = claimId,
                LecturerName = claim.Lecturer?.FullName,
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                TotalAmount = claim.TotalAmount,
                SubmissionDate = claim.SubmissionDate,
                InvoiceDate = DateTime.Now
            };

            return System.Text.Json.JsonSerializer.Serialize(invoiceData);
        }

        private async Task<decimal> GetMonthlyTotal(string lecturerId, DateTime date)
        {
            var startDate = new DateTime(date.Year, date.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.Claims
                .Where(c => c.LecturerId == lecturerId &&
                           c.SubmissionDate >= startDate &&
                           c.SubmissionDate <= endDate &&
                           c.Status != ClaimStatus.Rejected)
                .SumAsync(c => c.HoursWorked);
        }

        private async Task<bool> HasValidContract(string lecturerId)
        {
            var lecturer = await _context.Users.FindAsync(lecturerId);
            // Implement contract validation logic
            return lecturer != null; // Simplified for example
        }

        private async Task<bool> HasDuplicateClaim(Claim claim)
        {
            return await _context.Claims
                .AnyAsync(c => c.LecturerId == claim.LecturerId &&
                              c.SubmissionDate.Date == claim.SubmissionDate.Date &&
                              c.HoursWorked == claim.HoursWorked &&
                              c.ClaimId != claim.ClaimId);
        }
    }
}