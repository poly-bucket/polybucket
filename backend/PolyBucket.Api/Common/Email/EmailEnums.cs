namespace PolyBucket.Api.Common.Email;

public enum EmailTransportKind
{
    Disabled,
    Smtp,
    Log
}

public enum EmailSecurityMode
{
    None,
    StartTls,
    SslOnConnect,
    Auto
}

public enum EmailSettingSource
{
    Default,
    Database,
    Environment
}

public enum EmailDiagnosticStage
{
    Configuration,
    Dns,
    Connect,
    Tls,
    Auth,
    Send
}
