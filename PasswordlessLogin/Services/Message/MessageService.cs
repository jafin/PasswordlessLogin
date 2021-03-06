﻿// Copyright (c) Ryan Foster. All rights reserved. 
// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.Logging;
using SimpleIAM.PasswordlessLogin.Configuration;
using SimpleIAM.PasswordlessLogin.Helpers;
using SimpleIAM.PasswordlessLogin.Services.Email;
using SimpleIAM.PasswordlessLogin.Services.Localization;
using StandardResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIAM.PasswordlessLogin.Services.Message
{
    public class MessageService : IMessageService
    {
        private readonly IApplicationLocalizer _localizer;
        private readonly ILogger _logger;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUrlService _urlService;
        private readonly PasswordlessLoginOptions _passwordlessLoginOptions;
        private readonly IApplicationService _applicationService;

        public MessageService(
            IApplicationLocalizer localizer,
            ILogger<MessageService> logger,
            IEmailTemplateService emailTemplateService,
            IUrlService urlService,
            PasswordlessLoginOptions passwordlessLoginOptions,
            IApplicationService applicationService
            )
        {
            _localizer = localizer;
            _logger = logger;
            _emailTemplateService = emailTemplateService;
            _urlService = urlService;
            _passwordlessLoginOptions = passwordlessLoginOptions;
            _applicationService = applicationService;
        }

        public async Task<Status> SendAccountAlreadyExistsMessageAsync(string applicationId, string sendTo)
        {
            _logger.LogDebug("Sending account already exists message");
            if (!IsValidEmailAddress(sendTo))
            {
                return NotAnEmailAddress();
            }

            var link = _urlService.GetForgotPasswordUrl(true);
            var fields = GetCustomFields(applicationId);
            fields["forgot_password_link"] = link;
            return await _emailTemplateService.SendEmailAsync(PasswordlessLoginConstants.EmailTemplates.AccountAlreadyExists, sendTo, fields);
        }

        public async Task<Status> SendAccountNotFoundMessageAsync(string applicationId, string sendTo)
        {
            _logger.LogDebug("Sending account not found message");
            if (!IsValidEmailAddress(sendTo)) {
                return NotAnEmailAddress();
            }

            var link = _urlService.GetRegisterUrl(true);
            var fields = GetCustomFields(applicationId);
            fields["register_link"] = link;
            return await _emailTemplateService.SendEmailAsync(PasswordlessLoginConstants.EmailTemplates.AccountNotFound, sendTo, fields);
        }

        public async Task<Status> SendPasswordChangedNoticeAsync(string applicationId, string sendTo)
        {
            _logger.LogDebug("Sending password changed notice");
            if (!IsValidEmailAddress(sendTo))
            {
                return NotAnEmailAddress();
            }

            var fields = GetCustomFields(applicationId);
            return await _emailTemplateService.SendEmailAsync(PasswordlessLoginConstants.EmailTemplates.PasswordChangedNotice, sendTo, fields);
        }

        public async Task<Status> SendPasswordRemovedNoticeAsync(string applicationId, string sendTo)
        {
            _logger.LogDebug("Sending password removed notice");
            if (!IsValidEmailAddress(sendTo))
            {
                return NotAnEmailAddress();
            }

            var fields = GetCustomFields(applicationId);
            return await _emailTemplateService.SendEmailAsync(PasswordlessLoginConstants.EmailTemplates.PasswordRemovedNotice, sendTo, fields);
        }

        public async Task<Status> SendEmailChangedNoticeAsync(string applicationId, string sendTo, string longCode)
        {
            if (!IsValidEmailAddress(sendTo))
            {
                return NotAnEmailAddress();
            }

            var link = _urlService.GetCancelChangeLinkUrl(longCode, true);
            var fields = GetCustomFields(applicationId);
            fields["old_email_address"] = sendTo;
            fields["link_validity_hours"] = _passwordlessLoginOptions.CancelEmailChangeTimeWindowHours.ToString();
            fields["cancel_email_change_link"] = link;
            return await _emailTemplateService.SendEmailAsync(PasswordlessLoginConstants.EmailTemplates.EmailChangedNotice, sendTo, fields);
        }

        public async Task<Status> SendOneTimeCodeMessageAsync(string applicationId, string sendTo, string oneTimeCode)
        {
            _logger.LogDebug("Sending one time code message");
            return await SendOneTimeCodeMessageInternalAsync(PasswordlessLoginConstants.EmailTemplates.OneTimeCode, applicationId, sendTo, oneTimeCode, "");
        }

        public async Task<Status> SendOneTimeCodeAndLinkMessageAsync(string applicationId, string sendTo, string oneTimeCode, string longCode)
        {
            _logger.LogDebug("Sending one time code and link message");
            return await SendOneTimeCodeMessageInternalAsync(PasswordlessLoginConstants.EmailTemplates.SignInWithEmail, applicationId, sendTo, oneTimeCode, longCode);
        }

        private async Task<Status> SendOneTimeCodeMessageInternalAsync(string template, string clientId, string sendTo, string oneTimeCode, string longCode)
        {
            if (!IsValidEmailAddress(sendTo))
            {
                return NotAnEmailAddress();
            }

            var link = _urlService.GetSignInLinkUrl(longCode.ToString(), true);
            var fields = GetCustomFields(clientId);
            fields["one_time_code"] = oneTimeCode;
            fields["sign_in_link"] = link;
            fields["long_code"] = longCode.ToString();
            return await _emailTemplateService.SendEmailAsync(template, sendTo, fields);
        }

        private bool IsValidEmailAddress(string sendTo)
        {
            return EmailAddressChecker.EmailIsValid(sendTo);
        }

        private Status NotAnEmailAddress()
        {
            return Status.Error(_localizer["Could not deliver message. Email address is not valid."]); // non-email addresses not implemented
        }

        public async Task<Status> SendWelcomeMessageAsync(string applicationId, string sendTo, string oneTimeCode, string longCode, IDictionary<string, string> userFields)
        {
            _logger.LogDebug("Sending welcome message");
            if (!IsValidEmailAddress(sendTo))
            {
                return NotAnEmailAddress();
            }

            var link = _urlService.GetSignInLinkUrl(longCode.ToString(), true);
            var signInUrl = _urlService.GetSignInUrl(true);
            var fields = GetCustomFields(applicationId);
            if(userFields != null)
            {
                foreach (var field in userFields)
                {
                    if (!fields.ContainsKey(field.Key))
                    {
                        fields[field.Key] = field.Value;
                    }
                }
            }
            fields["one_time_code"] = oneTimeCode;
            fields["sign_in_link"] = link;
            fields["sign_in_url"] = signInUrl;
            fields["long_code"] = longCode.ToString();

            return await _emailTemplateService.SendEmailAsync(PasswordlessLoginConstants.EmailTemplates.Welcome, sendTo, fields);
        }

        public async Task<Status> SendPasswordResetMessageAsync(string applicationId, string sendTo, string oneTimeCode, string longCode)
        {
            _logger.LogDebug("Sending password reset message");
            if (!IsValidEmailAddress(sendTo))
            {
                return NotAnEmailAddress();
            }

            var link = _urlService.GetSignInLinkUrl(longCode.ToString(), true);
            var signInUrl = _urlService.GetSignInUrl(true);
            var fields = GetCustomFields(applicationId);
            fields["one_time_code"] = oneTimeCode;
            fields["password_reset_link"] = link;
            fields["long_code"] = longCode.ToString();

            return await _emailTemplateService.SendEmailAsync(PasswordlessLoginConstants.EmailTemplates.PasswordReset, sendTo, fields);
        }

        protected IDictionary<string, string> GetCustomFields(string applicationId)
        {
            var fields = new Dictionary<string, string>(_passwordlessLoginOptions.CustomProperties ?? new Dictionary<string, string>() { });
            if (applicationId != null)
            {
                var clientProperties = _applicationService.GetApplicationProperties(applicationId);
                if (clientProperties != null)
                {
                    foreach(var field in clientProperties)
                    {
                        fields[field.Key] = field.Value;
                    }
                }
            }
            return fields;
        }
    }
}
