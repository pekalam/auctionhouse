using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Core.Common.Domain.Categories;
using Core.Query.Queries.Auction.AuctionImage;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.Queries.Auction.Auctions.ByCategory;
using Core.Query.Queries.Auction.Auctions.ByTag;
using Core.Query.Queries.Auction.Categories;
using Core.Query.Queries.Auction.CommonTags;
using Core.Query.Queries.Auction.MostViewed;
using Core.Query.Queries.Auction.SingleAuction;
using Core.Query.Queries.Auction.TopAuctionsByTag;
using Core.Query.ReadModel;
using Core.Query.Views;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Queries;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    public class AuctionQueryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;


        public AuctionQueryController(IMediator mediator, IMapper mapper)
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
            var auction = await _mediator.Send(new AuctionQuery(queryDto.AuctionId));
            return Ok(auction);
        }

        [HttpGet("categories"), ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<CategoryTreeNode>> CategoriesQuery()
        {
            var categoriesTree = await _mediator.Send(new CategoriesQuery());
            return Ok(categoriesTree);
        }

        [HttpGet("auctionImage")]
        public async Task<ActionResult<AuctionImageQueryResult>> AuctionImage([FromQuery] AuctionImageQueryDto queryDto)
        {
            var query = new AuctionImageQuery()
            {
                ImageId = queryDto.ImageId
            };
            var result = await _mediator.Send(query);
            return File(result.Img.Img, "image/jpeg");
        }

        [HttpGet("topAuctionsByTag")]
        public async Task<ActionResult<TopAuctionsInTag>> TopAuctionsByTagQuery(
            [FromQuery] AuctionsByTagQueryDto queryDto)
        {
            var result = await _mediator.Send(new TopAuctionsByTagQuery() { Tag = queryDto.Tag, Page = queryDto.Page });
            return Ok(result);
        }

        [HttpGet("commonTags")]
        public async Task<ActionResult<CommonTags>> CommonTags([FromQuery] CommonTagsQueryDto queryDto)
        {
            var result = await _mediator.Send(new CommonTagsQuery() { Tag = queryDto.Tag });
            return Ok(result);
        }

        [HttpGet("auctionsByTag")]
        public async Task<ActionResult<AuctionsQueryResult>> AuctionsByTag([FromQuery] AuctionsByTagQueryDto queryDto)
        {
            var query = _mapper.Map<AuctionsByTagQuery>(queryDto);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("mostViewedAuctions")]
        public async Task<ActionResult<IEnumerable<MostViewedAuctionsResult>>> MostViewedAuctions()
        {
            var query = new MostViewedAuctionsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}