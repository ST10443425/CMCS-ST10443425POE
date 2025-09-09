using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using CMCS.Models;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET: Home/Index
        public IActionResult Index()
        {
            _logger.LogInformation("Home page accessed at {Time}", DateTime.UtcNow);
            return View();
        }

        // GET: Home/LecturerDashboard
        [Authorize(Roles = "Lecturer")]
        public IActionResult LecturerDashboard()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Lecturer dashboard accessed by user {UserId}", userId);

                // Mock data for demonstration (non-functional prototype)
                var dashboardData = new
                {
                    PendingClaims = 3,
                    ApprovedClaims = 5,
                    RejectedClaims = 1,
                    TotalEarnings = 12500.00m,
                    RecentClaims = new List<object>
                    {
                        new { Date = DateTime.Now.AddDays(-2), Amount = 4500.00m, Status = "Approved" },
                        new { Date = DateTime.Now.AddDays(-5), Amount = 3200.00m, Status = "Pending" },
                        new { Date = DateTime.Now.AddDays(-10), Amount = 2800.00m, Status = "Paid" }
                    }
                };

                ViewBag.DashboardData = dashboardData;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing lecturer dashboard");
                return RedirectToAction("Error", new { message = "Unable to load lecturer dashboard" });
            }
        }

        // GET: Home/CoordinatorDashboard
        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult CoordinatorDashboard()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.IsInRole("AcademicManager") ? "Academic Manager" : "Programme Coordinator";

                _logger.LogInformation("{Role} dashboard accessed by user {UserId}", userRole, userId);

                // Mock data for demonstration
                var dashboardData = new
                {
                    PendingApprovals = 8,
                    ApprovedThisMonth = 15,
                    RejectedThisMonth = 2,
                    TotalClaimsProcessed = 25,
                    RecentActions = new List<object>
                    {
                        new { ClaimId = 1001, Lecturer = "Dr. Smith", Amount = 4500.00m, Action = "Approved", Date = DateTime.Now.AddHours(-2) },
                        new { ClaimId = 1002, Lecturer = "Prof. Johnson", Amount = 3200.00m, Action = "Pending", Date = DateTime.Now.AddHours(-5) },
                        new { ClaimId = 1003, Lecturer = "Dr. Brown", Amount = 2800.00m, Action = "Rejected", Date = DateTime.Now.AddHours(-8) }
                    }
                };

                ViewBag.DashboardData = dashboardData;
                ViewBag.UserRole = userRole;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing coordinator dashboard");
                return RedirectToAction("Error", new { message = "Unable to load coordinator dashboard" });
            }
        }

        // GET: Home/ManagerDashboard
        [Authorize(Roles = "AcademicManager,HR")]
        public IActionResult ManagerDashboard()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Manager dashboard accessed by user {UserId}", userId);

                // Mock data for demonstration
                var dashboardData = new
                {
                    TotalClaimsThisMonth = 45,
                    TotalAmount = 187500.00m,
                    AverageProcessingTime = "2.5 days",
                    DepartmentStats = new List<object>
                    {
                        new { Department = "Computer Science", Claims = 15, Amount = 62500.00m },
                        new { Department = "Business", Claims = 12, Amount = 48000.00m },
                        new { Department = "Engineering", Claims = 10, Amount = 42000.00m },
                        new { Department = "Arts", Claims = 8, Amount = 35000.00m }
                    },
                    PerformanceMetrics = new
                    {
                        ApprovalRate = 92.5,
                        RejectionRate = 7.5,
                        AverageClaimValue = 4166.67m
                    }
                };

                ViewBag.DashboardData = dashboardData;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing manager dashboard");
                return RedirectToAction("Error", new { message = "Unable to load manager dashboard" });
            }
        }

        // GET: Home/About
        public IActionResult About()
        {
            _logger.LogInformation("About page accessed");

            var aboutInfo = new
            {
                SystemName = "Contract Monthly Claim System (CMCS)",
                Version = "1.0.0",
                Description = "A comprehensive system for managing monthly claims for Independent Contractor Lecturers",
                Features = new List<string>
                {
                    "Easy claim submission for lecturers",
                    "Streamlined approval process for coordinators and managers",
                    "Document upload and management",
                    "Real-time status tracking",
                    "Comprehensive reporting"
                },
                ContactEmail = "support@cmcs.example.com",
                SupportPhone = "+27 11 123 4567"
            };

            ViewBag.AboutInfo = aboutInfo;
            return View();
        }

        // GET: Home/Contact
        public IActionResult Contact()
        {
            _logger.LogInformation("Contact page accessed");
            return View();
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // In a real application, this would send an email or save to database
            _logger.LogInformation("Contact form submitted: {Name}, {Email}, {Subject}", model.Name, model.Email, model.Subject);

            TempData["SuccessMessage"] = "Thank you for your message! We'll get back to you soon.";
            return RedirectToAction("Contact");
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy policy page accessed");
            return View();
        }

        // GET: Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string? message = null, int? statusCode = null)
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = message ?? "An unexpected error occurred while processing your request.",
                StatusCode = statusCode ?? HttpContext.Response.StatusCode,
                ErrorTitle = statusCode switch
                {
                    404 => "Page Not Found",
                    500 => "Internal Server Error",
                    403 => "Access Denied",
                    401 => "Unauthorized",
                    _ => "Error"
                },
                ControllerName = RouteData.Values["controller"]?.ToString(),
                ActionName = RouteData.Values["action"]?.ToString(),
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _logger.LogError("Error occurred: {ErrorMessage}, Status: {StatusCode}, User: {UserId}",
                errorViewModel.ErrorMessage, errorViewModel.StatusCode, errorViewModel.UserId);

            return View(errorViewModel);
        }

        // GET: Home/AccessDenied
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied for user {UserId} at {Path}",
                User.FindFirstValue(ClaimTypes.NameIdentifier), HttpContext.Request.Path);

            return View();
        }

        // GET: Home/SystemStatus
        [Authorize(Roles = "AcademicManager,HR,ProgrammeCoordinator")]
        public IActionResult SystemStatus()
        {
            try
            {
                // Mock system status information
                var systemStatus = new
                {
                    ServerTime = DateTime.UtcNow,
                    Uptime = "15 days, 7 hours, 23 minutes",
                    MemoryUsage = "1.2GB / 4GB",
                    ActiveUsers = 23,
                    DatabaseStatus = "Online",
                    StorageUsage = "2.5GB / 10GB",
                    LastBackup = DateTime.UtcNow.AddHours(-6),
                    SystemVersion = "v1.0.0"
                };

                ViewBag.SystemStatus = systemStatus;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system status");
                return RedirectToAction("Error", new { message = "Unable to retrieve system status" });
            }
        }
    }

    // Contact form model (can be in a separate file)
    public class ContactFormModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        public string? Department { get; set; }
        public bool IsUrgent { get; set; }
    }
}