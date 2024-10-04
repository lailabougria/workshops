namespace Contracts;

public interface IPartialOrderPacked: IEvent
{
    Guid OrderId { get; set; }
    Dictionary<Guid,int> MissingProducts { get; set; }
}