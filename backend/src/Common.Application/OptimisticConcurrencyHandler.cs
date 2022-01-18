using Core.DomainFramework;

namespace Common.Application
{
    public class OptimisticConcurrencyHandler
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private int _maxAttempts = 3;

        public OptimisticConcurrencyHandler(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public int MaxAttempts { get => _maxAttempts; set
            {
                if (value <= 0) throw new ArgumentException(nameof(MaxAttempts) + " cannot be <= 0");
                _maxAttempts = value;
            }
        }

        public Task Run(Action<int, IUnitOfWorkFactory> action) => Run((repeats, unitOfWorkFactory) =>
        {
            action(repeats, unitOfWorkFactory);
            return Task.CompletedTask;
        });

        public async Task Run(Func<int, IUnitOfWorkFactory, Task> action)
        {
            int attempts = MaxAttempts;
            bool completed = false;
            while (!completed)
            {
                try
                {
                    await action(MaxAttempts - attempts, _unitOfWorkFactory);
                    completed = true;
                }
                catch (ConcurrencyException)
                {
                    attempts -= 1;
                    if (attempts == 0)
                    {
                        throw;
                    }
                    await Task.Delay(500);
                }
            }
        }
    }
}
