using Microsoft.AspNetCore.StaticFiles;

namespace ServeDirectory;

public class Program
{
    /// <summary>
    /// Static File Web Server
    /// </summary>
    /// <param name="urls">IP Addresses and Ports to Listen on</param>
    /// <param name="webroot">Directory to Serve Static Files From</param>

    public static void Main(string urls = "http://*:8080", string webroot = ".")
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            Args = System.Environment.GetCommandLineArgs(),
            WebRootPath = webroot
        });


        builder
            .Services
            .AddDirectoryBrowser();

        WebApplication app = builder.Build();

        app.Urls.Add(urls);

        app.UseDirectoryBrowser();

        FileExtensionContentTypeProvider provider = new();

        provider.Mappings[".blat"] = "application/octect-stream";
        provider.Mappings[".c"] = "text/plain";
        provider.Mappings[".cfg"] = "text/plain";
        provider.Mappings[".cpp"] = "text/plain";
        provider.Mappings[".cs"] = "text/plain";
        provider.Mappings[".csproj"] = "text/plain";
        provider.Mappings[".css"] = "text/css";
        provider.Mappings[".dat"] = "application/octet-stream";
        provider.Mappings[".dll"] = "application/octet-stream";
        provider.Mappings[".h"] = "text/plain";
        provider.Mappings[".html"] = "text/html";
        provider.Mappings[".js"] = "text/javascript";
        provider.Mappings[".json"] = "application/json";
        provider.Mappings[".m3u8"] = "application/x-mpegURL";
        provider.Mappings[".pdb"] = "application/octet-stream";
        provider.Mappings[".properties"] = "application/octect-stream";
        provider.Mappings[".wasm"] = "application/wasm";
        provider.Mappings[".woff"] = "application/font-woff";
        provider.Mappings[".woff2"] = "application/font-woff";

        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
            DefaultContentType = "application/octet-stream",
            ContentTypeProvider = provider
        });
        
        app.Run();
    }
}
