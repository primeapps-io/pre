﻿using Hangfire.Common;
using Hangfire.States;
using System;
using System.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace PrimeApps.App.Jobs.QueueAttributes
{
    public class CommonQueueAttribute : JobFilterAttribute, IElectStateFilter
    {
        public String Queue { get; }

        public CommonQueueAttribute()
        {
            Queue = ConfigurationManager.AppSettings["HangfireCommonQueue"];
        }
        public void OnStateElection(ElectStateContext context)
        {
            var enqueuedState = context.CandidateState as EnqueuedState;
            if (enqueuedState != null)
            {
                enqueuedState.Queue = Queue;
            }
        }
    }
}