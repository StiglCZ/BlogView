using System.Net;
using System.Text;

class Config {
    public short  Port  = 8080;
    public string Title = "TITLE";
    public string Color = "lightblue";
    public string RootDirectory = ".";
    public string Directory = "blogs";
    public string FileExtension = ".txt";
    private Config() {}
    public static Config Load(string fileName = "blog.conf") {
        Config result = new Config();
        
        string[] confFile = File.ReadAllLines(fileName);
        foreach(string line in confFile) {
            int i = 0;
            while(i < line.Length && line[i] != '#') {
                if (line[i] == '"') {
                    while (i < line.Length - 1 && line[++i] != '"') ;
                    if(i == line.Length - 1 && line[i] == '"') i++;
                }
                else i++;
            }

            string rwline = line.Substring(0, i).Trim();
            if(!rwline.Contains('=')) continue;
            else if(rwline.StartsWith("Port")) short.TryParse(rwline.Split('=')[1].Trim(), out result.Port);
            else if(rwline.StartsWith("Title"))         result.Title =         rwline.Substring(rwline.IndexOf('"') + 1, rwline.LastIndexOf('"') - rwline.IndexOf('"') -1);
            else if(rwline.StartsWith("Color"))         result.Color =         rwline.Substring(rwline.IndexOf('"') + 1, rwline.LastIndexOf('"') - rwline.IndexOf('"') -1);
            else if(rwline.StartsWith("RootDirectory")) result.RootDirectory = rwline.Substring(rwline.IndexOf('"') + 1, rwline.LastIndexOf('"') - rwline.IndexOf('"') -1);
            else if(rwline.StartsWith("BlogDirectory")) result.Directory     = rwline.Substring(rwline.IndexOf('"') + 1, rwline.LastIndexOf('"') - rwline.IndexOf('"') -1);
            else if(rwline.StartsWith("FileExtension")) result.FileExtension = rwline.Substring(rwline.IndexOf('"') + 1, rwline.LastIndexOf('"') - rwline.IndexOf('"') -1);
        }
        
        return result;
    }
}
class Program {
    static Config? config = null;

    private static void Serve(HttpListenerContext context) {
        if(config == null) {
            Console.WriteLine("Config was not loaded propertly!");
            Environment.Exit(-1);
        }
        if(context.Request.Url == null) return;
        if(context.Request.Url.LocalPath == "/favicon.ico") return;

        if(context.Request.Url.LocalPath.StartsWith("/" + config.Directory + "/")) {
            string fileName = config.RootDirectory + context.Request.Url.LocalPath;
            if(fileName.Contains("..")) return;
            if(!fileName.EndsWith(config.FileExtension)) fileName += config.FileExtension;
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
            string[] blogs = System.IO.Directory.GetFiles(Path.Combine(config.RootDirectory, config.Directory))
                .Select(x => x.Replace(config.FileExtension, String.Empty))
                .ToArray();
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
            string fileText = File.ReadAllText("main.html")
                .Replace("#title", config.Title)
                .Replace("#color", config.Color);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(fileText));
            context.Response.Close();
            return;
        }
    }
    public static async Task Main(string[] args) {
        HttpListener server = new();
        config = Config.Load();
        server.Prefixes.Add($"http://*:{config.Port}/");
        server.Start();
        Console.WriteLine($"Listening on http://0.0.0.0:{config.Port}/");
        
        while(server.IsListening) {
            HttpListenerContext context = await server.GetContextAsync();
            await Task.Run(() => Serve(context: context));
        }
        server.Abort();
        server.Close();
    }
}
