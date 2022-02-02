namespace Adapter.EfCore.ReadModelNotifications
{
    using System.ComponentModel.DataAnnotations.Schema;

    internal class DbSagaEventsConfirmation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string CommandId { get; set; } = null!;
        public string CorrelationId { get; set; } = null!;
        public bool Completed { get; set; }
        public bool Failed { get; set; }
    }
}