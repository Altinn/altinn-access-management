using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Controllers;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IPolicyRetrievalPoint"></see> interface
    /// </summary>
    public class PolicyRetrievalPointMock : IPolicyRetrievalPoint
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<PolicyRetrievalPointMock> _logger;

        private readonly string _orgAttributeId = "urn:altinn:org";

        private readonly string _appAttributeId = "urn:altinn:app";

        /// <summary>
        /// Constructor setting up dependencies
        /// </summary>
        /// <param name="httpContextAccessor">httpContextAccessor</param>
        /// <param name="logger">logger</param>
        public PolicyRetrievalPointMock(IHttpContextAccessor httpContextAccessor, ILogger<PolicyRetrievalPointMock> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public PolicyRetrievalPointMock()
        {
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(XacmlContextRequest request, CancellationToken cancellationToken = default)
        {
            string testID = GetTestId(_httpContextAccessor.HttpContext);
            if (!string.IsNullOrEmpty(testID) && testID.ToLower().Contains("altinnapps"))
            {
                if (File.Exists(Path.Combine(GetPolicyPath(request), "policy.xml")))
                {
                    return await Task.FromResult(ParsePolicy("policy.xml", GetPolicyPath(request)));
                }

                return await Task.FromResult(ParsePolicy(testID + "Policy.xml", GetAltinnAppsPath()));
            }
            else
            {
                return await Task.FromResult(ParsePolicy(testID + "Policy.xml", GetConformancePath()));
            }
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(string org, string app, CancellationToken cancellationToken = default)
        {
            if (File.Exists(Path.Combine(GetAltinnAppsPolicyPath(org, app), "policy.xml")))
            {
                return await Task.FromResult(ParsePolicy("policy.xml", GetAltinnAppsPolicyPath(org, app)));
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(string resourceRegistry, CancellationToken cancellationToken = default)
        {
            if (File.Exists(Path.Combine(GetAltinnResourcePolicyPath(resourceRegistry), "resourcepolicy.xml")))
            {
                return await Task.FromResult(ParsePolicy("resourcepolicy.xml", GetAltinnResourcePolicyPath(resourceRegistry)));
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyVersionAsync(string policyPath, string version, CancellationToken cancellationToken = default)
        {
            string path = GetAltinnAppsDelegationPolicyPath(policyPath);
            if (File.Exists(path))
            {
                return await Task.FromResult(ParsePolicy(string.Empty, path));
            }

            _logger.LogWarning("Policy Version did not found policy " + path);

            return null;
        }

        private static XacmlPolicy ParsePolicy(string policyDocumentTitle, string policyPath)
        {
            XmlDocument policyDocument = new XmlDocument();

            policyDocument.Load(Path.Combine(policyPath, policyDocumentTitle));
            XacmlPolicy policy;
            using (XmlReader reader = XmlReader.Create(new StringReader(policyDocument.OuterXml)))
            {
                policy = XacmlParser.ParseXacmlPolicy(reader);
            }

            return policy;
        }

        private string GetPolicyPath(XacmlContextRequest request)
        {
            string org = string.Empty;
            string app = string.Empty;
            foreach (XacmlContextAttributes attr in request.Attributes)
            {
                if (attr.Category.OriginalString.Equals(XacmlConstants.MatchAttributeCategory.Resource))
                {
                    foreach (XacmlAttribute asd in attr.Attributes)
                    {
                        if (asd.AttributeId.OriginalString.Equals(_orgAttributeId))
                        {
                            foreach (var asff in asd.AttributeValues)
                            {
                                org = asff.Value;
                                break;
                            }
                        }

                        if (asd.AttributeId.OriginalString.Equals(_appAttributeId))
                        {
                            foreach (var asff in asd.AttributeValues)
                            {
                                app = asff.Value;
                                break;
                            }
                        }
                    }
                }
            }

            return GetAltinnAppsPolicyPath(org, app);
        }

        private string GetAltinnAppsPolicyPath(string org, string app)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Xacml", "3.0", "AltinnApps", org, app);
        }

        private string GetAltinnResourcePolicyPath(string resourceRegistryId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Xacml", "3.0", "ResourceRegistry", resourceRegistryId);
        }

        private static string GetAltinnAppsDelegationPolicyPath(string policyPath)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "blobs", "input", policyPath);
        }

        private string GetTestId(HttpContext context)
        {
            return context.Request.Headers["testcase"];
        }

        private string GetAltinnAppsPath()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Xacml", "3.0", "AltinnApps");
        }

        private string GetConformancePath()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Xacml", "3.0", "ConformanceTests");
        }
    }
}
