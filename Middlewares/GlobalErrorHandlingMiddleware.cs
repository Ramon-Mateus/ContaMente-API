using System.Net;

namespace ContaMente.Middlewares
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            } catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string mensagem;

            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    mensagem = "Requisição inválida.";
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    mensagem = "Acesso não autorizado.";
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    mensagem = "Recurso não encontrado.";
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    mensagem = "Ocorreu um erro inesperado no servidor.";
                    break;
            }

            var errorResponse = new
            {
                status = (int)statusCode,
                mensagem,
                detalhe = exception.Message
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(result);
        }
    }
}