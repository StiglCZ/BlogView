using System.Net;
using System.Text;
using System.Security.Cryptography;

class Program {
    const string FileExtension = ".txt";
    private static bool AdminAuth(string tkn) {
        byte[] token = File.ReadAllBytes("./password");
        SHA256 sha256 = SHA256.Create();
        byte[] result = sha256.ComputeHash(Encoding.UTF8.GetBytes(tkn));

        return token.SequenceEqual(result);
    }

    private static void Serve(HttpListenerContext context) {
        if(context.Request.Url == null) return;
        if(context.Request.Url.LocalPath == "/favicon.ico") return;

        if(context.Request.Url.LocalPath.StartsWith("/blogs/")) {
            string fileName = "." + context.Request.Url.LocalPath;
            if(fileName.Contains("..")) return;
            if(!fileName.EndsWith(FileExtension)) fileName += FileExtension;
            if(!File.Exists(fileName)) {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }
            
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.OutputStream.Write(File.ReadAllBytes(fileName));
            context.Response.Close();
            return;
        }

        if(context.Request.Url.LocalPath.StartsWith("/all")) {
            string[] blogs = Directory.GetFiles("blogs/").Select(x => x.Replace(FileExtension, String.Empty)).ToArray();
            Array.Sort(blogs);
            Array.Reverse(blogs);
            
            string ToRespond = string.Join("\n", blogs);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ToRespond));
            context.Response.Close();
            return;
        }

        if(context.Request.Url.LocalPath.Equals("/")) {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.OutputStream.Write(File.ReadAllBytes("main.html"));
            context.Response.Close();
            return;
        }
    }
    public static async Task Main(string[] args) {
        HttpListener server = new();
        server.Prefixes.Add("http://*:8080/");
        server.Start();
        Console.WriteLine("Listening on *:8080/");
        
        while(server.IsListening) {
            HttpListenerContext context = await server.GetContextAsync();
            await Task.Run(() => Serve(context: context));
        }
        server.Abort();
        server.Close();
    }
}
