﻿
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PrimeApps.App.Results
{
	public class ChallengeResult : IActionResult
    {
        public ChallengeResult(string loginProvider, Controller controller)
        {
            LoginProvider = loginProvider;
            Request = controller.Request;
        }

        public string LoginProvider { get; set; }
        public HttpRequest Request { get; set; }
		
		//TODO Removed
        /*public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            
            Request.GetOwinContext().Authentication.Challenge(LoginProvider);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.RequestMessage = Request;
            return Task.FromResult(response);
        }*/

        public Task ExecuteResultAsync(ActionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
