using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.CommonTags
{
//    public class TreeNode
//    {
//        public static readonly TreeNode Empty = new TreeNode() {Value = null, Children = new TreeNode[0]};
//
//        public string Value { get; set; }
//        public TreeNode[] Children { get; set; }
//    }

    public class CommonTagsQuery : IRequest<CommonTagsReadModel>
    {
        public string Tag { get; set; }
    }

    public class CommonTagsQueryHandler : IRequestHandler<CommonTagsQuery, CommonTagsReadModel>
    {
        private readonly ReadModelDbContext _dbContext;

        public CommonTagsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public Task<CommonTagsReadModel> Handle(CommonTagsQuery request, CancellationToken cancellationToken)
        {
            var commonTags = _dbContext.CommonTagsCollection.Find(m => m.Tag == request.Tag).FirstOrDefault();
            return Task.FromResult(commonTags);
        }
    }


}
