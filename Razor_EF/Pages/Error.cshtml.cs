using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Microsoft.Data.SqlClient; // Подключите пространство имён для SqlException

public class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // Сообщения для пользователя
    public string ErrorMessage { get; private set; } = "Произошла внутренняя ошибка сервера";
    public string? ShortErrorInfo { get; private set; }

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (feature?.Error != null)
        {
            var ex = feature.Error;
            _logger.LogError(ex, "Необработанное исключение на странице {Path}", feature.Path);

            // Генерация краткого сообщения пользователю
            ShortErrorInfo = ex switch
            {
                SqlException sqlEx => GetFriendlySqlMessage(sqlEx),
                FileNotFoundException => "Не найден запрошенный файл.",
                InvalidOperationException => "Некорректная операция.",
                _ => "Неизвестная ошибка."
            };
        }
    }

    private string GetFriendlySqlMessage(SqlException sqlEx)
    {
        // Можно кастомизировать сообщение в зависимости от кода ошибки SQL
        return sqlEx.Number switch
        {
            53 => "Не удалось подключиться к серверу базы данных. Проверьте доступность SQL Server.",
            4060 => "Ошибка доступа к базе данных. Проверьте параметры подключения.",
            18456 => "Ошибка авторизации в SQL Server.",
            _ => "Ошибка при работе с базой данных."
        };
    }
}