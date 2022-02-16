namespace Adapter.EfCore.ReadModelNotifications
{
    using System.ComponentModel.DataAnnotations.Schema;

    internal class DbSagaEventToConfirm
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string CorrelationId { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public bool Processed { get; set; }
    }
}