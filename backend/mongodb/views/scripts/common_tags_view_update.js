var conn = new Mongo('db-mongos1');
var db = conn.getDB('appDb');

db.getCollection('AuctionsReadModel').aggregate([
    { $project: { "_id": "$Tags", "2": "$Tags" } },
    { $unwind: "$_id" },
    { $unwind: "$2" },
    { $group: { _id: "$_id", "a": { $push: { "n": "$2" } }  } },
    { $unwind: "$a" },
    { $project: { _id: "$_id", "a": "$a", "tag": "$a.n" } },
    { $match: { $expr: { $ne: ["$_id", "$tag"] } } },
    { $group: { _id: { "t1": "$_id", "t2": "$tag" }, c: { $sum: NumberInt(1) } } },
    { $project: { "_id": "$_id.t1", "with": "$_id.t2", "times": "$c" } },
    { $group: { _id: "$_id", "WithTags": { $push: { "Tag": "$with", "Times": "$times" } } } },
    { $out: "CommonTags" },
])