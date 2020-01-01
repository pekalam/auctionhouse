var conn = new Mongo('db-mongos1');
var db = conn.getDB('appDb');


db.getCollection('AuctionsReadModel').aggregate([
    { $match : { "Archived" : false } },
    { $project: { "CanonicalName": "$Product.CanonicalName", "_id": 0, "AuctionId": 1, "Name": 1, "Views": 1, "AuctionImage": { $arrayElemAt: [ "$AuctionImages", 0 ] } } },
    { $unwind: "$CanonicalName" },
    { $sort: {"Views": -1} },
    { $group: { _id: "$CanonicalName", Total: { $sum: NumberInt(1)}, Auctions: { $push: { AuctionId: "$AuctionId", AuctionName: "$Name", AuctionImage: "$AuctionImage" } } } },
    { $project: { "Auctions": { $slice: [ "$Auctions", 5 ] }, Total: 1  } },
    { $out: "TopAuctionsByProductName" }
])