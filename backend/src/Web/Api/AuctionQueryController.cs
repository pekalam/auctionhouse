using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Core.Common;
using Core.Common.Domain.Categories;
using Core.Common.Query;
using Core.Query;
using Core.Query.Mediator;
using Core.Query.Queries.Auction.AuctionImage;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.Queries.Auction.Auctions.ByCategory;
using Core.Query.Queries.Auction.Auctions.ByTag;
using Core.Query.Queries.Auction.Categories;
using Core.Query.Queries.Auction.CommonTags;
using Core.Query.Queries.Auction.EndingAuctions;
using Core.Query.Queries.Auction.MostViewed;
using Core.Query.Queries.Auction.SingleAuction;
using Core.Query.Queries.Auction.TopAuctionsByTag;
using Core.Query.ReadModel;
using Core.Query.Views;
using Core.Query.Views.TopAuctionsByProductName;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Web.Dto.Queries;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    [FeatureGate("Auctionhouse_AuctionQueries")]
    public class AuctionQueryController : ControllerBase
    {
        private readonly QueryMediator _mediator;
        private readonly IMapper _mapper;

        public AuctionQueryController(QueryMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("auctions")]
        public async Task<ActionResult<IEnumerable<AuctionsQueryResult>>> Auctions(
            [FromQuery] AuctionsByCategoryQueryDto byCategoryQueryDto)
        {
            var query = _mapper.Map<AuctionsByCategoryQuery>(byCategoryQueryDto);
            var auctions = await _mediator.Send(query);
            return Ok(auctions);
        }

        [HttpGet("auction")]
        public async Task<ActionResult<AuctionRead>> Auction([FromQuery] AuctionQueryDto queryDto)
        {
            var query = _mapper.Map<AuctionQuery>(queryDto);
            var auction = await _mediator.Send(query);
            return Ok(auction);
        }

        [HttpGet("categories"), ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<CategoryTreeNode>> CategoriesQuery()
        {
            var categoriesTree = await _mediator.Send(new CategoriesQuery());
            return Ok(categoriesTree);
        }

        [HttpGet("auctionImage"), ResponseCache(Duration = 60 * 60 * 24, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<AuctionImageQueryResult>> AuctionImage([FromQuery] AuctionImageQueryDto queryDto)
        {
            var query = new AuctionImageQuery()
            {
                ImageId = queryDto.ImageId
            };
            var result = await _mediator.Send(query);
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
            var result = await _mediator.Send(new TopAuctionsInTagQuery() { Tag = queryDto.Tag, Page = queryDto.Page });
            return Ok(result);
        }

        [HttpGet("topAuctionsByProductName")]
        public async Task<ActionResult<TopAuctionsByProductName>> TopAuctionsByProductName(
            [FromQuery] TopAuctionsByProductNameDto queryDto)
        {
            var query = _mapper.Map<TopAuctionsByProductNameQuery>(queryDto);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("commonTags")]
        public async Task<ActionResult<CommonTags>> CommonTags([FromQuery] CommonTagsQueryDto queryDto)
        {
            var query = _mapper.Map<CommonTagsQuery>(queryDto);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("auctionsByTag")]
        public async Task<ActionResult<AuctionsQueryResult>> AuctionsByTag([FromQuery] AuctionsByTagQueryDto queryDto)
        {
            var query = _mapper.Map<AuctionsByTagQuery>(queryDto);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("mostViewedAuctions"), ResponseCache(Duration = 60 * 5, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<MostViewedAuctionsResult>>> MostViewedAuctions()
        {
            var query = new MostViewedAuctionsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("endingAuctions")]
        public async Task<ActionResult<IEnumerable<EndingAuctions>>> EndingAuctions()
        {
            var query = new EndingAuctionsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}