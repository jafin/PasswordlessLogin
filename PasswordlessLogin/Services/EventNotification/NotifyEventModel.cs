namespace SimpleIAM.PasswordlessLogin.Services.EventNotification
{
    public class NotifyEventModel
    {
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string EventType { get; set; }
        public string Details { get; set; }
    }
}