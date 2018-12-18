﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using QuantConnect.AlphaStream.Infrastructure;
using QuantConnect.AlphaStream.Models;
using QuantConnect.AlphaStream.Requests;

namespace QuantConnect.AlphaStream.Tests
{
    [TestFixture]
    public class AlphaStreamRestClientTests
    {
        const string TestAlphaId = "5443d94e213604f4fefbab185";
        const string TestAuthorId = "1f48359f6c6cbad65b091232eaae73ce";

        [Test]
        public async Task GetsAlphaById()
        {
            var request = new GetAlphaByIdRequest {Id = TestAlphaId};
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.Id, TestAlphaId);
        }

        [Test]
        public async Task GetAlphaInsights()
        {
            var request = new GetAlphaInsightsRequest {Id = TestAlphaId};
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
        }

        [Test]
        public async Task GetAuthorById()
        {
            var request = new GetAuthorByIdRequest {Id = TestAuthorId};
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.AreEqual(response.Id, TestAuthorId);
            Assert.AreEqual(response.Language, "C#");
        }

        [Test]
        public async Task GetAlphaPrices()
        {
            var request = new GetAlphaPricesRequest { Id = TestAlphaId };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
            var first = response.FirstOrDefault();
            Assert.AreEqual(first.PriceType, PriceType.Ask);
            Assert.AreEqual(first.SharedPrice, 39m);
            Assert.AreEqual(first.ExclusivePrice, null);
        }

        [Test]
        public async Task GetAlphaErrors()
        {
            var request = new GetAlphaErrorsRequest { Id = TestAuthorId };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
            var first = response.FirstOrDefault();
            Assert.AreEqual(first.Error.Substring(0, 10), "Test Error");
            Assert.AreEqual(first.StackTrace.Substring(0, 10), "Test stack");
        }

        [Test]
        public async Task GetAlphaList()
        {
            var request = new GetAlphaListRequest();
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
        }

        [Test]
        public async Task SearchAlphas()
        {
            var request = new SearchAlphasRequest
            {
                Author = TestAuthorId,
                AssetClasses = {AssetClass.Crypto},
                Accuracy = Range.Create(0, 1d),
                SharedFee = Range.Create(0, 999999999m),
                ExclusiveFee = Range.Create(0, 999999999m),
                Sharpe = Range.Create(0d, null),
                // this is the quantconnect symbol security identifier string
                Symbols = new List<string> {"BTCUSD XJ" },
                Uniqueness = Range.Create(0d, 100d)
            };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
        }

        [Test]
        public async Task SearchAuthors()
        {
            var request = new SearchAuthorsRequest
            {
                Biography = "QuantConnect",
                Languages = { "C#" },
                SignedUp = Range.Create(Time.UnixEpoch, DateTime.Today),
                AlphasListed = Range.Create(0, int.MaxValue),
                ForumComments = Range.Create(0, int.MaxValue),
                ForumDiscussions = Range.Create(0, int.MaxValue),
                LastLogin = Range.Create(Time.UnixEpoch, DateTime.Today),
                Projects = Range.Create(0, int.MaxValue)
            };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
        }

        [Test]
        public async Task Subscribe()
        {
            var request = new SubscribeRequest { Id = TestAlphaId, Exclusive = false };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.Messages.Count);
            Assert.AreEqual("Subscribed successfully (shared)", response.Messages[0]);
        }

        [Test]
        public async Task Unubscribe()
        {
            var request = new UnsubscribeRequest { Id = TestAlphaId };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.Messages.Count);
            Assert.AreEqual("Subscription cancelled", response.Messages[0]);
        }

        [Test]
        public async Task CreateConversation()
        {
            var request = new CreateConversationRequest
            {
                Id = "118d1cbc375709792ea4d823a",
                From = "support@quantconnect.com",
                Message = "Hello World!",
                Subject = "Alpha Conversation",
                CC = "support@quantconnect.com"
            };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
        }

        [Test]
        public async Task CreateBid()
        {
            var createRequest = new CreateBidPriceRequest
            {
                Id = TestAlphaId,
                SharedPrice = 7,
                GoodUntil = DateTime.Now.AddDays(1).ToUnixTime()
            };
            var createResponse = await ExecuteRequest(createRequest).ConfigureAwait(false);
            Assert.IsNotNull(createResponse);
            Assert.IsTrue(createResponse.Success);

            var request = new GetAlphaPricesRequest { Id = TestAlphaId };
            var response = await ExecuteRequest(request).ConfigureAwait(false);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response);
            var last = response.LastOrDefault();
            Assert.AreEqual(last.SharedPrice, 6.99);
        }

        private static async Task<T> ExecuteRequest<T>(IRequest<T> request)
        {
            var service = new AlphaStreamRestClient(Credentials.Test);
            return await service.Execute(request).ConfigureAwait(false);
        }
    }
}