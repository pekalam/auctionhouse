using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Core.Command.Commands.AuctionCreateSession.StartAuctionCreateSession;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Infrastructure.Services;
using RestEase;
using RestEase.Implementation;
using Web.Dto.Commands;

namespace CreateAuction
{
    public interface Req
    {
        [Post("/api/startCreateSession")]
        Task StartSession([Header("Authorization")] string jwt);

        [Post("/api/createAuction")]
        Task CreateAuction([Header("Authorization")] string jwt, [Body] CreateAuctionCommandDto dto);

        [Get("/api/command/{correlationId}")]
        Task<RequestStatusDto> GetCommandStatus([Header("Authorization")] string jwt, [Path] string correlationId);

        IRequester Requester { get; }
    }

    class Program
    {
        static CreateAuctionCommandDto CreateCommand(List<(int, int, int)> testCategories)
        {
            Func<List<string>> fakeCategory = () =>
            {
                var categoryTreeService = new CategoryTreeService(new CategoryNameServiceSettings()
                    {CategoriesFilePath = "./categories.xml", SchemaFilePath = "./categories.xsd"});
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
                .RuleFor(dto => dto.StartDate, faker => DateTime.UtcNow.AddMinutes(20))
                .RuleFor(dto => dto.EndDate, faker => DateTime.UtcNow.AddDays(faker.Random.Int(5, 10)))
                .RuleFor(dto => dto.Product, faker => new ProductDto()
                {
                    Name = faker.Commerce.ProductName(),
                    Description = faker.Lorem.Lines(20),
                    Condition = faker.Random.Bool() ? Condition.New : Condition.Used
                })
                .RuleFor(dto => dto.BuyNowOnly, true)
                .RuleFor(dto => dto.Name, faker => faker.Commerce.ProductName() + " " + faker.Commerce.Color())
                .RuleFor(dto => dto.Tags, faker => faker.Random.Bool() ? new []{"tag1", "tag2", "tag3"} : new[] { "tag4", "tag5", "tag6" })
                .RuleSet("buyNowAndAuction", set =>
                {
                    set.RuleFor(dto => dto.Category, fakeCategory())
                        .RuleFor(dto => dto.BuyNowPrice,
                            faker => faker.Random.Bool() ? faker.Random.Decimal(20, 100) : 0);
                });

            return cmd.Generate("default,buyNowAndAuction");
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

            var img = args.Length >= 3 ? args[2] : "";

            var categories =
                args.Length >= 4 ? GetTestCategoriesList(args[3]) : new List<(int, int, int)>() {(0, 0, 0)};

            var api = RestClient.For<Req>(url);

            var cmd = CreateCommand(categories);

            api.StartSession(jwt)
                .Wait();


            if (!string.IsNullOrEmpty(img))
            {
                var bytes = File.ReadAllBytes(img);
                var content = new MultipartFormDataContent();
                content.Headers.Add("enctype", "multipart/form-data");

                var imageContent = new ByteArrayContent(bytes);

                var ext = img.Split('.')
                    .Last();
                imageContent.Headers.ContentType =
                    new MediaTypeHeaderValue(ext == "jpg" ? "image/jpeg" : $"image/{ext}");
                content.Add(imageContent, "img", "0.jpg");
                content.Add(new StringContent("0"), "img-num");
                content.Add(new StringContent("123"), "correlation-id");

                var reqInfo = new RequestInfo(HttpMethod.Post, "/api/addAuctionImage");
                reqInfo.AddHeaderParameter("Authorization", jwt);
                reqInfo.SetBodyParameterInfo(BodySerializationMethod.Default, content);

                var status = api.Requester.RequestAsync<RequestStatusDto>(reqInfo).Result;
                for (int i = 0; i < 5; i++)
                {
                    var newStatus = api.GetCommandStatus(jwt, status.CorrelationId).Result;
                    if(newStatus.Status != "PENDING") { break;}
                    Thread.Sleep(2000);
                }
            }


            api.CreateAuction(jwt, cmd)
                .Wait();
        }
    }
}