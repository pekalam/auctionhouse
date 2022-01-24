var replicaset = {
  _id: replSetName,
  protocolVersion: 1,
  members: [
    { _id: 0, host: replica_1hostname + ":" + replica_1port, priority: 2 }
  ]
};

rs.initiate(replicaset);
var i = 0;
var tryCount = 3;
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
  exit();
}
