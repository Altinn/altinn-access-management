using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Tests.Mocks;

    /// <summary>
    /// Class for mocking http responses when testing
    /// </summary>
public class MessageHandlerMock : DelegatingHandler
{
    private readonly HttpStatusCode _expectedResponseStatus;
    private readonly HttpContent _httpContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandlerMock"/> class.
    /// </summary>
    /// <param name="expectedResponseStatus">Expected HttpStatusCode</param>
    /// <param name="httpContent">Expected response content</param>
    public MessageHandlerMock(HttpStatusCode expectedResponseStatus, HttpContent httpContent)
    {
        _expectedResponseStatus = expectedResponseStatus;
        _httpContent = httpContent;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage()
            { StatusCode = _expectedResponseStatus, Content = _httpContent });
    }
}
