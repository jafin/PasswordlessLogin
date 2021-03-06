﻿// Copyright (c) Ryan Foster. All rights reserved. 
// Licensed under the Apache License, Version 2.0.

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SimpleIAM.PasswordlessLogin.Extensions;
using SimpleIAM.PasswordlessLogin.Orchestrators;

namespace SimpleIAM.PasswordlessLogin.API
{
    [Route("passwordless-api/v1")]
    [EnableCors(PasswordlessLoginConstants.Security.CorsPolicyName)]
    public class AuthenticateApiController : Controller
    {
        private readonly AuthenticateOrchestrator _authenticateOrchestrator;

        public AuthenticateApiController(
            AuthenticateOrchestrator authenticateOrchestrator
            )
        {
            _authenticateOrchestrator = authenticateOrchestrator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterInputModel model)
        {
            if (ModelState.IsValid)
            {
                return (await _authenticateOrchestrator.RegisterAsync(model)).ToJsonResult();
            }
            return new ActionResponse(ModelState).ToJsonResult();
        }

        [HttpPost("send-one-time-code")]
        public async Task<IActionResult> SendOneTimeCode([FromBody] SendCodeInputModel model)
        {
            if (ModelState.IsValid)
            {
                return (await _authenticateOrchestrator.SendOneTimeCodeAsync(model)).ToJsonResult();
            }
            return new ActionResponse(ModelState).ToJsonResult();
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticatePasswordInputModel model)
        {
            if (ModelState.IsValid)
            {
                var status = await _authenticateOrchestrator.AuthenticateAsync(model);
                if(status.StatusCode == HttpStatusCode.Redirect)
                {
                    return NextUrlJsonResult(status.RedirectUrl);
                }
                return status.ToJsonResult();
            }
            return new ActionResponse(ModelState).ToJsonResult();
        }

        [HttpPost("authenticate-one-time-code")]
        public async Task<IActionResult> AuthenticateCode([FromBody] AuthenticateInputModel model)
        {
            if (ModelState.IsValid)
            {
                var status = await _authenticateOrchestrator.AuthenticateCodeAsync(model);
                if (status.StatusCode == HttpStatusCode.Redirect)
                {
                    return NextUrlJsonResult(status.RedirectUrl); 
                }
                return status.ToJsonResult();
            }
            return new ActionResponse(ModelState).ToJsonResult();
        }

        [HttpPost("authenticate-long-code")]
        public async Task<IActionResult> AuthenticateLongCode([FromBody] AuthenticateLongCodeInputModel model)
        {
            if (ModelState.IsValid)
            {
                var status = await _authenticateOrchestrator.AuthenticateLongCodeAsync(model.LongCode);
                if (status.StatusCode == HttpStatusCode.Redirect)
                {
                    return NextUrlJsonResult(status.RedirectUrl);
                }
                return status.ToJsonResult();
            }
            return new ActionResponse(ModelState).ToJsonResult();
        }

        [HttpPost("authenticate-password")]
        public async Task<IActionResult> AuthenticatePassword([FromBody] AuthenticatePasswordInputModel model)
        {
            if (ModelState.IsValid)
            {
                var status = await _authenticateOrchestrator.AuthenticatePasswordAsync(model);
                if (status.StatusCode == HttpStatusCode.Redirect)
                {
                    return NextUrlJsonResult(status.RedirectUrl);
                }
                return status.ToJsonResult();
            }
            return new ActionResponse(ModelState).ToJsonResult();
        }

        [HttpPost("send-password-reset-message")]
        public async Task<IActionResult> SendPasswordResetMessage([FromBody]SendPasswordResetMessageInputModel model)
        {
            if (ModelState.IsValid)
            {
                return (await _authenticateOrchestrator.SendPasswordResetMessageAsync(model)).ToJsonResult();
            }
            return new ActionResponse(ModelState).ToJsonResult();
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            return (await _authenticateOrchestrator.SignOutAsync()).ToJsonResult();
        }

        private IActionResult NextUrlJsonResult(string nextUrl)
        {
            return new JsonResult(new
            {
                Message = (string)null,
                NextUrl = nextUrl
            });
        }
    }
}