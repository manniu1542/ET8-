namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            root.RemoveComponent<ClientSenderComponent>();
    
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            
            long playerId = await clientSenderComponent.LoginAsync(account, password);

            root.GetComponent<PlayerComponent>().MyId = playerId;
            // 进入大厅的ui
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}