using Microsoft.AspNetCore.Mvc;
using CUSoCalPhoneAssignments.Services;
namespace CUSoCalPhoneAssignments.Controllers;
public sealed class HomeController : Controller
{
    private readonly AssignmentStore _store;
    private readonly ILogger<HomeController> _logger;
    public HomeController(AssignmentStore store, ILogger<HomeController> logger) { _store = store; _logger = logger; }
    [HttpGet] public IActionResult Index() => View(_store.GetDashboard());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Assign(int memberId, int phoneId)
    {
        try { var error = _store.Assign(memberId, phoneId); TempData[error is null ? "Success" : "Error"] = error ?? "Phone assignment saved successfully."; }
        catch (IOException ex) { _logger.LogError(ex, "Unable to save assignment"); TempData["Error"] = "Unable to save changes. Please try again."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Remove(int memberId)
    {
        try { var error = _store.Remove(memberId); TempData[error is null ? "Success" : "Error"] = error ?? "Phone assignment removed."; }
        catch (IOException ex) { _logger.LogError(ex, "Unable to remove assignment"); TempData["Error"] = "Unable to save changes. Please try again."; }
        return RedirectToAction(nameof(Index));
    }
    public IActionResult Error() => View();
}
