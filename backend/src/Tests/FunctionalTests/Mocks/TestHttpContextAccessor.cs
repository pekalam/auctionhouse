using Microsoft.AspNetCore.Http;
using System;

namespace FunctionalTests.Mocks
{
    public class TestHttpContextAccessor : IHttpContextAccessor
    {
        private static readonly TestSession _testSession = new TestSession();

        public HttpContext? HttpContext
        {
            get => new DefaultHttpContext()
            {
                Session = _testSession
            }; set => throw new NotImplementedException();
        }
    }
}