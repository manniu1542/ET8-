namespace ET
{
    public interface IRoomMessage: IMessage
    {
        long PlayerId { get; set; }
    }

    public interface IRoomRequest: IRequest
    {
        long PlayerId { get; set; }
    }

    public interface IRoomResponse: IResponse
    {
    }
}