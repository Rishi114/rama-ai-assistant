using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Rama.Core;
using Rama.Skills;

namespace Rama
{
    /// <summary>
    /// HTTP API Server for Rama Brain
    /// Allows external clients (like Rama 3D) to communicate with Rama's brain
    /// </summary>
    public class RamaApiServer
    {
        private readonly HttpListener _listener;
        private readonly Brain _brain;
        private readonly Memory _memory;
        private readonly int _port;
        private bool _isRunning;

        public RamaApiServer(int port = 5000)
        {
            _port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            
            // Initialize brain components
            var skillManager = new SkillManager();
            skillManager.LoadBuiltInSkills();
            skillManager.LoadExternalSkills();
            
            _memory = new Memory();
            var learner = new Learner();
            _brain = new Brain(skillManager, learner, _memory);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"[Rama API] Server started on http://localhost:{_port}/");
            Console.WriteLine($"[Rama API] Endpoints:");
            Console.WriteLine($"  POST /brain - Send message to Rama");
            Console.WriteLine($"  GET  /skills - List available skills");
            Console.WriteLine($"  GET  /memory - Get memory stats");
            Console.WriteLine($"  GET  /status - Server status");

            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(context));
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        Console.WriteLine($"[Rama API] Error: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            Console.WriteLine("[Rama API] Server stopped");
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            var response = context.Response;
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (context.Request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            try
            {
                string path = context.Request.Url?.AbsolutePath ?? "/";
                string method = context.Request.HttpMethod;

                Console.WriteLine($"[Rama API] {method} {path}");

                if (method == "POST" && path == "/brain")
                {
                    await HandleBrainRequest(context, response);
                }
                else if (method == "GET" && path == "/skills")
                {
                    await HandleSkillsRequest(response);
                }
                else if (method == "GET" && path == "/memory")
                {
                    await HandleMemoryRequest(response);
                }
                else if (method == "GET" && path == "/status")
                {
                    await HandleStatusRequest(response);
                }
                else
                {
                    response.StatusCode = 404;
                    var error = JsonSerializer.Serialize(new { error = "Not found" });
                    await WriteResponse(response, error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Rama API] Request error: {ex.Message}");
                response.StatusCode = 500;
                var error = JsonSerializer.Serialize(new { error = ex.Message });
                await WriteResponse(response, error);
            }
        }

        private async Task HandleBrainRequest(HttpListenerContext context, HttpListenerResponse response)
        {
            using var reader = new StreamReader(context.Request.InputStream);
            var body = await reader.ReadToEndAsync();
            
            string input;
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(body);
                input = json.GetProperty("message").GetString() ?? json.GetProperty("input").GetString() ?? "";
            }
            catch
            {
                input = body;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                var error = JsonSerializer.Serialize(new { error = "Empty message" });
                await WriteResponse(response, error, 400);
                return;
            }

            Console.WriteLine($"[Rama API] Processing: {input.Substring(0, Math.Min(50, input.Length))}...");

            // Process through Rama brain
            var result = await _brain.ThinkAsync(input);

            var responseJson = JsonSerializer.Serialize(new
            {
                response = result,
                timestamp = DateTime.UtcNow,
                input = input
            });

            await WriteResponse(response, responseJson);
        }

        private async Task HandleSkillsRequest(HttpListenerResponse response)
        {
            var skillManager = new SkillManager();
            skillManager.LoadBuiltInSkills();
            
            var skills = skillManager.GetAllSkillInfo();
            
            var json = JsonSerializer.Serialize(new
            {
                count = skills.Count,
                skills = skills
            });
            
            await WriteResponse(response, json);
        }

        private async Task HandleMemoryRequest(HttpListenerResponse response)
        {
            var stats = _memory.GetStats();
            
            var json = JsonSerializer.Serialize(new
            {
                stats = stats,
                knowledgeBases = Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge"))
                    ? Directory.GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BrainKnowledge"))
                        .Select(d => Path.GetFileName(d)).ToArray()
                    : Array.Empty<string>()
            });
            
            await WriteResponse(response, json);
        }

        private async Task HandleStatusRequest(HttpListenerResponse response)
        {
            var json = JsonSerializer.Serialize(new
            {
                status = "running",
                version = "1.0.0",
                uptime = "N/A",
                port = _port,
                features = new[]
                {
                    "brain", "skills", "memory", "learning",
                    "3d-ui-ready", "voice-ready", "offline-capable"
                }
            });
            
            await WriteResponse(response, json);
        }

        private async Task WriteResponse(HttpListenerResponse response, string content, int statusCode = 200)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.Close();
        }
    }

    /// <summary>
    /// Standalone API server entry point
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 5000;
            
            if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
            {
                port = parsedPort;
            }

            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("       RAMA BRAIN API SERVER");
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine($"Starting on port {port}...");
            Console.WriteLine();

            var server = new RamaApiServer(port);
            
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                server.Stop();
            };

            await server.StartAsync();
        }
    }
}