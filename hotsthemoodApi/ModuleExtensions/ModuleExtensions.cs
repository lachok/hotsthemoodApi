using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Validation;

namespace hotsthemoodApi.ModuleExtensions
{
    public static class ModuleExtensions
    {
        public static void GetHandler<TOut>(this NancyModule module, string path, Func<TOut> handler)
        {
            module.Get[path] = _ => RunHandler(module, handler);
        }

        public static void GetHandler<TIn, TOut>(this NancyModule module, string path, Func<TIn, TOut> handler)
        {
            module.Get[path] = _ => RunHandler(module, handler);
        }

        public static void GetHandlerAsync<TIn, TOut>(this NancyModule module, string path,
            Func<TIn, Task<TOut>> handler)
        {
            module.Get[path, true] = async (x, ctx) => await RunHandlerAsync(module, handler);
        }

        public static void GetHandlerAsync<TOut>(this NancyModule module, string path, Func<Task<TOut>> handler)
        {
            module.Get[path, true] = async (x, ctx) => await RunHandlerAsync(module, handler);
        }

        public static object RunHandler<TOut>(this NancyModule module, Func<TOut> handler)
        {
            try
            {
                return handler();
            }
            catch (HttpException hEx)
            {
                return module.Negotiate.WithStatusCode(hEx.StatusCode).WithModel(hEx.Content);
            }
        }

        public static async Task<object> RunHandlerAsync<TOut>(this NancyModule module, Func<Task<TOut>> handler)
        {
            try
            {
                TOut result = await handler();
                return result;
            }
            catch (HttpException hEx)
            {
                return module.Negotiate.WithStatusCode(hEx.StatusCode).WithModel(hEx.Content);
            }
        }

        public static object RunHandler<TIn, TOut>(this NancyModule module, Func<TIn, TOut> handler)
        {
            try
            {
                TIn model;
                try
                {
                    model = module.BindAndValidate<TIn>();
                    if (!module.ModelValidationResult.IsValid)
                    {
                        return module.Negotiate.RespondWithValidationFailure(module.ModelValidationResult);
                    }
                }
                catch (ModelBindingException)
                {
                    return module.Negotiate.RespondWithValidationFailure("Model binding failed");
                }

                return handler(model);
            }
            catch (HttpException hEx)
            {
                return module.Negotiate.WithStatusCode(hEx.StatusCode).WithModel(hEx.Content);
            }
        }

        public static async Task<object> RunHandlerAsync<TIn, TOut>(this NancyModule module,
            Func<TIn, Task<TOut>> handler)
        {
            try
            {
                TIn model;
                try
                {
                    model = module.BindAndValidate<TIn>();
                    if (!module.ModelValidationResult.IsValid)
                    {
                        return module.Negotiate.RespondWithValidationFailure(module.ModelValidationResult);
                    }
                }
                catch (ModelBindingException)
                {
                    return module.Negotiate.RespondWithValidationFailure("Model binding failed");
                }

                TOut result = await handler(model);
                return result;
            }
            catch (HttpException hEx)
            {
                return module.Negotiate.WithStatusCode(hEx.StatusCode).WithModel(hEx.Content);
            }
        }

        public static Negotiator RespondWithValidationFailure(this Negotiator negotiate,
            ModelValidationResult validationResult)
        {
            var model = new ValidationFailedResponse(validationResult);

            return negotiate
                .WithModel(model)
                .WithStatusCode(HttpStatusCode.BadRequest);
        }

        public static object RespondWithValidationFailure(this Negotiator negotiate, string message)
        {
            var model = new ValidationFailedResponse(message);

            return negotiate
                .WithModel(model)
                .WithStatusCode(HttpStatusCode.BadRequest);
        }
    }


    public class ValidationFailedResponse
    {
        public List<string> Messages { get; set; }

        public ValidationFailedResponse()
        {
        }

        public ValidationFailedResponse(ModelValidationResult validationResult)
        {
            Messages = new List<string>();
            ErrorsToStrings(validationResult);
        }

        public ValidationFailedResponse(string message)
        {
            Messages = new List<string>
            {
                message
            };
        }

        private void ErrorsToStrings(ModelValidationResult validationResult)
        {
            foreach (var errorGroup in validationResult.Errors)
            {
                foreach (var error in errorGroup.Value)
                {
                    Messages.Add(error.ErrorMessage);
                }
            }
        }
    }


    [Serializable]
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public object Content { get; private set; }

        public HttpException(HttpStatusCode statusCode, object content)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public HttpException(HttpStatusCode statusCode)
            : this(statusCode, string.Empty)
        {
        }

        public HttpException()
            : this(HttpStatusCode.InternalServerError, string.Empty)
        {
        }

        public static HttpException NotFound(object content)
        {
            return new HttpException(HttpStatusCode.NotFound, content);
        }

        public static Exception InternalServerError(object content)
        {
            return new HttpException(HttpStatusCode.InternalServerError, content);
        }
    }
}
