﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Resources;
using System.Collections.Generic;

namespace PrimeApps.App.Jobs.Email
{
    public class Notification
    {
        /// <summary>
        /// Sends a reminder email for a task to a specified user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="taskSubject"></param>
        /// <param name="emailAddress"></param>
        /// <param name="culture"></param>
        public static void Task(string userName, string taskSubject, string emailAddress, string culture, string deadline, int appId, UserItem appUser, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            /// create email mesage with its parameters.
            Dictionary<string, string> emailData = new Dictionary<string, string>();
            emailData.Add("UserName", userName);
            emailData.Add("Description", taskSubject);
            emailData.Add("Deadline", deadline);

            /// send the email.
            Helpers.Email email = new Helpers.Email(EmailResource.TaskReminder, culture, emailData, configuration, serviceScopeFactory, appId, appUser);
            email.AddRecipient(emailAddress);
            email.AddToQueue(appUser: appUser);
        }

        /// <summary>
        /// Sends a reminder email for an event to a specified user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="callSubject"></param>
        /// <param name="emailAddress"></param>
        /// <param name="culture"></param>
        public static void Event(string userName, string eventSubject, string emailAddress, string culture, string startDate, string endDate, int appId, UserItem appUser, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            /// create email mesage with its parameters.
            Dictionary<string, string> emailData = new Dictionary<string, string>();
            emailData.Add("UserName", userName);
            emailData.Add("Description", eventSubject);
            emailData.Add("StartDate", startDate);
            emailData.Add("EndDate", endDate);

            /// send the email.
            Helpers.Email email = new Helpers.Email(EmailResource.EventReminder, culture, emailData, configuration, serviceScopeFactory, appId, appUser);
            email.AddRecipient(emailAddress);
            email.AddToQueue(appUser: appUser);
        }

        /// <summary>
        /// Sends a reminder email for a call to a specified user.
        /// </summary>
        public static void Call(string userName, string callSubject, string emailAddress, string culture, string startDate, int appId, UserItem appUser, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            /// create email mesage with its parameters.
            Dictionary<string, string> emailData = new Dictionary<string, string>();
            emailData.Add("UserName", userName);
            emailData.Add("Description", callSubject);
            emailData.Add("StartDate", startDate);

            /// send the email.
            Helpers.Email email = new Helpers.Email(EmailResource.CallReminder, culture, emailData, configuration, serviceScopeFactory, appId, appUser);
            email.AddRecipient(emailAddress);
            email.AddToQueue(appUser: appUser);
        }
    }
}