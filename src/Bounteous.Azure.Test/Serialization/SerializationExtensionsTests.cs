using System;
using Bounteous.Azure.Serialization;
using Bounteous.Azure.Test.Models;
using Bounteous.Azure.Test.Utils;
using Bounteous.Core.Validations;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Bounteous.Azure.Test.Serialization
{
    public class SerializationExtensionsTests : IDisposable
    {
        private readonly MockRepository mocks;
        private readonly Mock<HttpRequest> request;

        public SerializationExtensionsTests()
        {
            mocks = new MockRepository(MockBehavior.Strict);
            request = mocks.Create<HttpRequest>();
        }

        [Fact]
        public void CanSerializeFromHttpRequest()
        {
            var project = new ProjectModel { ProjectId = Guid.NewGuid(), Name = "testMe" };
            request.Setup(x => x.Body).Returns(project.ToJsonStream());

            var fromJson = request.Object.FromHttpRequest<ProjectModel>();

            Validate.Begin()
                .IsNotNull(project, nameof(project)).Check()
                .IsNotNull(fromJson, nameof(fromJson)).Check()
                .IsEqual(fromJson.ProjectId, project.ProjectId, nameof(fromJson.ProjectId))
                .IsEqual(fromJson.Name, project.Name, nameof(fromJson.Name))
                .Check();
        }
        
        public void Dispose() 
            => mocks.VerifyAll();
    }
}