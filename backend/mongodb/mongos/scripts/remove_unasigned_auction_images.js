var conn = new Mongo();
var db = conn.getDB('appDb');

db.fs.files.aggregate([
    {$project: {filename: 1, metadata: 1, minSinceUpload: { $divide: [{$subtract: [new Date(), "$uploadDate"]}, 1000*60] } } },
    {$match: { $and: [ {"metadata.IsAssignedToAuction": false}, { minSinceUpload: { $gt: 10 } } ] } }
])
.forEach(function(doc)
{
    db.fs.files.remove( {"_id": { $eq: doc._id } } );
    db.fs.chunks.remove( {"files_id": { $eq: doc._id } } );
})