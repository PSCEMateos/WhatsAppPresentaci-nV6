namespace WhatsAppPresentacionV6.Modelos.RecivedMedia
{
    public class EncryptedRequestModel
    {
        public string encrypted_flow_data { get; set; }
        public string encrypted_aes_key { get; set; }
        public string initial_vector { get; set; }
    }
}
