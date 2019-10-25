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
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Query.ReadModel;
using Infrastructure.Services;
using RestEase;
using RestEase.Implementation;
using Web.Dto.Commands;

namespace CreateAuction
{
    public interface Req
    {
        [Post("/api/startCreateSession")]
        Task StartSession([Header("Authorization")] string jwt, [Body] StartAuctionCreateSessionCommandDto dto);

        [Post("/api/createAuction")]
        Task CreateAuction([Header("Authorization")] string jwt, [Body] CreateAuctionCommandDto dto);

        IRequester Requester { get; }
    }

    class Program
    {
        static CreateAuctionCommandDto CreateCommand()
        {
            Func<List<string>> fakeCategory = () =>
            {
                var categoryTreeService = new CategoryTreeService(new CategoryNameServiceSettings()
                    {CategoriesFilePath = "./categories.xml"});
                categoryTreeService.Init();

                var categoryBuilder = new CategoryBuilder(categoryTreeService);
                var rnd = new Random(new Random().Next());
                var catId = 0; //rnd.Next(0, 8);
                var subId = 0; //rnd.Next(0, 2);
                var sub2Id = 0; //rnd.Next(0, 2);
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
                .RuleFor(dto => dto.Category, fakeCategory())
                .RuleFor(dto => dto.BuyNowPrice,
                    faker => faker.Random.Bool() ? faker.Random.Decimal(20, 100) : new decimal?())
                .RuleFor(dto => dto.CorrelationId, "123")
                .RuleFor(dto => dto.StartDate, faker => DateTime.UtcNow.AddMinutes(20))
                .RuleFor(dto => dto.EndDate, faker => DateTime.UtcNow.AddDays(faker.Random.Int(5, 10)))
                .RuleFor(dto => dto.Product, faker => new Product()
                {
                    Name = faker.Commerce.ProductName(),
                    Description = faker.Lorem.Lines(20)
                });

            return cmd.Generate();
        }

        static void Main(string[] args)
        {
            var url = args[0];
            var jwt = $"Bearer " + args[1];

            var img = args.Length == 3 ? args[2] : "";


            var api = RestClient.For<Req>(url);

            var dto = new StartAuctionCreateSessionCommandDto() {CorrelationId = "123"};
            api.StartSession(jwt, dto)
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

                api.Requester.RequestVoidAsync(reqInfo)
                    .Wait();
            }


            api.CreateAuction(jwt, CreateCommand())
                .Wait();
        }
    }
}