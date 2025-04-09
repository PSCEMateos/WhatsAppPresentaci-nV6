namespace WhatsAppPresentacionV6.Modelos
{
    public class WebhookValidationRequest
    {
        public string mode { get; set; }
        public string challenge { get; set; }
        public string? TokenDeVerificacion { get; set; }
        public string? verify_token { get; set; }
    }
}
