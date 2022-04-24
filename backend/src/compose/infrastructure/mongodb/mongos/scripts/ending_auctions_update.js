var conn = new Mongo();
var db = conn.getDB('appDb');

db.getCollection('AuctionsReadModel').aggregate([
    { $project: { "Tags": "$Tags", "_id": 0, "AuctionId": 1, "Name": 1, "Views": 1, "EndDate": 1, "ActualPrice": 1, "AuctionImages": 1, "TotalBids": 1, "MinToEnd": { $divide: [{$subtract: ["$EndDate", new Date()]}, 1000*60] } } },
    { $match: { "MinToEnd": { $lt: 180 } } },
    { $sort: { "MinToEnd": -1, "Views": -1, "TotalBids": -1 } },
    { $limit: 4 },
    { $out: "EndingAuctions" }
])