using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using WebApp.Models;
namespace WebApp
{
  public class EmailService : IIdentityMessageService
  {
    public Task SendAsync(IdentityMessage message)
    {
      // Credentials:
      // Credentials:
      var credentialUserName = "yourAccount@outlook.com";
      var sentFrom = "yourAccount@outlook.com";
      var pwd = "yourApssword";

      // Configure the client:
      var client =
          new System.Net.Mail.SmtpClient("smtp-mail.outlook.com")
          {
            Port = 587,
            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
          };

      // Creatte the credentials:
      var credentials =
          new System.Net.NetworkCredential(credentialUserName, pwd);

      client.EnableSsl = true;
      client.Credentials = credentials;

      // Create the message:
      var mail =
          new System.Net.Mail.MailMessage(sentFrom, message.Destination)
          {
            Subject = message.Subject,
            Body = message.Body
          };

      // Send:
      return client.SendMailAsync(mail);
    }
  }


  public class SmsService : IIdentityMessageService
  {
  public Task SendAsync(IdentityMessage message)
  {
    //string AccountSid = "YourTwilioAccountSID";
    //string AuthToken = "YourTwilioAuthToken";
    //string twilioPhoneNumber = "YourTwilioPhoneNumber";

    //var twilio = new TwilioRestClient(AccountSid, AuthToken);

    //twilio.SendSmsMessage(twilioPhoneNumber, message.Destination, message.Body);

    // Twilio does not return an async Task, so we need this:
    return Task.FromResult(0);
  }
}

  // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
  public class ApplicationUserManager : UserManager<ApplicationUser>
  {
    private static readonly UserStore<IdentityUser> UserStore = new UserStore<IdentityUser>();
    //private static readonly ApplicationUserManager Instance = new ApplicationUserManager();
    public ApplicationUserManager(IUserStore<ApplicationUser> store)
        : base(store)
    {
    }

    public virtual async Task<IdentityResult> AddUserToRolesAsync(
        string userId, IList<string> roles)
    {
      var userRoleStore = (IUserRoleStore<ApplicationUser, string>)Store;

      var user = await FindByIdAsync(userId).ConfigureAwait(false);
      if (user == null)
      {
        throw new InvalidOperationException("Invalid user Id");
      }

      var userRoles = await userRoleStore
          .GetRolesAsync(user)
          .ConfigureAwait(false);

      // Add user to each role using UserRoleStore
      foreach (var role in roles.Where(role => !userRoles.Contains(role)))
      {
        await userRoleStore.AddToRoleAsync(user, role).ConfigureAwait(false);
      }
      // Call update once when all roles are added
      return await UpdateAsync(user).ConfigureAwait(false);
    }


    public virtual async Task<IdentityResult> RemoveUserFromRolesAsync(
        string userId, IList<string> roles)
    {
      var userRoleStore = (IUserRoleStore<ApplicationUser, string>)Store;

      var user = await FindByIdAsync(userId).ConfigureAwait(false);
      if (user == null)
      {
        throw new InvalidOperationException("Invalid user Id");
      }

      var userRoles = await userRoleStore
          .GetRolesAsync(user)
          .ConfigureAwait(false);

      // Remove user to each role using UserRoleStore
      foreach (var role in roles.Where(userRoles.Contains))
      {
        await userRoleStore
            .RemoveFromRoleAsync(user, role)
            .ConfigureAwait(false);
      }
      // Call update once when all roles are removed
      return await UpdateAsync(user).ConfigureAwait(false);
    }

    public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
    {
      var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
      // Configure validation logic for usernames
      manager.UserValidator = new UserValidator<ApplicationUser>(manager)
      {
        AllowOnlyAlphanumericUserNames = false,
        RequireUniqueEmail = true
      };

      // Configure validation logic for passwords
      manager.PasswordValidator = new PasswordValidator
      {
        RequiredLength = 4,
        RequireNonLetterOrDigit = false,
        RequireDigit = false,
        RequireLowercase = false,
        RequireUppercase = false,
      };

      // Configure user lockout defaults
      manager.UserLockoutEnabledByDefault = true;
      manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
      manager.MaxFailedAccessAttemptsBeforeLockout = 5;

      // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
      // You can write your own provider and plug it in here.

      manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
      {
        MessageFormat = "Your security code is {0}"
      });
      manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
      {
        Subject = "Security Code",
        BodyFormat = "Your security code is {0}"
      });

      manager.EmailService = new EmailService();
      manager.SmsService = new SmsService();
      var dataProtectionProvider = options.DataProtectionProvider;

      if (dataProtectionProvider != null)
      {
        manager.UserTokenProvider =
                        new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));

      }

      return manager;
    }

    public static void Seed()
    {
      
      // Make sure we always have at least the demo user available to login with
      // this ensures the user does not have to explicitly register upon first use
      var admin = new IdentityUser
      {
        Id = "6bc8cee0-a03e-430b-9711-420ab0d6a596",
        Email = "admin@email.com",
        UserName = "Smart Admin",
        PasswordHash = "APc6/pVPfTnpG89SRacXjlT+sRz+JQnZROws0WmCA20+axszJnmxbRulHtDXhiYEuQ==",
        SecurityStamp = "18272ba5-bf6a-48a7-8116-3ac34dbb7f38"
      };

      UserStore.Context.Set<IdentityUser>().AddOrUpdate(admin);
      UserStore.Context.SaveChanges();
    }
  }

  // Configure the application sign-in manager which is used in this application.
  public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
  {
    public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
        : base(userManager, authenticationManager)
    {
    }
    public void SignOut() => this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.TwoFactorCookie);
    public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user) => user.GenerateUserIdentityAsync((ApplicationUserManager)this.UserManager, DefaultAuthenticationTypes.ApplicationCookie);

    public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context) => new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
  }


}
