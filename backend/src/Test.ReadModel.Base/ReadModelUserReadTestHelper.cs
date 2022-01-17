using MongoDB.Driver;
using ReadModel.Core.Model;

namespace Test.ReadModel.Base
{
    public class ReadModelUserReadTestHelper : IDisposable
    {
        private List<string> _insertedUserIds = new();
        private ReadModelDbContext? _dbContext;

        public void Dispose()
        {
            if (_dbContext == null) return;
            _dbContext.UsersReadModel.DeleteMany(Builders<UserRead>.Filter.In(u => u.UserIdentity.UserId, _insertedUserIds));
        }

        public void TryInsertUserRead(Guid userId, ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
            var existing = dbContext.UsersReadModel
                .Find(u => u.UserIdentity.UserId == userId.ToString())
                .SingleOrDefault();

            if(existing != null)
            {
                return;
            }

            dbContext.UsersReadModel.InsertOne(new UserRead
            {
                UserIdentity = new UserIdentityRead
                {
                    UserId = userId.ToString(),
                    UserName = "test"
                }
            });
            _insertedUserIds.Add(userId.ToString());
        }
    }
}