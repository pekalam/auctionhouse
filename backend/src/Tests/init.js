rs.initiate();

var i = 0;
var tryCount = 6;
while (i < tryCount) {
  var state = db.adminCommand({ replSetGetStatus: 1 }).myState;
  if (state != 1) {
    print("replSetGetStatus.myState: " + state);
    i++;
    sleep(10000);
  } else {
    print("successfully configured replSet");
    break;
  }
}
if (i == tryCount) {
  print("replSet configuration error");
  exit;
}

db.createCollection("AuctionsReadModel");
db.createCollection("UsersReadModel");

