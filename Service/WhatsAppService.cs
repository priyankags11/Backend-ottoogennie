using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class WhatsAppService
{
    private readonly IConfiguration _config;

    public WhatsAppService(IConfiguration config)
    {
        _config = config;
    }

    public void SendMessage(string phone, string message)
    {
        var sid = _config["Twilio:AccountSid"];
        var token = _config["Twilio:AuthToken"];
        var from = _config["Twilio:FromNumber"];

        TwilioClient.Init(sid, token);

        MessageResource.Create(
            body: message,
            from: new PhoneNumber(from),
            to: new PhoneNumber($"whatsapp:+91{phone}")
        );
    }
}