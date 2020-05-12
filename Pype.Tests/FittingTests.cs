using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pype;
using Pype.Models;

namespace Pype.Tests
{
    [TestClass]
    public class FittingTests
    {
        [TestMethod]
        public async Task CanMakeGetRequests()
        {
            Fitting fitting = new Fitting
            {
                ApiBasePath = "https://xkcd.com",
                RequestSuffix = "/info.0.json",
                ContentType = "application/json",
                Method = "GET"
            };

            FittingResponse<object> response = await fitting.SendRequest<object>();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Status.Health == FittingResponseStatusHealth.Good);
        }

        [TestMethod]
        public async Task CanMakePostRequests()
        {
            Fitting fitting = new Fitting
            {
                ApiBasePath = "https://localhost:5001",
                RequestSuffix = "/api/offers",
                ContentType = "application/json",
                Method = "POST"
            };

            FittingResponse<object> response = await fitting.SendRequest<object>();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Status.Health == FittingResponseStatusHealth.Good);
        }
    }
}
