using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Threading.Tasks;

namespace Project02.Binder
{
    public class DateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var valueStr = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(valueStr))
            {
                return Task.CompletedTask;
            }

            try
            {
                // Chuyển "CH" thành "PM" và "SA" thành "AM"
                valueStr = valueStr.Replace("CH", "PM").Replace("SA", "AM");

                // Các format datetime phổ biến bạn muốn parse
                string[] formats = {
                "dd/MM/yyyy h:mm tt",
                "MM/dd/yyyy h:mm tt",
                "yyyy-MM-ddTHH:mm",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy/MM/dd HH:mm:ss"
            };

                if (DateTime.TryParseExact(valueStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    bindingContext.Result = ModelBindingResult.Success(date);
                }
                else // Thử parse mặc định
                {
                    if (DateTime.TryParse(valueStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        bindingContext.Result = ModelBindingResult.Success(date);
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(modelName, "Ngày giờ không đúng định dạng.");
                    }
                }
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.TryAddModelError(modelName, ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}
