using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace Scootr;

public class SortedHtmlDirectoryFormatter : IDirectoryFormatter
{
    private const string TextHtmlUtf8 = "text/html; charset=utf-8";

    private HtmlEncoder _htmlEncoder;

    private string HtmlEncode(string body)
    {
        return _htmlEncoder.Encode(body);
    }

    public SortedHtmlDirectoryFormatter()
    {
        _htmlEncoder = HtmlEncoder.Default;
    }

    public Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (contents == null)
        {
            throw new ArgumentNullException(nameof(contents));
        }

        context.Response.ContentType = TextHtmlUtf8;

        if (HttpMethods.IsHead(context.Request.Method))
        {
            // HEAD, no response body
            return Task.CompletedTask;
        }

        PathString requestPath = context.Request.PathBase + context.Request.Path;

        var builder = new StringBuilder();

        builder.AppendFormat(
@"<!DOCTYPE html>
<html lang=""{0}"">", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

        builder.AppendFormat(@"
<head>
  <title>Index Of {0}</title>", HtmlEncode(requestPath.Value));

        builder.Append(@"
  <style>
    body {
        font-family: ""Segoe UI"", ""Segoe WP"", ""Helvetica Neue"", 'RobotoRegular', sans-serif;
        font-size: 14px;}
    header h1 {
        font-family: ""Segoe UI Light"", ""Helvetica Neue"", 'RobotoLight', ""Segoe UI"", ""Segoe WP"", sans-serif;
        font-size: 28px;
        font-weight: 100;
        margin-top: 5px;
        margin-bottom: 0px;}
    #index {
        border-collapse: separate; 
        border-spacing: 0; 
        margin: 0 0 20px; }
    #index th {
        vertical-align: bottom;
        padding: 10px 5px 5px 5px;
        font-weight: 400;
        color: #a0a0a0;
        text-align: center; }
    #index td { padding: 3px 10px; }
    #index th, #index td {
        border-right: 1px #ddd solid;
        border-bottom: 1px #ddd solid;
        border-left: 1px transparent solid;
        border-top: 1px transparent solid;
        box-sizing: border-box; }
    #index th:last-child, #index td:last-child {
        border-right: 1px transparent solid; }
    #index td.length, td.modified { text-align:right; }
    a { color:#1ba1e2;text-decoration:none; }
    a:hover { color:#13709e;text-decoration:underline; }
  </style>
</head>
<body>
  <section id=""main"">");

        builder.AppendFormat(@"
    <header><h1>Index Of <a href=""/"">/</a>");

        string cumulativePath = "/";
        foreach (var segment in requestPath.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
        {
            cumulativePath = cumulativePath + segment + "/";
            builder.AppendFormat(@"<a href=""{0}"">{1}/</a>",
                HtmlEncode(cumulativePath), HtmlEncode(segment));
        }

        builder.AppendFormat(CultureInfo.CurrentUICulture,
  @"</h1></header>
    <table id=""index"" summary=""{0}"">
    <thead>
      <tr><th abbr=""{1}"">{1}</th><th abbr=""{2}"">{2}</th><th abbr=""{3}"">{4}</th></tr>
    </thead>
    <tbody>",
            HtmlEncode("The list of files in the given directory.  Column headers are listed in the first row."),
            HtmlEncode("Name"),
            HtmlEncode("Size"),
            HtmlEncode("Modified"),
            HtmlEncode("LastModified"));

        foreach (var subdir in contents.Where(info => info.IsDirectory).OrderBy(info => info.Name))
        {
            builder.AppendFormat(@"
      <tr class=""directory"">
        <td class=""name""><a href=""./{0}/"">{0}/</a></td>
        <td></td>
        <td class=""modified"">{1}</td>
      </tr>",
            HtmlEncode(subdir.Name),
            HtmlEncode(subdir.LastModified.ToString(CultureInfo.CurrentCulture)));
        }

        foreach (var file in contents.Where(info => !info.IsDirectory).OrderBy(info => info.Name))
        {
            builder.AppendFormat(@"
      <tr class=""file"">
        <td class=""name""><a href=""./{0}"">{0}</a></td>
        <td class=""length"">{1}</td>
        <td class=""modified"">{2}</td>
      </tr>",
            HtmlEncode(file.Name),
            HtmlEncode(file.Length.ToString("n0", CultureInfo.CurrentCulture)),
            HtmlEncode(file.LastModified.ToString(CultureInfo.CurrentCulture)));
        }

        builder.Append(@"
    </tbody>
    </table>
  </section>
</body>
</html>");
            string data = builder.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            context.Response.ContentLength = bytes.Length;
            return context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
