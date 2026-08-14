namespace Host.ContextData;

public class DatabaseSettings
{
    public bool IsMySql { get; set; }
    public string MySqlConnectionString { get; set; } = string.Empty;
    public string MssqlConnectionString { get; set; } = string.Empty;
}