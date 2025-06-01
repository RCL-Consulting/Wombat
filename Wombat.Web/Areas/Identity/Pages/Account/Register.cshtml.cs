// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Data;
using static Wombat.Data.WombatUser;

namespace Wombat.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<WombatUser> _signInManager;
        private readonly UserManager<WombatUser> _userManager;
        private readonly IUserStore<WombatUser> _userStore;
        private readonly IUserEmailStore<WombatUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly ISpecialityRepository _specialityRepository;
        private readonly ISubSpecialityRepository _subspecialityRepository;
        private readonly IWebHostEnvironment _environment;

        public Dictionary<string, string> CoordinatorsBySubspecialityAndInstitution { get; set; }

        public RegisterModel( UserManager<WombatUser> userManager,
                              IUserStore<WombatUser> userStore,
                              SignInManager<WombatUser> signInManager,
                              ILogger<RegisterModel> logger,
                              IEmailSender emailSender,
                              IInstitutionRepository institutionRepository,
                              ISpecialityRepository specialityRepository,
                              ISubSpecialityRepository subspecialityRepository,
                              IWebHostEnvironment environment )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _institutionRepository = institutionRepository;
            _specialityRepository = specialityRepository;
            _subspecialityRepository = subspecialityRepository;
            _environment = environment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public List<SelectListItem> Institutions { get; set; }

        public List<SelectListItem> Specialities { get; set; }

        public Dictionary<int, List<SelectListItem>> Subspecialities { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Surname")]
            public string Surname { get; set; }

            [Required]
            [Display(Name = "Institution")]
            public int InstitutionId { get; set; }

            [Required]
            [Display(Name = "Training start date")]
            public DateTime StartDate { get; set; }

            //[Required]
            [Display(Name = "Training Number")]
            public string IdNumber { get; set; }

            [Required]
            [Display(Name = "Speciality")]
            public int SpecialityId { get; set; }

            [Required]
            [Display(Name = "Subspeciality")]
            public int SubspecialityId { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DisplayName("HPCSA Number")]
            public string HPCSANumber { get;  set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Institutions = (await _institutionRepository.GetAllAsync())
                .Select(i => new SelectListItem(i.Name, i.Id.ToString()))
                .ToList();

            Specialities = (await _specialityRepository.GetAllAsync())
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();

            var subspecialities = (await _subspecialityRepository.GetAllAsync());
            Subspecialities = new Dictionary<int, List<SelectListItem>>();
            foreach (var item in subspecialities)
            {
                if (!Subspecialities.ContainsKey(item.SpecialityId))
                    Subspecialities[item.SpecialityId] = new List<SelectListItem>();
                Subspecialities[item.SpecialityId].Add(new SelectListItem(item.Name, item.Id.ToString()));
            }

            var rawMap = await _subspecialityRepository.GetCoordinatorsBySubspecialityAndInstitutionAsync();

            // Convert tuple keys to string keys like "subspecialityId-institutionId"
            CoordinatorsBySubspecialityAndInstitution = rawMap.ToDictionary(
                kvp => $"{kvp.Key.SubSpecialityId}-{kvp.Key.InstitutionId}",
                kvp => kvp.Value
            );
        }

        public string LoadTemplateAndInsertValues(string url)
        {
            var templatePath = Path.Combine(_environment.WebRootPath, "Templates", "EmailConfirmation.html");
            var emailTemplate = System.IO.File.ReadAllText(templatePath);
            return emailTemplate
                .Replace("{{confirmationLink}}", url);
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.Name = Input.Name;
                user.Surname = Input.Surname;
                user.IdNumber = Input.IdNumber;
                user.InstitutionId = Input.InstitutionId;
                user.StartDate = Input.StartDate;
                user.PhoneNumber = Input.PhoneNumber;
                user.SubSpecialityId = Input.SubspecialityId;
                user.DateJoined = DateTime.Now; //Input.DateJoined ?? default;
                user.HPCSANumber = Input.HPCSANumber;
                user.ApprovalStatus = eApprovalStatus.Pending;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, Roles.PendingTrainee);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string htmlContent = LoadTemplateAndInsertValues(callbackUrl);

                    await _emailSender.SendEmailAsync(Input.Email, "Wombat Email Confirmation", htmlContent);

                    //await _emailSender.SendEmailAsync(Input.Email, "Wombat Email Confirmation",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private WombatUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<WombatUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(WombatUser)}'. " +
                    $"Ensure that '{nameof(WombatUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<WombatUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<WombatUser>)_userStore;
        }
    }
}
