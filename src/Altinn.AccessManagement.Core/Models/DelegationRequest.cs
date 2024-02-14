using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This element describe a public class DelegationRequest
    /// </summary>
    [DataContract]
    public class DelegationRequest
    {
        /// <summary>
        ///  Gets or sets the Guid of a valid DelegationRequest
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the status of an AuthorizationRequest
        /// </summary>
        public RestAuthorizationRequestStatus RequestStatus { get; set; }

        /// <summary>
        ///  Gets or sets The OrgID/personalID for who gets the delegation when the delegationRequest is approved
        /// </summary>
        [DataMember(IsRequired = true)]
        public string CoveredBy { get; set; }

        /// <summary>
        ///  Gets or sets The OrgID/personalID for who gets the delegation when the delegationRequest is approved
        /// </summary>
        [DataMember(IsRequired = true)]
        public string CoveredByName { get; set; }

        /// <summary>
        ///  Gets or sets The personalID who offer the delegation
        /// </summary>
        [DataMember(IsRequired = true)]
        public string OfferedBy { get; set; }

        /// <summary>
        ///  Gets or sets The name who offer the delegation
        /// </summary>
        [DataMember(IsRequired = true)]
        public string OfferedByName { get; set; }

        /// <summary>
        ///  Gets or sets The RedirectUrl is a link that sends the user back to the external website after he/she made an operation in Altinn
        /// </summary>
        [DataMember]
        public string RedirectUrl { get; set; }

        /// <summary>
        ///  Gets or sets the RequestMessage if any
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string RequestMessage { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether the session should be kept alive after a redirect
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool KeepSessionAlive { get; set; }

        /// <summary>
        ///  Gets or sets the date of when the request was created
        /// </summary>
        [DataMember]
        public DateTime Created { get; set; }

        /// <summary>
        ///  Gets or sets the date of when the request was last changed
        /// </summary>
        [DataMember]
        public DateTime LastChanged { get; set; }

        /// <summary>
        ///  Gets or sets The RequestServices are all information of services which the DelegationRequest need 
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<AuthorizationRequestResource> RequestResources { get; set; }
    }

    /// <summary>
    /// Represents a list of DelegationRequests
    /// </summary>
    public class DelegationRequests : List<DelegationRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRequests"/> class.
        /// </summary>
        public DelegationRequests()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationRequests"/> class.
        /// </summary>
        /// <param name="list">List of consents</param>
        public DelegationRequests(List<DelegationRequest> list) : base(list)
        {
        }

        /// <summary>
        /// Gets or sets the Continuation Token used for pagination and
        /// sequential retrieval. The returned value consists of the
        /// lastChanged timestamp of the last consent element returned and the
        /// ID. When this token is sent as an  argument in the continue
        /// parameter, the request will limit the responses only to consents
        /// that are  changed after this time. The ID will prevent endless
        /// loops if many elements have the same timestamp.
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
