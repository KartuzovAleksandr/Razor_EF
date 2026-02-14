using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
// для обработки ошибок SQL Server
using Microsoft.Data.SqlClient; 
using System.Diagnostics;
using System.Net.Sockets;
// для обработки статус-кодов HHTP
using Microsoft.AspNetCore.Http.Features; 

public class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;

    public int? StatusCode { get; private set; }

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
                SocketException socketEx => GetFriendlySockMessage(socketEx),
                SqlException sqlEx => GetFriendlySqlMessage(sqlEx),
                FileNotFoundException => "Не найден запрошенный файл.",
                InvalidOperationException => "Некорректная операция.",
                _ => "Неизвестная ошибка."
            };
        }

        // Новая обработка статус-кодов (404 и др.)
        var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        if (statusCodeFeature != null)
        {
            StatusCode = int.Parse(HttpContext.Request.Query["statusCode"]);
            _logger.LogWarning("Статус-код {StatusCode} для пути {OriginalPath}", StatusCode, statusCodeFeature.OriginalPath);

            ShortErrorInfo = StatusCode switch
            {
                404 => "Запрошенная страница не найдена !",
                403 => "Доступ запрещён !",
                429 => "Слишком много запросов. Попробуйте позже !",
                500 => "Внутренняя ошибка сервера !",
                _ => $"Ошибка HTTP {StatusCode}."
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

    private string GetFriendlySockMessage(SocketException socketEx)
    {
        // Можно кастомизировать сообщение в зависимости от кода ошибки
        return socketEx.ErrorCode switch
        {
            10061 => "Подключение не установлено, т.к.конечный компьютер отверг запрос на подключение",
            _ => $"Ошибка {socketEx.Message} при работе с сетевым сокетом."
        };
    }
}