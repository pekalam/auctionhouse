using Auctionhouse.Query.Queries;
using AutoMapper;
using Common.Application.Mediator;
using Core.Common.Domain.Categories;
using Microsoft.AspNetCore.Mvc;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.Auction.AuctionImage;
using ReadModel.Contracts.Queries.Auction.Auctions;
using ReadModel.Contracts.Queries.Auction.Auctions.ByCategory;
using ReadModel.Contracts.Queries.Auction.Auctions.ByTag;
using ReadModel.Contracts.Queries.Auction.Auctions.TopAuctions;
using ReadModel.Contracts.Queries.Auction.Categories;
using ReadModel.Contracts.Queries.Auction.CommonTags;
using ReadModel.Contracts.Queries.Auction.EndingAuctions;
using ReadModel.Contracts.Queries.Auction.MostViewed;
using ReadModel.Contracts.Queries.Auction.SingleAuction;
using ReadModel.Contracts.Views;

namespace Auctionhouse.Query.Controllers
{
    [ApiController]
    [Route("api/q")]
    //[FeatureGate("Auctionhouse_AuctionQueries")]
    public class AuctionQueryController : ControllerBase
    {
        private readonly CommandQueryMediator _mediator;
        private readonly IMapper _mapper;

        public AuctionQueryController(CommandQueryMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("auctions")]
        public async Task<ActionResult<IEnumerable<AuctionsQueryResult>>> Auctions(
            [FromQuery] AuctionsByCategoryQueryDto byCategoryQueryDto)
        {
            var query = _mapper.Map<AuctionsByCategoryQuery>(byCategoryQueryDto);
            var auctions = await _mediator.SendQuery(query);
            return Ok(auctions);
        }

        [HttpGet("auction")]
        public async Task<ActionResult<AuctionRead>> Auction([FromQuery] AuctionQueryDto queryDto)
        {
            var query = _mapper.Map<AuctionQuery>(queryDto);
            var auction = await _mediator.SendQuery(query);
            return Ok(auction);
        }

        [HttpGet("categories"), ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<CategoryTreeNode>> CategoriesQuery()
        {
            var categoriesTree = await _mediator.SendQuery(new CategoriesQuery());
            return Ok(categoriesTree);
        }

        [HttpGet("auctionImage"), ResponseCache(Duration = 60 * 60 * 24, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<AuctionImageQueryResult>> AuctionImage([FromQuery] AuctionImageQueryDto queryDto)
        {
            var query = new AuctionImageQuery()
            {
                ImageId = queryDto.ImageId
            };
            var result = await _mediator.SendQuery(query);
            if (result.Img != null)
            {
                return File(result.Img.Img, "image/jpeg");
            }
            else
            {
                //TODO
                return NotFound();
            }
        }

        [HttpGet("topAuctionsByTag")]
        public async Task<ActionResult<TopAuctionsInTag>> TopAuctionsByTagQuery(
            [FromQuery] TopAuctionsByTagQueryDto queryDto)
        {
            var result = await _mediator.SendQuery(new TopAuctionsInTagQuery() { Tag = queryDto.Tag, Page = queryDto.Page });
            return Ok(result);
        }

        [HttpGet("topAuctionsByProductName")]
        public async Task<ActionResult<TopAuctionsByProductName[]>> TopAuctionsByProductName(
            [FromQuery] TopAuctionsByProductNameDto queryDto)
        {
            var query = _mapper.Map<TopAuctionsByProductNameQuery>(queryDto);
            var result = await _mediator.SendQuery(query);
            return Ok(result);
        }

        [HttpGet("commonTags")]
        public async Task<ActionResult<CommonTags>> CommonTags([FromQuery] CommonTagsQueryDto queryDto)
        {
            var query = _mapper.Map<CommonTagsQuery>(queryDto);
            var result = await _mediator.SendQuery(query);
            return Ok(result);
        }

        [HttpGet("auctionsByTag")]
        public async Task<ActionResult<AuctionsQueryResult>> AuctionsByTag([FromQuery] AuctionsByTagQueryDto queryDto)
        {
            var query = _mapper.Map<AuctionsByTagQuery>(queryDto);
            var result = await _mediator.SendQuery(query);
            return Ok(result);
        }

        [HttpGet("mostViewedAuctions"), ResponseCache(Duration = 60 * 5, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<MostViewedAuctionsResult>>> MostViewedAuctions()
        {
            var query = new MostViewedAuctionsQuery();
            var result = await _mediator.SendQuery(query);
            return Ok(result);
        }

        [HttpGet("endingAuctions")]
        public async Task<ActionResult<IEnumerable<EndingAuctions>>> EndingAuctions()
        {
            var query = new EndingAuctionsQuery();
            var result = await _mediator.SendQuery(query);
            return Ok(result);
        }
    }
}