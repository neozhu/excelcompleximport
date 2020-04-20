using System.Security.Claims;
using System.Web.Mvc;

namespace WebApp
{
  public class ViewBagFilter : IActionFilter
  {
    private static readonly string Enabled = "Enabled";
    private static readonly string Disabled = string.Empty;

    public void OnActionExecuting(ActionExecutingContext context)
    {
      if (context.Controller is Controller controller)
      {
        
        var claimsidentity = (ClaimsIdentity)controller.User.Identity;
        var username = claimsidentity.FindFirst(ClaimTypes.Name)?.Value;
        var givenname = claimsidentity.FindFirst(ClaimTypes.GivenName)?.Value;
        var email = claimsidentity.FindFirst(ClaimTypes.Email)?.Value;
        var mobilephone = claimsidentity.FindFirst(ClaimTypes.MobilePhone)?.Value;
        var avatars = claimsidentity.FindFirst("http://schemas.microsoft.com/identity/claims/avatars")?.Value;
        var tenantid = claimsidentity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
        var tenantname = claimsidentity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantname")?.Value;
        var culture = claimsidentity.FindFirst(ClaimTypes.Country)?.Value;
        var role = claimsidentity.FindFirst(ClaimTypes.Role)?.Value;
        // SmartAdmin Toggle Features
        controller.ViewBag.AppSidebar = Enabled;
        controller.ViewBag.AppHeader = Enabled;
        controller.ViewBag.AppLayoutShortcut = Enabled;
        controller.ViewBag.AppFooter = Enabled;
        controller.ViewBag.ShortcutMenu = Enabled;
        controller.ViewBag.ChatInterface = Enabled;
        controller.ViewBag.LayoutSettings = Enabled;

        // SmartAdmin Default Settings
        controller.ViewBag.App = "omx app";
        controller.ViewBag.AppName = "智能接单平台";
        controller.ViewBag.AppFlavor = "smart admin v4";
        controller.ViewBag.AppFlavorSubscript = ".NET 4.8";
        controller.ViewBag.User = username;
        controller.ViewBag.Role = role;
        controller.ViewBag.Email = email;
        controller.ViewBag.Twitter = email;
        controller.ViewBag.Avatar = avatars;
        controller.ViewBag.MobilePhone = mobilephone;
        controller.ViewBag.GivenName = givenname;
        controller.ViewBag.TenantId = tenantid;
        controller.ViewBag.TenantName = tenantname;
        controller.ViewBag.Culture = culture;
        controller.ViewBag.Version = "4.0.2";
        controller.ViewBag.Bs4v = "4.3";
        controller.ViewBag.Logo = "logo.png";
        controller.ViewBag.LogoM = "logo.png";
        controller.ViewBag.Copyright = "2019 © SmartAdmin by&nbsp;<a href='https://www.gotbootstrap.com' class='text-primary fw-500' title='gotbootstrap.com' target='_blank'>gotbootstrap.com</a>";
        controller.ViewBag.CopyrightInverse = "2019 © SmartAdmin by&nbsp;<a href='https://www.gotbootstrap.com' class='text-white opacity-40 fw-500' title='gotbootstrap.com' target='_blank'>gotbootstrap.com</a>";
      }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
  }
}
