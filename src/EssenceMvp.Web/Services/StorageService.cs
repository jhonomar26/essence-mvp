using Microsoft.JSInterop;

namespace EssenceMvp.Web.Services;

public sealed class StorageService(IJSRuntime js)
{
    public ValueTask<string?> GetAsync(string key) =>
        js.InvokeAsync<string?>("storage.get", key);

    public ValueTask SetAsync(string key, string value) =>
        js.InvokeVoidAsync("storage.set", key, value);

    public ValueTask RemoveAsync(string key) =>
        js.InvokeVoidAsync("storage.remove", key);

    public ValueTask DevLogAsync(string msg) =>
        js.InvokeVoidAsync("devLog", "info", msg);

    public ValueTask DevErrorAsync(string msg) =>
        js.InvokeVoidAsync("devLog", "error", msg);
}
