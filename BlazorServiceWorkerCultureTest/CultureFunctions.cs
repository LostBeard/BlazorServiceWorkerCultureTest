using Microsoft.JSInterop;
using System.Globalization;

namespace BlazorServiceWorkerCultureTest
{
    public static class CultureFunctions
    {
        public static async Task SetCulture(IJSRuntime jsRuntime)
        {
            // Get culture
            string countryCode = await jsRuntime.InvokeAsync<string>("getCulture");

            // Set culture
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                CultureInfo culture = new(countryCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
        }
    }
}
