using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi
{
    public class MyControllerBase : ControllerBase
    {
        public MyControllerBase() : base()
        {
        }
        protected InternalServerErrorObjectResult InternalServerError()
        {
            return new InternalServerErrorObjectResult();
        }

        protected InternalServerErrorObjectResult InternalServerError(object value)
        {
            var env = (IWebHostEnvironment)HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            if (env.IsProduction())
            {
                return new InternalServerErrorObjectResult("InternalServerError");
            }
            return new InternalServerErrorObjectResult(value);
        }
    }
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }

        public InternalServerErrorObjectResult() : this(null)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
