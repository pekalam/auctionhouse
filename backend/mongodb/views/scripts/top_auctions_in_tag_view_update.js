db.getCollection('AuctionsReadModel').aggregate([
    { $project: { "Tag": "$Tags", "_id": 0, "AuctionId": 1, "Product.Name": 1, "Views": 1 } },
    { $unwind: "$Tag" },
    { $sort: {"Views": -1} },
    { $group: { _id: "$Tag", Total: { $sum: NumberInt(1)}, Auctions: { $push: { AuctionId: "$AuctionId", AuctionName: "$Product.Name" } } } },
    { $project: { "Auctions": { $slice: [ "$Auctions", 5 ] }, Total: 1  } },
    { $out: "TopAuctionsInTag" }
])