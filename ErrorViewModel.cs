using System;

namespace ContractMonthlyClaimSystem.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ErrorMessage { get; set; }

        public int? StatusCode { get; set; }

        public string? ErrorTitle { get; set; }

        public DateTime ErrorTime { get; set; } = DateTime.UtcNow;

        public string? StackTrace { get; set; }

        public string? ControllerName { get; set; }

        public string? ActionName { get; set; }

        public string? UserId { get; set; }
    }
}