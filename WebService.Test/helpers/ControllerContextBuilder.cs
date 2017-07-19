// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace WebService.Test.helpers
{
    static class ControllerContextBuilder
    {
        public static ControllerContext Build(byte[] body, string etag)
        {
            var request = new Mock<HttpRequest>();
            var context = new Mock<HttpContext>();
            context.SetupGet(x => x.Request).Returns(request.Object);

            var response = new Mock<HttpResponse>();
            response.SetupGet(x => x.Headers).Returns(new HeaderDictionary());
            response.SetupGet(x => x.Body).Returns(new MemoryStream());
            context.SetupGet(x => x.Response).Returns(response.Object);

            if (body != null)
            {
                request.SetupGet(x => x.Body).Returns(new MemoryStream(body));
            }

            if (etag != null)
            {
                var headers = new HeaderDictionary();
                headers.Add("ETag", etag);
                request.SetupGet(x => x.Headers).Returns(headers);
            }

            return new ControllerContext
            {
                HttpContext = context.Object
            };
        }
    }
}
