using Microsoft.JSInterop;

namespace DagoniteEmpire.Helper
{
    public static class IJSRuntimeExtension
    {
        public static async ValueTask AttrLimit(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowToastr", "attributeLimit", message);
        }
        public static async ValueTask BaseSkillLimit(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowToastr", "baseSkillLimit", message);
        }
        public static async ValueTask SpecialSkillLimit(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowToastr", "specialSkillLimit", message);
        }
        public static async ValueTask ToastrWarning(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowToastr", "warning", message);
        }
        public static async ValueTask ToastrError(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowToastr", "error", message);
        }

        public static async ValueTask ToastrSuccess(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowToastr", "success", message);
        }

        public static async ValueTask SweetAlertSuccess(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowSweetAlert", "success", message);
        }
        public static async ValueTask SweetAlertFailure(this IJSRuntime jsRuntime, string message)
        {
            await jsRuntime.InvokeVoidAsync("ShowSweetAlert", "error", message);
        }

        public static async ValueTask ScrollToBottom(this IJSRuntime jsRuntime, string container)
        {
            await jsRuntime.InvokeVoidAsync("ScrollToBottom",  container);
        }


    }
}
