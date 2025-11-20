using System;

namespace CMCS.Models
{
    public class HRReport
    {
        public int ReportId { get; set; }
        public string ReportType { get; set; } // "Monthly", "Invoice", "Summary"
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public string ReportData { get; set; } // JSON or formatted data
        public string FilePath { get; set; }
    }

    public class LecturerData
    {
        public string LecturerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string BankAccount { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
