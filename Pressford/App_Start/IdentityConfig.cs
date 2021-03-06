﻿using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using AspNet.Identity.MongoDB;

using Pressford.Models;

namespace Pressford
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationIdentityContext>().Users));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is: {0}"
            });
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "SecurityCode",
                BodyFormat = "Your security code is {0}"
            });
            
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        public enum SignInStatus
        {
            Success,
            LockedOut,
            RequiresTwoFactorAuthentication,
            Failure
        }

        // These help with sign and two factor (will possibly be moved into identity framework itself)
        public class SignInHelper
        {
            public SignInHelper(ApplicationUserManager userManager, IAuthenticationManager authManager)
            {
                UserManager = userManager;
                AuthenticationManager = authManager;
            }

            public ApplicationUserManager UserManager { get; private set; }
            public IAuthenticationManager AuthenticationManager { get; private set; }

            public async Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser)
            {
                // Clear any partial cookies from external or two factor partial sign ins
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
                var userIdentity = await user.GenerateUserIdentityAsync(UserManager, DefaultAuthenticationTypes.TwoFactorCookie);
                if (rememberBrowser)
                {
                    var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id);
                    AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity, rememberBrowserIdentity);
                }
                else
                {
                    AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
                }
            }

            public async Task<bool> SendTwoFactorCode(string provider)
            {
                var userId = await GetVerifiedUserIdAsync();
                if (userId == null)
                {
                    return false;
                }

                var token = await UserManager.GenerateTwoFactorTokenAsync(userId, provider);
                // See IdentityConfig.cs to plug in Email/SMS services to actually send the code
                await UserManager.NotifyTwoFactorTokenAsync(userId, provider, token);
                return true;
            }

            public async Task<string> GetVerifiedUserIdAsync()
            {
                var result = await AuthenticationManager.AuthenticateAsync(DefaultAuthenticationTypes.TwoFactorCookie);
                if (result != null && result.Identity != null && !String.IsNullOrEmpty(result.Identity.GetUserId()))
                {
                    return result.Identity.GetUserId();
                }
                return null;
            }

            public async Task<bool> HasBeenVerified()
            {
                return await GetVerifiedUserIdAsync() != null;
            }

            public async Task<SignInStatus> TwoFactorSignIn(string provider, string code, bool isPersistent, bool rememberBrowser)
            {
                var userId = await GetVerifiedUserIdAsync();
                if (userId == null)
                {
                    return SignInStatus.Failure;
                }
                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return SignInStatus.Failure;
                }
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return SignInStatus.LockedOut;
                }
                if (await UserManager.VerifyTwoFactorTokenAsync(user.Id, provider, code))
                {
                    // When token is verified correctly, clear the access failed count used for lockout
                    await UserManager.ResetAccessFailedCountAsync(user.Id);
                    await SignInAsync(user, isPersistent, rememberBrowser);
                    return SignInStatus.Success;
                }
                // If the token is incorrect, record the failure which also may cause the user to be locked out
                await UserManager.AccessFailedAsync(user.Id);
                return SignInStatus.Failure;
            }

            public async Task<SignInStatus> ExternalSignIn(ExternalLoginInfo loginInfo, bool isPersistent)
            {
                var user = await UserManager.FindAsync(loginInfo.Login);
                if (user == null)
                {
                    return SignInStatus.Failure;
                }
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return SignInStatus.LockedOut;
                }
                return await SignInOrTwoFactor(user, isPersistent);
            }

            private async Task<SignInStatus> SignInOrTwoFactor(ApplicationUser user, bool isPersistent)
            {
                if (await UserManager.GetTwoFactorEnabledAsync(user.Id) &&
                    !await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id))
                {
                    var identity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    AuthenticationManager.SignIn(identity);
                    return SignInStatus.RequiresTwoFactorAuthentication;
                }
                await SignInAsync(user, isPersistent, false);
                return SignInStatus.Success;

            }

            public async Task<SignInStatus> PasswordSignIn(string userName, string password, bool isPersistent, bool shouldLockout)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return SignInStatus.Failure;
                }
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return SignInStatus.LockedOut;
                }
                if (await UserManager.CheckPasswordAsync(user, password))
                {
                    return await SignInOrTwoFactor(user, isPersistent);
                }
                if (shouldLockout)
                {
                    // If lockout is requested, increment access failed count which might lock out the user
                    await UserManager.AccessFailedAsync(user.Id);
                    if (await UserManager.IsLockedOutAsync(user.Id))
                    {
                        return SignInStatus.LockedOut;
                    }
                }
                return SignInStatus.Failure;
            }
        }
    }
}
