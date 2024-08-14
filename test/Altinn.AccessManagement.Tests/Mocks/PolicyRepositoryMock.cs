using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IPolicyRepository"></see> interface
    /// </summary>
    public class PolicyRepositoryMock(string filepath, ILogger<PolicyRepositoryMock> logger) : IPolicyRepository
    {
        private string Filepath { get; } = filepath;

        private ILogger<PolicyRepositoryMock> Logger { get; } = logger;

        /// <inheritdoc/>
        public Task<Stream> GetPolicyAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetTestDataStream(Filepath));
        }

        /// <inheritdoc/>
        public Task<Stream> GetPolicyVersionAsync(string version, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetTestDataStream(Filepath));
        }

        /// <inheritdoc/>
        public Task<Response<BlobContentInfo>> WritePolicyAsync(Stream fileStream, CancellationToken cancellationToken = default)
        {
            return WriteStreamToTestDataFolder(Filepath, fileStream);
        }

        /// <inheritdoc/>
        public Task<Response> DeletePolicyVersionAsync(string version, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<Response<BlobContentInfo>> WritePolicyConditionallyAsync(Stream fileStream, string blobLeaseId, CancellationToken cancellationToken = default)
        {
            if (blobLeaseId == "CorrectLeaseId" && !Filepath.Contains("error/blobstorageleaselockwritefail"))
            {
                return await WriteStreamToTestDataFolder(Filepath, fileStream);
            }

            throw new RequestFailedException((int)HttpStatusCode.PreconditionFailed, "The condition specified using HTTP conditional header(s) is not met.");
        }

        /// <inheritdoc/>
        public Task<string> TryAcquireBlobLease(CancellationToken cancellationToken = default)
        {
            if (Filepath.Contains("error/blobstoragegetleaselockfail"))
            {
                return Task.FromResult((string)null);
            }

            return Task.FromResult("CorrectLeaseId");
        }

        /// <inheritdoc/>
        public void ReleaseBlobLease(string leaseId, CancellationToken cancellationToken = default)
        {
        }

        /// <inheritdoc/>
        public Task<bool> PolicyExistsAsync(CancellationToken cancellationToken = default)
        {
            string fullpath = Path.Combine(GetDataInputBlobPath(), Filepath);

            if (File.Exists(fullpath))
            {
                return Task.FromResult(true);
            }

            Logger.LogWarning("Policy not found for full path" + fullpath);

            return Task.FromResult(false);
        }

        private static string GetDataOutputBlobPath()
        {
            return Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "blobs", "output");
        }

        private static string GetDataInputBlobPath()
        {
            return Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "blobs", "input");
        }

        private static Stream GetTestDataStream(string filepath)
        {
            string dataPath = Path.Combine(GetDataInputBlobPath(), filepath);
            Stream ms = new MemoryStream();
            if (File.Exists(dataPath))
            {
                using FileStream file = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
                file.CopyTo(ms);
            }

            return ms;
        }

        private static async Task<Response<BlobContentInfo>> WriteStreamToTestDataFolder(string filepath, Stream fileStream)
        {
            string dataPath = Path.Combine(GetDataOutputBlobPath(), filepath);

            if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dataPath));
            }

            int filesize;

            using (Stream streamToWriteTo = File.Open(dataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                await fileStream.CopyToAsync(streamToWriteTo);
                streamToWriteTo.Flush();
                filesize = (int)streamToWriteTo.Length;
            }

            BlobContentInfo mockedBlobInfo = BlobsModelFactory.BlobContentInfo(new ETag("ETagSuccess"), DateTime.Now, new byte[1], DateTime.Now.ToUniversalTime().ToString(), "encryptionKeySha256", "encryptionScope", 1);
            Mock<Response<BlobContentInfo>> mockResponse = new Mock<Response<BlobContentInfo>>();
            mockResponse.SetupGet(r => r.Value).Returns(mockedBlobInfo);

            Mock<Response> responseMock = new Mock<Response>();
            responseMock.SetupGet(r => r.Status).Returns((int)HttpStatusCode.Created);
            mockResponse.Setup(r => r.GetRawResponse()).Returns(responseMock.Object);

            return mockResponse.Object;
        }
    }
}