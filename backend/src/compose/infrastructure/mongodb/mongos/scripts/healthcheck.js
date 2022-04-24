var conn = new Mongo();
var db = conn.getDB('appDb');

var collections = db.getCollectionNames();

assert(collections.length > 0);