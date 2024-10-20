﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Auctionhouse.Command.Controllers;
using Auctionhouse.Command.Dto;
using Bogus;
using Bogus.DataSets;
using Core.Common.Domain.Categories;
using RestEase;
using RestEase.Implementation;
using Web.Dto.Commands;

namespace CreateAuction
{
    public interface Req
    {
        [Post("/api/c/startCreateSession")]
        Task StartSession([Header("Authorization")] string jwt);

        [Post("/api/c/demoCode")]
        Task SetDemoCode([Header("Authorization")] string jwt, [Body] DemoCodeDto dto);

        [Post("/api/c/createAuction")]
        Task CreateAuction([Header("Authorization")] string jwt, [Body] CreateAuctionCommandDto dto);

        [Get("/api/s/status/{commandId}")]
        Task<RequestStatusDto> GetCommandStatus([Header("Authorization")] string jwt, [Path] string commandId);

        IRequester Requester { get; }
    }

    class Program
    {
        static CreateAuctionCommandDto CreateCommand(List<(int, int, int)> testCategories)
        {
            Func<List<string>> fakeCategory = () =>
            {

                var categoryTreeService = new XmlCategoryTreeStore.XmlCategoryTreeStore(new XmlCategoryTreeStore.XmlCategoryNameStoreSettings { CategoriesFilePath = "./categories.xml", SchemaFilePath = "./categories.xsd" });
                categoryTreeService.Init();

                var categoryBuilder = new CategoryBuilder(categoryTreeService);
                var rnd = new Random(new Random().Next());
                var cat = testCategories[rnd.Next(0, testCategories.Count)];
                var catId = cat.Item1;
                var subId = cat.Item2; //rnd.Next(0, 2);
                var sub2Id = cat.Item3; //rnd.Next(0, 2);
                var tree = categoryTreeService.GetCategoriesTree();
                var node = tree.SubCategories[catId];

                var categoryNames = new List<string>()
                {
                    node.CategoryName,
                    node.SubCategories[subId]
                        .CategoryName,
                    node.SubCategories[subId]
                        .SubCategories[sub2Id]
                        .CategoryName
                };
                return categoryNames;
            };

            var cmd = new Faker<CreateAuctionCommandDto>()
                .RuleFor(dto => dto.StartDate, faker => DateTime.UtcNow)
                .RuleFor(dto => dto.EndDate, faker => DateTime.UtcNow.AddDays(faker.Random.Int(5, 10)))
                .RuleFor(dto => dto.Product, faker => new ProductDto()
                {
                    Name = faker.Commerce.ProductName(),
                    Description = faker.Lorem.Lines(20),
                    Condition = faker.Random.Bool() ? (int)Auctions.Domain.Condition.New : (int)Auctions.Domain.Condition.Used
                })
                .RuleFor(dto => dto.Name, faker => faker.Commerce.ProductName() + " " + faker.Commerce.Color())
                .RuleFor(dto => dto.Tags,
                    faker => faker.Random.Bool() ? new[] {"tag1", "tag2", "tag3"} : new[] {"tag4", "tag5", "tag6"})
                .RuleFor(dto => dto.Category, fakeCategory())
                .RuleSet("buyNowAndAuctionOrAuction", set =>
                {
                    set.RuleFor(dto => dto.BuyNowOnly, false)
                        .RuleFor(dto => dto.BuyNowPrice,
                            faker => faker.Random.Bool() ? faker.Random.Decimal(20, 100) : 1);
                })
                .RuleSet("buyNowOnly", set =>
                {
                    set.RuleFor(dto => dto.BuyNowOnly, true)
                        .RuleFor(dto => dto.BuyNowPrice, faker => faker.Random.Decimal(20, 100));
                });


            bool randBool()
            {
                var r = new Random();
                return r.Next(100) < 50;
            }

            return cmd.Generate(randBool() ? "default,buyNowAndAuctionOrAuction" : "default,buyNowOnly");
        }

        static List<(int, int, int)> GetTestCategoriesList(string arg)
        {
            var strings = arg.Split('/');
            var ret = new List<(int, int, int)>();
            foreach (var s in strings)
            {
                int cat0 = s[0] - 48;
                int cat1 = s[1] - 48;
                int cat2 = s[2] - 48;
                ret.Add((cat0, cat1, cat2));
            }

            return ret;
        }

        static void Main(string[] args)
        {
            var url = args[0];
            var jwt = $"Bearer " + args[1];

            var imgDr = args.Length >= 3 ? args[2] : "";

            var categories =
                args.Length >= 4 ? GetTestCategoriesList(args[3]) : new List<(int, int, int)>() {(0, 0, 0)};

            var api = RestClient.For<Req>(url);

            var cmd = CreateCommand(categories);

            api.SetDemoCode(jwt, new DemoCodeDto
            {
                DemoCode = "12345"
            }).Wait();

            api.StartSession(jwt)
                .Wait();


            if (!string.IsNullOrEmpty(imgDr))
            {
                int i = 0;
                foreach (var img in Directory.EnumerateFiles(imgDr))
                {
                    var bytes = File.ReadAllBytes(img);
                    var content = new MultipartFormDataContent();
                    content.Headers.Add("enctype", "multipart/form-data");

                    var imageContent = new ByteArrayContent(bytes);

                    var ext = img.Split('.')
                        .Last();
                    imageContent.Headers.ContentType =
                        new MediaTypeHeaderValue(ext == "jpg" ? "image/jpeg" : $"image/{ext}");
                    content.Add(imageContent, "img", $"{i}.png");
                    content.Add(new StringContent(i.ToString()), "img-num");
                    content.Add(new StringContent("123"), "correlation-id");

                    var reqInfo = new RequestInfo(HttpMethod.Post, "/api/c/addAuctionImage");
                    reqInfo.AddHeaderParameter("Authorization", jwt);
                    reqInfo.SetBodyParameterInfo(BodySerializationMethod.Default, content);

                    var status = api.Requester.RequestAsync<RequestStatusDto>(reqInfo).Result;
                    for (int j = 0; j < 5; j++)
                    {
                        var newStatus = api.GetCommandStatus(jwt, status.CommandId).Result;
                        if (newStatus == null || newStatus.Status != "PENDING")
                        {
                            break;
                        }

                        Thread.Sleep(2000);
                    }
                    i++;
                }


            }


            api.CreateAuction(jwt, cmd)
                .Wait();
        }
    }
}