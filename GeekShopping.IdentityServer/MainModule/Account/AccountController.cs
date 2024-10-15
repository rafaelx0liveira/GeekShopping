using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using GeekShopping.IdentityServer.MainModule.Account;
using GeekShopping.IdentityServer.Model;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
/// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
/// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
/// </summary>
[SecurityHeaders]
[AllowAnonymous]
public class AccountController(IIdentityServerInteractionService interaction, IClientStore clientStore, IAuthenticationSchemeProvider schemeProvider, IIdentityProviderStore identityProviderStore, IEventService events,
    UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager) : Controller
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    private readonly IIdentityServerInteractionService _interaction = interaction;
    private readonly IClientStore _clientStore = clientStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider = schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore = identityProviderStore;
    private readonly IEventService _events = events;

    /// <summary>
    /// Entry point into the login workflow
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl)
    {
        var vm = await BuildLoginViewModelAsync(returnUrl);

        if (vm.IsExternalLoginOnly)
        {
            return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
        }

        return View(vm);
    }

    /// <summary>
    /// Handle postback from username/password login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel model, string button)
    {
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

        if (button != "login")
        {
            if (context != null)
            {
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                if (context.IsNativeClient())
                    return this.LoadingPage("Redirect", model.ReturnUrl!);

                return Redirect(model.ReturnUrl!);
            }
            else
                return Redirect("~/");
        }

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username!, model.Password!, model.RememberLogin, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username!);
                await _events.RaiseAsync(
                    new UserLoginSuccessEvent(user!.UserName,
                        user.Id,
                        user.UserName,
                        clientId: context?.Client.ClientId));

                AuthenticationProperties? props = null;
                if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                {
                    props = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                    };
                };

                var isuser = new IdentityServerUser(user.Id)
                {
                    DisplayName = user.UserName
                };

                await HttpContext.SignInAsync(isuser, props);

                if (context != null)
                {
                    if (context.IsNativeClient())
                        return this.LoadingPage("Redirect", model.ReturnUrl!);

                    return Redirect(model.ReturnUrl!);
                }

                if (Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                else if (string.IsNullOrEmpty(model.ReturnUrl))
                    return Redirect("~/");
                else
                    throw new Exception("invalid return URL");
            }

            await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
            ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
        }

        var vm = await BuildLoginViewModelAsync(model);
        return View(vm);
    }


    /// <summary>
    /// Show logout page
    /// </summary>
    [HttpGet]
    [Obsolete]
    public async Task<IActionResult> Logout(string logoutId)
    {
        var vm = await BuildLogoutViewModelAsync(logoutId);

        if (vm.ShowLogoutPrompt == false)
            return await Logout(vm);

        return View(vm);
    }

    /// <summary>
    /// Handle logout page postback
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Obsolete]
    public async Task<IActionResult> Logout(LogoutInputModel model)
    {
        var vm = await BuildLoggedOutViewModelAsync(model.LogoutId!);

        if (User?.Identity!.IsAuthenticated == true)
        {
            await _signInManager.SignOutAsync();

            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
        }

        if (vm.TriggerExternalSignout)
        {
            string? url = Url.Action("Logout", new { logoutId = vm.LogoutId });

            return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme!);
        }

        return View("LoggedOut", vm);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Register(string returnUrl)
    {
        var vm = await BuildRegisterViewModelAsync(returnUrl);

        return View(vm);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName!,
                LastName = model.LastName!
            };

            var result = await _userManager.CreateAsync(user, model.Password!);
            if (result.Succeeded)
            {
                if (!_roleManager.RoleExistsAsync(model.RoleName!).GetAwaiter().GetResult())
                {
                    var userRole = new IdentityRole
                    {
                        Name = model.RoleName,
                        NormalizedName = model.RoleName,

                    };
                    await _roleManager.CreateAsync(userRole);
                }

                await _userManager.AddToRoleAsync(user, model.RoleName!);

                await _userManager.AddClaimsAsync(user, [
                new(JwtClaimTypes.Name, model.Username!),
                new(JwtClaimTypes.Email, model.Email!),
                new(JwtClaimTypes.FamilyName, model.FirstName!),
                new(JwtClaimTypes.GivenName, model.LastName!),
                new(JwtClaimTypes.WebSite, $"http://{model.Username}.com"),
                new(JwtClaimTypes.Role,"User") ]);

                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                var loginresult = await _signInManager.PasswordSignInAsync(model.Username!, model.Password!, false, lockoutOnFailure: true);
                if (loginresult.Succeeded)
                {
                    var checkuser = await _userManager.FindByNameAsync(model.Username!);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(checkuser!.UserName, checkuser.Id, checkuser.UserName, clientId: context?.Client.ClientId));

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                            return this.LoadingPage("Redirect", model.ReturnUrl!);

                        return Redirect(model.ReturnUrl!);
                    }

                    if (Url.IsLocalUrl(model.ReturnUrl))
                        return Redirect(model.ReturnUrl);
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                        return Redirect("~/");
                    else
                        throw new Exception("invalid return URL");
                }

            }
        }

        return View(model);
    }

    private async Task<RegisterViewModel> BuildRegisterViewModelAsync(string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        List<string> roles = [];
        roles.Add("Admin");
        roles.Add("Client");
        ViewBag.message = roles;
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

            var vm = new RegisterViewModel
            {
                EnableLocalLogin = local,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint!,
            };

            if (!local)
                vm.ExternalProviders = [new ExternalProvider { AuthenticationScheme = context!.IdP }];

            return vm;
        }

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ExternalProvider
            {
                DisplayName = x.DisplayName ?? x.Name,
                AuthenticationScheme = x.Name
            }).ToList();

        var allowLocal = true;
        if (context?.Client.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;

                if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Count != 0)
                    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
            }
        }

        return new RegisterViewModel
        {
            AllowRememberLogin = AccountOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
            ReturnUrl = returnUrl,
            Username = context?.LoginHint!,
            ExternalProviders = [.. providers]
        };
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

            var vm = new LoginViewModel
            {
                EnableLocalLogin = local,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint!,
            };

            if (!local)
                vm.ExternalProviders = [new ExternalProvider { AuthenticationScheme = context!.IdP }];

            return vm;
        }

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes.Where(x => x.DisplayName != null).Select(x => new ExternalProvider
        {
            DisplayName = x.DisplayName ?? x.Name,
            AuthenticationScheme = x.Name
        }).ToList();

        var dyanmicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync()).Where(x => x.Enabled).Select(x => new ExternalProvider
        {
            AuthenticationScheme = x.Scheme,
            DisplayName = x.DisplayName
        });
        providers.AddRange(dyanmicSchemes);

        var allowLocal = true;
        if (context?.Client.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;

                if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Count != 0)
                    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
            }
        }

        return new LoginViewModel
        {
            AllowRememberLogin = AccountOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
            ReturnUrl = returnUrl,
            Username = context?.LoginHint!,
            ExternalProviders = [.. providers]
        };
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
    {
        var vm = await BuildLoginViewModelAsync(model.ReturnUrl!);
        vm.Username = model.Username;
        vm.RememberLogin = model.RememberLogin;
        return vm;
    }

    private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
    {
        var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

        if (User?.Identity!.IsAuthenticated != true)
        {
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        var context = await _interaction.GetLogoutContextAsync(logoutId);
        if (context?.ShowSignoutPrompt == false)
        {
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        return vm;
    }

    [Obsolete]
    private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
    {
        var logout = await _interaction.GetLogoutContextAsync(logoutId);

        var vm = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
            ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            SignOutIframeUrl = logout?.SignOutIFrameUrl,
            LogoutId = logoutId
        };

        if (User?.Identity!.IsAuthenticated == true)
        {
            var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                if (providerSupportsSignout)
                {
                    vm.LogoutId ??= await _interaction.CreateLogoutContextAsync();

                    vm.ExternalAuthenticationScheme = idp;
                }
            }
        }

        return vm;
    }
}