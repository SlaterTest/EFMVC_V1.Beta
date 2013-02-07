﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using EFMVC.Web.ViewModels;
using EFMVC.Domain.Commands;
using EFMVC.Web.Core.Models;
using EFMVC.CommandProcessor.Dispatcher;
using EFMVC.Data.Repositories;
using EFMVC.Core.Common;
using EFMVC.Web.Core.Extensions;
using EFMVC.Web.Core.ActionFilters;
using EFMVC.Web.Core.Authentication;
using EFMVC.Model;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace EFMVC.Web.Controllers
{

    public class AccountController : Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IUserRepository userRepository;
        private readonly IFormsAuthentication formAuthentication;
    
        public AccountController(ICommandBus commandBus, IUserRepository userRepository, IFormsAuthentication formAuthentication)
        {
            this.commandBus = commandBus;
            this.userRepository = userRepository;
            this.formAuthentication = formAuthentication;
        }

        public ActionResult Index()
        {
            var userlist = userRepository.GetAll();
            return View(userlist);
        } 


        [CompressResponse]
        [EFMVCAuthorize(Roles.Admin)]
        public ActionResult UserList()
        {
            return View();
        }


        [CompressResponse]
        [EFMVCAuthorize(Roles.Admin)]
         public ActionResult UserList_Read([DataSourceRequest] DataSourceRequest request)
          {
              var userlist = userRepository.GetAll() ;
              return Json(userlist.ToDataSourceResult(request));
          }

        /*
        public ActionResult UserList_Read()
        {
            var userlist = userRepository.GetAll();
            return View(userlist);
        }
        */


        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            formAuthentication.Signout();
            return RedirectToAction("Index", "Home");
        }

        [CompressResponse]
        //[EFMVCAuthorize(Roles.Admin)]
        public ActionResult Register()
        {
            return ContextDependentView();
        }    
     
        private bool ValidatePassword(User user, string password)
        {
            var encoded = Md5Encrypt.Md5EncryptPassword(password);
            return user.PasswordHash.Equals(encoded);
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            return ContextDependentView();
        }
        [HttpPost]
        public ActionResult Login(LogOnFormModel form, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                User user = userRepository.Get(u => u.Email == form.UserName && u.Activated == true);
                if (user != null)
                {
                    if (ValidatePassword(user, form.Password))
                    {
                        formAuthentication.SetAuthCookie(this.HttpContext,
                                                                 UserAuthenticationTicketBuilder.CreateAuthenticationTicket(
                                                                     user));

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }


                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }
        [HttpPost]
        public JsonResult JsonLogin(LogOnFormModel form , string returnUrl)
        {
            if (ModelState.IsValid)
            {
                User user = userRepository.Get(u => u.Email == form.UserName && u.Activated == true);
                if (user != null)
                {
                    if (ValidatePassword(user, form.Password))
                    {
                        formAuthentication.SetAuthCookie(this.HttpContext,
                                                                 UserAuthenticationTicketBuilder.CreateAuthenticationTicket(
                                                                     user));

                        return Json(new { success = true, redirect = returnUrl });


                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }
        //
        // POST: /Account/JsonRegister


        [HttpPost]
        [CompressResponse]
        //[EFMVCAuthorize(Roles.Admin)]
        public ActionResult JsonRegister(UserFormModel form)
        {
            if (ModelState.IsValid)
            {
                var command = new UserRegisterCommand
                {
                    FirstName = form.FirstName,
                    LastName = form.LastName,
                    Email = form.Email,
                    Password = form.Password,
                    Activated = true,
                    RoleId = (Int32)form.Role
                    //RoleId = (Int32)UserRoles.User
                };
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    var result = commandBus.Submit(command);
                    if (result.Success)
                    {
                        User user = userRepository.Get(u => u.Email == form.Email);
                        formAuthentication.SetAuthCookie(this.HttpContext,
                                                          UserAuthenticationTicketBuilder.CreateAuthenticationTicket(
                                                              user));                      
                        return Json(new { success = true });                       
                    }
                    else
                    {
                        ModelState.AddModelError("", "An unknown error occurred.");
                    }
                }
                // If we got this far, something failed
                return Json(new { errors = GetErrorsFromModelState() });
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }


        [HttpPost]
        public ActionResult Register(UserFormModel form)
        {
            if (ModelState.IsValid)
            {
                var command = new UserRegisterCommand
                {
                    FirstName = form.FirstName,
                    LastName = form.LastName,
                    Email = form.Email,
                    Password = form.Password,
                    Activated = true,
                   // RoleId = (Int32)UserRoles.User
                    RoleId = (Int32)form.Role
                };
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    var result = commandBus.Submit(command);
                    if (result.Success)
                    {
                        User user = userRepository.Get(u => u.Email == form.Email);
                        formAuthentication.SetAuthCookie(this.HttpContext,
                                                          UserAuthenticationTicketBuilder.CreateAuthenticationTicket(
                                                              user));
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An unknown error occurred.");
                    }
                }
                // If we got this far, something failed, redisplay form
                return View(form);
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }
        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
        private ActionResult ContextDependentView()
        {
            string actionName = ControllerContext.RouteData.GetRequiredString("action");
            if (Request.QueryString["content"] != null)
            {
                ViewBag.FormAction = "Json" + actionName;
                return PartialView();
            }
            else
            {
                ViewBag.FormAction = actionName;
                return View();
            }
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordFormModel form)
        {
            if (ModelState.IsValid)
            {
                EFMVCUser efmvcUser = HttpContext.User.GetEFMVCUser();
                var command = new ChangePasswordCommand
                {
                    UserId = efmvcUser.UserId,
                    OldPassword=form.OldPassword,
                    NewPassword = form.NewPassword
                };
                  IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    var result = commandBus.Submit(command);
                    if (result.Success)
                    {
                        return RedirectToAction("ChangePasswordSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }     
            // If we got this far, something failed, redisplay form
            return View(form);
        }
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
    }
}
