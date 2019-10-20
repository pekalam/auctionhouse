var replicaset = {
  _id: replSetName,
  protocolVersion: 1,
  members: [
    { _id: 0, host: replica_1hostname + ":" + replica_1port, priority: 2 }
  ]
};
