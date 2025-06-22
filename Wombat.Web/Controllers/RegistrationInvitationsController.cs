/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;
using Wombat.Services;

namespace Wombat.Web.Controllers
{
    [Authorize]
    public class RegistrationInvitationsController : Controller
    {
        public UserManager<WombatUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public SignInManager<WombatUser> SignInManager { get; }
        public IRegistrationInvitationRepository RegistrationInvitationRepository { get; }
        public ISubSpecialityRepository SubSpecialityRepository { get; }
        public ISpecialityRepository SpecialityRepository { get; }
        public IInstitutionRepository InstitutionRepository { get; }
        public IWebHostEnvironment Environment { get; }
        public IEmailSender EmailSender { get; }
        public IMapper Mapper { get; }

        public RegistrationInvitationsController( UserManager<WombatUser> userManager,
                                                  RoleManager<IdentityRole> roleManager, 
                                                  SignInManager<WombatUser> signInManager,
                                                  IRegistrationInvitationRepository registrationInvitationRepository,
                                                  ISubSpecialityRepository subSpecialityRepository,
                                                  ISpecialityRepository specialityRepository,
                                                  IInstitutionRepository institutionRepository,
                                                  IWebHostEnvironment environment,
                                                  IEmailSender emailSender,
                                                  IMapper mapper )
        {
            UserManager = userManager;
            RoleManager = roleManager;
            SignInManager = signInManager;
            RegistrationInvitationRepository = registrationInvitationRepository;
            SubSpecialityRepository = subSpecialityRepository;
            SpecialityRepository = specialityRepository;
            InstitutionRepository = institutionRepository;
            Environment = environment;
            EmailSender = emailSender;
            Mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Complete(string token)
        {
            var invitation = await RegistrationInvitationRepository.GetByTokenAsync(token);
            if (invitation == null)
                return BadRequest("This invitation is invalid or has expired.");

            var model = Mapper.Map<RegisterFromInviteVM>(invitation);
            model.SpecialityName = invitation?.Speciality?.Name ?? "";
            model.SubSpecialityName = invitation?.SubSpeciality?.Name ?? "";
            model.InstitutionName = invitation?.Institution?.Name ?? "";

            return View("RegisterFromInvite", model);
        }

        [HttpGet]
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Details(int id)
        {
            var invitation = await RegistrationInvitationRepository.GetAsync(id);
            if (invitation == null)
                return NotFound();

            var vm = Mapper.Map<RegistrationInvitationVM>(invitation);
            return View(vm);
        }


        [HttpGet]
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Invite()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var currentUserRoles = await UserManager.GetRolesAsync(currentUser);

            var allowedRoles = RoleHelper.DisplayRoles
                .Where(r => currentUserRoles.Any(cur => RoleHierarchy.CanAssign(cur, r.ToStringValue())))
                .ToList();

            var model = new InviteUserVM
            {
                AvailableRoles = allowedRoles
                    .Select(r => new SelectListItem
                    {
                        Text = r.GetDisplayName(),
                        Value = r.ToStringValue()
                    }).ToList(),
                Specialities = (await SpecialityRepository.GetAllAsync())
                    .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList(),
                SubSpecialities = (await SubSpecialityRepository.GetAllAsync())
                    .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList()
            };

            model.AllSubSpecialities = (await SubSpecialityRepository.GetAllAsync())
                .Select(s => new SubSpecialityOption
                {
                    Id = s.Id,
                    Name = s.Name,
                    SpecialityId = s.SpecialityId
                }).ToList();

            model.Institutions = (await InstitutionRepository.GetAllAsync())
                .Select(i => new SelectListItem { Text = i.Name, Value = i.Id.ToString() })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Invite(InviteUserVM model)
        {
            var roles = model.Roles ?? new List<string>();
            bool isOnlyAdmin = roles.Count == 1 && roles[0] == Role.Administrator.ToStringValue();

            if (!isOnlyAdmin && model.InstitutionId == null)
            {
                ModelState.AddModelError(nameof(model.InstitutionId), "Institution is required unless the only role is Administrator.");
            }

            var currentUser = await UserManager.GetUserAsync(User);
            var currentUserRoles = await UserManager.GetRolesAsync(currentUser);
            var disallowed = model.Roles.Except(RoleHelper.DisplayRoles
                .Where(r => currentUserRoles.Any(cur => RoleHierarchy.CanAssign(cur, r.ToStringValue())))
                .Select(r => r.ToStringValue())).ToList();

            if (disallowed.Any())
            {
                ModelState.AddModelError(nameof(model.Roles), "You cannot assign one or more of the selected roles.");
            }

            if (!ModelState.IsValid)
            {
                // reload dropdowns
                model.AvailableRoles = RoleHelper.DisplayRoles
                    .Select(r => new SelectListItem
                    {
                        Text = r.GetDisplayName(),
                        Value = r.ToStringValue(),
                        Selected = model.Roles.Contains(r.ToStringValue())
                    })
                    .ToList();
                model.Specialities = (await SpecialityRepository.GetAllAsync()).Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                model.SubSpecialities = (await SubSpecialityRepository.GetAllAsync()).Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();

                model.AllSubSpecialities = (await SubSpecialityRepository.GetAllAsync())
                    .Select(s => new SubSpecialityOption
                    {
                        Id = s.Id,
                        Name = s.Name,
                        SpecialityId = s.SpecialityId
                    }).ToList();

                model.Institutions = (await InstitutionRepository.GetAllAsync())
                    .Select(i => new SelectListItem { Text = i.Name, Value = i.Id.ToString() })
                    .ToList();
                return View(model);
            }

            var token = Guid.NewGuid().ToString();

            var invitation = new RegistrationInvitation
            {
                Email = model.Email,
                Roles = string.Join(",", model.Roles),
                InstitutionId = model.InstitutionId,
                SpecialityId = model.SpecialityId,
                SubSpecialityId = model.SubSpecialityId,
                ExpiryDate = model.ExpiryDate,
                IsUsed = false,
                Token = token,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            await RegistrationInvitationRepository.AddAsync(invitation);

            var url = Url.Action("Complete", "RegistrationInvitations", new { token }, Request.Scheme);

            string htmlContent = LoadTemplateAndInsertValues(invitation, url);

            await EmailSender.SendEmailAsync(model.Email, "Wombat Registration", htmlContent);

            TempData["Success"] = "Invitation sent.";
            return RedirectToAction("Invite");
        }

        public string LoadTemplateAndInsertValues(RegistrationInvitation invitation, string url)
        {
            var templatePath = Path.Combine(Environment.WebRootPath, "Templates", "RegistrationInvitation.html");
            var emailTemplate = System.IO.File.ReadAllText(templatePath);
            return emailTemplate
                .Replace("{{registrationLink}}", url)
                .Replace("{{expiryDate}}", invitation.ExpiryDate.ToString("yyyy-MM-dd"));
        }

        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Delete(int id)
        {
            await RegistrationInvitationRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Resend(int id)
        {
            var invitation = await RegistrationInvitationRepository.GetAsync(id);
            if (invitation == null || invitation.IsUsed) return NotFound();

            var url = Url.Action("Complete", "RegistrationInvitations", new { token = invitation.Token }, Request.Scheme);

            string htmlContent = LoadTemplateAndInsertValues(invitation, url);

            await EmailSender.SendEmailAsync(invitation.Email, "Wombat Registration Reminder", htmlContent);
            TempData["Success"] = $"Invitation resent to {invitation.Email}.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Index()
        {
            var currentUser = await UserManager.GetUserAsync(User);
            var roles = await UserManager.GetRolesAsync(currentUser);

            var invitations = await RegistrationInvitationRepository.GetAllAsync();

            // Apply filtering based on role hierarchy
            if (roles.Contains(Role.Administrator.ToStringValue()))
            {
                // No filter needed
            }
            else if (roles.Contains(Role.InstitutionalAdmin.ToStringValue()))
            {
                invitations = invitations
                    .Where(i => i.InstitutionId == currentUser.InstitutionId)
                    .ToList();
            }
            else if (roles.Contains(Role.SpecialityAdmin.ToStringValue()))
            {
                var userSpecialityId = currentUser.SubSpeciality?.SpecialityId;
                invitations = invitations
                    .Where(i => i.InstitutionId == currentUser.InstitutionId &&
                                i.SpecialityId == userSpecialityId)
                    .ToList();
            }
            else if (roles.Contains(Role.SubSpecialityAdmin.ToStringValue()))
            {
                invitations = invitations
                    .Where(i => i.InstitutionId == currentUser.InstitutionId &&
                                i.SubSpecialityId == currentUser.SubSpecialityId)
                    .ToList();
            }
            else
            {
                return Forbid(); // No access
            }

            var result = invitations.Select(inv => new RegistrationInvitationVM
            {
                Id = inv.Id,
                Email = inv.Email,
                Roles = inv.Roles,
                Institution = inv.Institution?.Name ?? "",
                Speciality = inv.Speciality?.Name,
                SubSpeciality = inv.SubSpeciality?.Name,
                ExpiryDate = inv.ExpiryDate,
                IsUsed = inv.IsUsed
            }).ToList();

            return View(result);
        }


        private string GetFieldForIdentityError(string errorCode)
        {
            return errorCode switch
            {
                "PasswordTooShort" => "Password",
                "PasswordRequiresNonAlphanumeric" => "Password",
                "PasswordRequiresDigit" => "Password",
                "PasswordRequiresLower" => "Password",
                "PasswordRequiresUpper" => "Password",
                "PasswordMismatch" => "ConfirmPassword",
                "DuplicateUserName" => "Email",
                "InvalidEmail" => "Email",
                _ => string.Empty // fallback to summary
            };
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(RegisterFromInviteVM model, string token)
        {
            if (!ModelState.IsValid)
                return View("RegisterFromInvite", model);

            var invitation = await RegistrationInvitationRepository.GetByTokenAsync(token);
            if (invitation == null)
                return BadRequest("Invalid or expired invitation.");

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View("RegisterFromInvite", model);
            }

            var user = new WombatUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.FirstName,
                Surname = model.LastName,
                DateJoined = DateTime.UtcNow,
                InstitutionId =invitation.InstitutionId,
                SubSpecialityId = invitation.SubSpecialityId,
                EmailConfirmed = true, // Assuming email is confirmed by invitation
                ApprovalStatus = WombatUser.eApprovalStatus.Approved // Automatically approve from invite
            };

            var result = await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                foreach (var role in model.Roles)
                {
                    await UserManager.AddToRoleAsync(user, role);
                }

                await RegistrationInvitationRepository.MarkAsUsedAsync(token);

                await SignInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                var field = GetFieldForIdentityError(error.Code);
                ModelState.AddModelError(field, error.Description);
            }

            if (!ModelState.IsValid)
            {
                // Rehydrate these values if lost
                model.Token = token; // if not already bound
                model.Roles = invitation?.Roles?.Split(',').ToList() ?? new();
                model.SpecialityName = invitation?.Speciality?.Name ?? "";
                model.SubSpecialityName = invitation?.SubSpeciality?.Name ?? "";
                
                model.SpecialityName = invitation?.Speciality?.Name ?? "";
                model.SubSpecialityName = invitation?.SubSpeciality?.Name ?? "";
            }
            
            return View("RegisterFromInvite", model);
            
        }

    }
}
