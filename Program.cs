using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

using Mono.Options;

namespace Scootr;

public class Program
{
    public static void Main(string[] args)
    {
        string urls = "http://*:8080";
        string webroot = ".";
        bool showHelp = false;
        string sslCertFile = null;
        string sslCertPassword = null;
        string sslKeyFile = null;

		OptionSet p = new()
        {
			"Usage: scootr [OPTIONS]",
			"Static File Web Server",
			"",
			"Options:",
			{ "sslCertFile=", "SSL Certificate File (PEM Encoded)", (string v) => sslCertFile = v },
			{ "sslCertPassword=", "SSL Certificate File", (string v) => sslCertPassword = v },
			{ "sslKeyFile=", "SSL Key File (PEM Encoded)", (string v) => sslKeyFile = v },
			{ "urls=", "IP Addresses and Ports to List on [default: http://*:8080]", (string v) => urls = v },
			{ "webroot=", "Directory to Serve Static Files from [default: .]", (string v) => webroot = v },
			{ "h|help",  "Show this Message and Exit", v => showHelp = v != null },
		};

		List<string> extra;

		try
        {
			extra = p.Parse(args);
		}
		catch (OptionException e)
        {
			Console.Write("scootr: ");
			Console.WriteLine(e.Message);
			Console.WriteLine("Try 'scootr --help' for more information.");

			return;
		}

		if (showHelp)
        {
			p.WriteOptionDescriptions(Console.Out);

            Environment.Exit(0);
		}

        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            Args = System.Environment.GetCommandLineArgs(),
            WebRootPath = webroot
        });

        builder
            .WebHost
            .UseKestrel()
            .ConfigureKestrel((context, options) =>
            {
                options.ConfigureEndpointDefaults(defaults =>
                {
                    defaults.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                    defaults.KestrelServerOptions.ConfigureHttpsDefaults(httpsDefaults =>
                    {
                        httpsDefaults.AllowAnyClientCertificate();

                        if (sslCertFile is string && sslKeyFile is string)
                        {
                            X509Certificate2 certificate = sslCertPassword is string
                                ? X509Certificate2.CreateFromEncryptedPemFile(sslCertFile, sslCertPassword, sslKeyFile)
                                : X509Certificate2.CreateFromPemFile(sslCertFile, sslKeyFile);

                            httpsDefaults.ServerCertificate = certificate;
                        }
                    });
                });
            });

        builder
            .Services
            .AddDirectoryBrowser();

        WebApplication app = builder.Build();

        app.Urls.Add(urls);

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
            DefaultContentType = "text/plain",
            ContentTypeProvider = provider
        });

        app.UseDirectoryBrowser(new DirectoryBrowserOptions()
        {
            FileProvider = new PhysicalFileProvider(Path.GetFullPath(webroot)),
            RequestPath = new PathString(""),
            Formatter = new SortedHtmlDirectoryFormatter()
        });
        
        app.Run();
    }
}
