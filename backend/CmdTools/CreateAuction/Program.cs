using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Query.ReadModel;
using Infrastructure.Services;
using RestEase;
using Web.Dto.Commands;

namespace CreateAuction
{
    interface Req
    {
        [Post("/api/startCreateSession")]
        Task StartSession([Header("Authorization")] string jwt);

        [Post("/api/createAuction")]
        Task CreateAuction([Header("Authorization")] string jwt, [Body] CreateAuctionCommandDto dto);
    }

    class Program
    {
        static CreateAuctionCommandDto CreateCommand()
        {
            Func<List<string>> fakeCategory = () =>
            {
                var categoryTreeService = new CategoryTreeService(new CategoryNameServiceSettings()
                    { CategoriesFilePath = "./categories.xml" });
                categoryTreeService.Init();

                var categoryBuilder = new CategoryBuilder(categoryTreeService);
                var rnd = new Random(new Random().Next());
                var catId = 0;//rnd.Next(0, 8);
                var subId = 0;//rnd.Next(0, 2);
                var sub2Id = 0;//rnd.Next(0, 2);
                var tree = categoryTreeService.GetCategoriesTree();
                var node = tree.SubCategories[catId];

                var categoryNames = new List<string>()
                {
                    node.CategoryName,
                    node.SubCategories[subId].CategoryName,
                    node.SubCategories[subId].SubCategories[sub2Id].CategoryName
                };
                return categoryNames;
            };

            var cmd = new Faker<CreateAuctionCommandDto>()
                .RuleFor(dto => dto.Category, fakeCategory())
                .RuleFor(dto => dto.BuyNowPrice,
                    faker => faker.Random.Bool() ? faker.Random.Decimal(20, 100) : new decimal?())
                .RuleFor(dto => dto.CorrelationId, "123")
                .RuleFor(dto => dto.StartDate, faker => DateTime.UtcNow)
                .RuleFor(dto => dto.EndDate, faker => DateTime.UtcNow.AddMinutes(faker.Random.Int(5, 10)))
                .RuleFor(dto => dto.Product, faker => new Product()
                {
                    Name = faker.Commerce.ProductName(),
                    Description = faker.Lorem.Lines(20)
                });

            return cmd.Generate();
        }


        static async void Main(string[] args)
        {
            var url = args[0];
            var jwt = $"Bearer " + args[1];

            var product = new Product();

            var api = RestClient.For<Req>(url);

            await api.StartSession(jwt);

            await api.CreateAuction(jwt, CreateCommand());

            Console.WriteLine("Hello World!");
        }
    }
}
