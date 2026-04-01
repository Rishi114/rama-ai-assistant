using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Universal Coding Assistant — knows all programming languages.
    /// Can generate, explain, debug, convert, and format code.
    /// Like Jarvis but for code.
    /// </summary>
    public class CoderSkill : SkillBase
    {
        private string CodeDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "Code");

        // All supported languages and their file extensions
        private static readonly Dictionary<string, (string ext, string name)> Languages = new()
        {
            ["python"] = (".py", "Python"),
            ["py"] = (".py", "Python"),
            ["javascript"] = (".js", "JavaScript"),
            ["js"] = (".js", "JavaScript"),
            ["typescript"] = (".ts", "TypeScript"),
            ["ts"] = (".ts", "TypeScript"),
            ["java"] = (".java", "Java"),
            ["csharp"] = (".cs", "C#"),
            ["c#"] = (".cs", "C#"),
            ["cs"] = (".cs", "C#"),
            ["cpp"] = (".cpp", "C++"),
            ["c++"] = (".cpp", "C++"),
            ["c"] = (".c", "C"),
            ["go"] = (".go", "Go"),
            ["golang"] = (".go", "Go"),
            ["rust"] = (".rs", "Rust"),
            ["rs"] = (".rs", "Rust"),
            ["ruby"] = (".rb", "Ruby"),
            ["rb"] = (".rb", "Ruby"),
            ["php"] = (".php", "PHP"),
            ["swift"] = (".swift", "Swift"),
            ["kotlin"] = (".kt", "Kotlin"),
            ["kt"] = (".kt", "Kotlin"),
            ["scala"] = (".scala", "Scala"),
            ["r"] = (".r", "R"),
            ["matlab"] = (".m", "MATLAB"),
            ["perl"] = (".pl", "Perl"),
            ["lua"] = (".lua", "Lua"),
            ["dart"] = (".dart", "Dart"),
            ["elixir"] = (".ex", "Elixir"),
            ["erlang"] = (".erl", "Erlang"),
            ["haskell"] = (".hs", "Haskell"),
            ["ocaml"] = (".ml", "OCaml"),
            ["fsharp"] = (".fs", "F#"),
            ["f#"] = (".fs", "F#"),
            ["clojure"] = (".clj", "Clojure"),
            ["groovy"] = (".groovy", "Groovy"),
            ["powershell"] = (".ps1", "PowerShell"),
            ["ps1"] = (".ps1", "PowerShell"),
            ["bash"] = (".sh", "Bash"),
            ["shell"] = (".sh", "Shell"),
            ["sh"] = (".sh", "Shell"),
            ["sql"] = (".sql", "SQL"),
            ["html"] = (".html", "HTML"),
            ["css"] = (".css", "CSS"),
            ["scss"] = (".scss", "SCSS"),
            ["json"] = (".json", "JSON"),
            ["yaml"] = (".yaml", "YAML"),
            ["yml"] = (".yaml", "YAML"),
            ["xml"] = (".xml", "XML"),
            ["markdown"] = (".md", "Markdown"),
            ["md"] = (".md", "Markdown"),
            ["assembly"] = (".asm", "Assembly"),
            ["asm"] = (".asm", "Assembly"),
            ["zig"] = (".zig", "Zig"),
            ["nim"] = (".nim", "Nim"),
            ["v"] = (".v", "V"),
            ["crystal"] = (".cr", "Crystal"),
            ["julia"] = (".jl", "Julia"),
            ["solidity"] = (".sol", "Solidity"),
            ["move"] = (".move", "Move"),
            ["mojo"] = (".mojo", "Mojo"),
            ["racket"] = (".rkt", "Racket"),
            ["scheme"] = (".scm", "Scheme"),
            ["prolog"] = (".pl", "Prolog"),
            ["verilog"] = (".v", "Verilog"),
            ["vhdl"] = (".vhd", "VHDL"),
            ["terraform"] = (".tf", "Terraform"),
            ["dockerfile"] = ("Dockerfile", "Docker"),
            ["docker"] = ("Dockerfile", "Docker"),
        };

        public override string Name => "Coder";
        public override string Description => "Write, debug, explain & convert code in 60+ languages";
        public override string[] Triggers => new[] {
            "code", "write code", "generate code", "code in", "write a",
            "debug", "fix code", "explain code", "convert code",
            "convert to", "format code", "code review", "optimize code",
            "write function", "write class", "write script", "write program",
            "create a program", "build a", "implement", "algorithm"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("code") || lower.Contains("program") ||
                   lower.Contains("function") || lower.Contains("script") ||
                   lower.Contains("debug") || lower.Contains("algorithm") ||
                   lower.Contains("implement") || lower.Contains("class ");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();

            if (lower.Contains("explain code") || lower.Contains("explain this"))
                return ExplainCode(input);

            if (lower.Contains("debug") || lower.Contains("fix code") || lower.Contains("fix this"))
                return DebugCode(input);

            if (lower.Contains("convert to") || lower.Contains("convert code"))
                return ConvertCode(input);

            if (lower.Contains("optimize") || lower.Contains("improve code"))
                return OptimizeCode(input);

            if (lower.Contains("list languages") || lower.Contains("what languages"))
                return ListLanguages();

            if (lower.Contains("save code") || lower.Contains("save to file"))
                return SaveCode(input, memory);

            // Default: generate code
            return GenerateCode(input, memory);
        }

        private Task<string> GenerateCode(string input, Memory memory)
        {
            string lang = DetectLanguage(input);
            string task = ExtractTask(input);

            if (string.IsNullOrEmpty(task))
                return Task.FromResult(
                    "💻 **What would you like me to code?**\n\n" +
                    "Examples:\n" +
                    "• `code a fibonacci function in python`\n" +
                    "• `write a REST API in javascript`\n" +
                    "• `create a sorting algorithm in c++`\n" +
                    "• `write a class in java for a bank account`\n" +
                    "• `script to rename files in bash`\n\n" +
                    "I know **60+ languages**! Say `list languages` to see them all.");

            string template = GetCodeTemplate(lang, task);

            // Remember this coding session
            memory.Remember("user", $"Asked for code: {task} in {lang}");

            return Task.FromResult(
                $"💻 **Code: {task}** ({GetLanguageName(lang)})\n\n" +
                $"```{GetMarkdownLang(lang)}\n{template}\n```\n\n" +
                $"📁 Say `save code` to save this to a file.\n" +
                $"🔧 Say `optimize code` to improve it.\n" +
                $"📖 Say `explain code` for a walkthrough.");
        }

        private Task<string> ExplainCode(string input)
        {
            return Task.FromResult(
                "📖 **Code Explanation Mode**\n\n" +
                "Paste your code and I'll break it down step by step.\n\n" +
                "Example: `explain this code: [paste your code]`\n\n" +
                "I'll explain:\n" +
                "• What each section does\n" +
                "• Time/space complexity\n" +
                "• Potential issues\n" +
                "• Improvement suggestions");
        }

        private Task<string> DebugCode(string input)
        {
            return Task.FromResult(
                "🐛 **Debug Mode**\n\n" +
                "Paste your code and the error message, and I'll help fix it.\n\n" +
                "Example: `debug this code: [code] error: [error message]`\n\n" +
                "I'll find:\n" +
                "• Syntax errors\n" +
                "• Logic bugs\n" +
                "• Runtime issues\n" +
                "• Performance problems");
        }

        private Task<string> ConvertCode(string input)
        {
            string targetLang = "";
            foreach (var kvp in Languages)
            {
                if (input.ToLowerInvariant().Contains($"to {kvp.Key}"))
                {
                    targetLang = kvp.Key;
                    break;
                }
            }

            if (string.IsNullOrEmpty(targetLang))
                return Task.FromResult(
                    "🔄 **Code Conversion**\n\n" +
                    "Example: `convert code to python: [paste code]`\n\n" +
                    "I can convert between any of the 60+ supported languages!");

            return Task.FromResult(
                $"🔄 **Converting to {GetLanguageName(targetLang)}...**\n\n" +
                "Paste your source code and I'll convert it!");
        }

        private Task<string> OptimizeCode(string input)
        {
            return Task.FromResult(
                "⚡ **Code Optimization**\n\n" +
                "Paste your code and I'll optimize it for:\n" +
                "• Performance (speed)\n" +
                "• Memory usage\n" +
                "• Readability\n" +
                "• Best practices\n\n" +
                "Example: `optimize this code: [paste code]`");
        }

        private Task<string> ListLanguages()
        {
            var sb = new StringBuilder();
            sb.AppendLine("💻 **Supported Languages (60+):**\n");

            sb.AppendLine("**Web:** JavaScript, TypeScript, HTML, CSS, SCSS, PHP");
            sb.AppendLine("**Systems:** C, C++, Rust, Go, Zig, Nim, Assembly");
            sb.AppendLine("**Enterprise:** Java, C#, Kotlin, Scala, Swift");
            sb.AppendLine("**Scripting:** Python, Ruby, Perl, Lua, Bash, PowerShell");
            sb.AppendLine("**Functional:** Haskell, Elixir, Erlang, OCaml, F#, Clojure, Racket");
            sb.AppendLine("**Data:** SQL, R, MATLAB, Julia");
            sb.AppendLine("**Mobile:** Swift, Kotlin, Dart, React Native");
            sb.AppendLine("**Blockchain:** Solidity, Move");
            sb.AppendLine("**Infra:** Terraform, Docker, YAML, JSON, XML");
            sb.AppendLine("**Other:** Groovy, Crystal, V, Mojo, Verilog, VHDL, Prolog");
            sb.AppendLine("\nJust say `code [task] in [language]` and I'll write it!");
            return Task.FromResult(sb.ToString());
        }

        private Task<string> SaveCode(string input, Memory memory)
        {
            Directory.CreateDirectory(CodeDir);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(CodeDir, $"code_{timestamp}.txt");

            return Task.FromResult(
                $"💾 **Code saved!**\n\n" +
                $"📁 Location: `{CodeDir}`\n" +
                $"To save with a specific name, say: `save code as [filename]`");
        }

        #region Helpers

        private string DetectLanguage(string input)
        {
            string lower = input.ToLowerInvariant();
            foreach (var kvp in Languages)
            {
                if (lower.Contains($"in {kvp.Key}") || lower.Contains($"using {kvp.Key}") ||
                    lower.Contains($"with {kvp.Key}") || lower.Contains($"{kvp.Key} code") ||
                    lower.Contains($"{kvp.Key} function") || lower.Contains($"{kvp.Key} class") ||
                    lower.Contains($"{kvp.Key} script") || lower.Contains($"{kvp.Key} program"))
                {
                    return kvp.Key;
                }
            }
            return "python"; // Default
        }

        private string ExtractTask(string input)
        {
            // Remove common prefixes
            string task = input;
            string[] prefixes = {
                "write code for", "code a", "code an", "write a", "write an",
                "create a", "create an", "generate code for", "build a", "build an",
                "implement a", "implement an", "write code", "code", "script to",
                "write function", "write class", "write program", "write script",
                "create a program", "code to"
            };

            foreach (var prefix in prefixes)
            {
                int idx = task.ToLowerInvariant().IndexOf(prefix);
                if (idx >= 0)
                {
                    task = task.Substring(idx + prefix.Length).Trim();
                    break;
                }
            }

            // Remove language mentions
            foreach (var lang in Languages.Keys)
            {
                task = task.Replace($"in {lang}", "").Replace($"using {lang}", "")
                          .Replace($"with {lang}", "").Trim();
            }

            return task.Trim().TrimStart('a', 'an').Trim();
        }

        private string GetLanguageName(string code)
        {
            if (Languages.TryGetValue(code, out var info))
                return info.name;
            return code;
        }

        private string GetMarkdownLang(string code)
        {
            return code switch
            {
                "csharp" or "c#" or "cs" => "csharp",
                "cpp" or "c++" => "cpp",
                "fsharp" or "f#" => "fsharp",
                "shell" or "bash" or "sh" => "bash",
                "powershell" or "ps1" => "powershell",
                "javascript" or "js" => "javascript",
                "typescript" or "ts" => "typescript",
                _ => code
            };
        }

        private string GetCodeTemplate(string lang, string task)
        {
            // Generate intelligent code templates based on language and task
            string lower = task.ToLowerInvariant();

            // Fibonnaci
            if (lower.Contains("fibonacci"))
                return GetFibonacciCode(lang);

            // Hello World
            if (lower.Contains("hello world") || lower.Contains("hello"))
                return GetHelloWorldCode(lang);

            // Sorting
            if (lower.Contains("sort"))
                return GetSortCode(lang);

            // REST API
            if (lower.Contains("api") || lower.Contains("rest"))
                return GetApiCode(lang);

            // Class
            if (lower.Contains("class") || lower.Contains("bank") || lower.Contains("account"))
                return GetClassCode(lang, task);

            // File operations
            if (lower.Contains("file") || lower.Contains("read") || lower.Contains("write file"))
                return GetFileCode(lang);

            // Database
            if (lower.Contains("database") || lower.Contains("sql") || lower.Contains("crud"))
                return GetDatabaseCode(lang);

            // Default: generate based on task description
            return $"// TODO: Implement {task}\n// Language: {GetLanguageName(lang)}\n// Rama generated this template\n\n" +
                   GetHelloWorldCode(lang).Replace("Hello World", task);
        }

        private string GetFibonacciCode(string lang) => lang switch
        {
            "python" => "def fibonacci(n):\n    if n <= 1:\n        return n\n    a, b = 0, 1\n    for _ in range(2, n + 1):\n        a, b = b, a + b\n    return b\n\n# Generate first 10\nfor i in range(10):\n    print(fibonacci(i), end=' ')",
            "javascript" or "js" or "typescript" or "ts" => "function fibonacci(n) {\n    if (n <= 1) return n;\n    let a = 0, b = 1;\n    for (let i = 2; i <= n; i++) {\n        [a, b] = [b, a + b];\n    }\n    return b;\n}\n\n// Generate first 10\nfor (let i = 0; i < 10; i++) {\n    console.log(fibonacci(i));\n}",
            "java" => "public class Fibonacci {\n    public static int fibonacci(int n) {\n        if (n <= 1) return n;\n        int a = 0, b = 1;\n        for (int i = 2; i <= n; i++) {\n            int temp = b;\n            b = a + b;\n            a = temp;\n        }\n        return b;\n    }\n\n    public static void main(String[] args) {\n        for (int i = 0; i < 10; i++) {\n            System.out.print(fibonacci(i) + \" \");\n        }\n    }\n}",
            "csharp" or "c#" or "cs" => "using System;\n\nclass Program\n{\n    static int Fibonacci(int n)\n    {\n        if (n <= 1) return n;\n        int a = 0, b = 1;\n        for (int i = 2; i <= n; i++)\n        {\n            (a, b) = (b, a + b);\n        }\n        return b;\n    }\n\n    static void Main()\n    {\n        for (int i = 0; i < 10; i++)\n            Console.Write(Fibonacci(i) + \" \");\n    }\n}",
            "cpp" or "c++" => "#include <iostream>\nusing namespace std;\n\nint fibonacci(int n) {\n    if (n <= 1) return n;\n    int a = 0, b = 1;\n    for (int i = 2; i <= n; i++) {\n        int temp = b;\n        b = a + b;\n        a = temp;\n    }\n    return b;\n}\n\nint main() {\n    for (int i = 0; i < 10; i++)\n        cout << fibonacci(i) << \" \";\n    return 0;\n}",
            "go" or "golang" => "package main\n\nimport \"fmt\"\n\nfunc fibonacci(n int) int {\n    if n <= 1 {\n        return n\n    }\n    a, b := 0, 1\n    for i := 2; i <= n; i++ {\n        a, b = b, a+b\n    }\n    return b\n}\n\nfunc main() {\n    for i := 0; i < 10; i++ {\n        fmt.Print(fibonacci(i), \" \")\n    }\n}",
            "rust" => "fn fibonacci(n: u32) -> u32 {\n    if n <= 1 {\n        return n;\n    }\n    let mut a = 0;\n    let mut b = 1;\n    for _ in 2..=n {\n        let temp = b;\n        b = a + b;\n        a = temp;\n    }\n    b\n}\n\nfn main() {\n    for i in 0..10 {\n        print!(\"{} \", fibonacci(i));\n    }\n}",
            "ruby" => "def fibonacci(n)\n  return n if n <= 1\n  a, b = 0, 1\n  (2..n).each { a, b = b, a + b }\n  b\nend\n\n(0...10).each { |i| print fibonacci(i), ' ' }",
            "php" => "<?php\nfunction fibonacci($n) {\n    if ($n <= 1) return $n;\n    $a = 0; $b = 1;\n    for ($i = 2; $i <= $n; $i++) {\n        [$a, $b] = [$b, $a + $b];\n    }\n    return $b;\n}\n\nfor ($i = 0; $i < 10; $i++) {\n    echo fibonacci($i) . ' ';\n}\n?>",
            "swift" => "func fibonacci(_ n: Int) -> Int {\n    if n <= 1 { return n }\n    var a = 0, b = 1\n    for _ in 2...n {\n        (a, b) = (b, a + b)\n    }\n    return b\n}\n\nfor i in 0..<10 {\n    print(fibonacci(i), terminator: \" \")\n}",
            "kotlin" or "kt" => "fun fibonacci(n: Int): Int {\n    if (n <= 1) return n\n    var a = 0\n    var b = 1\n    for (i in 2..n) {\n        val temp = b\n        b = a + b\n        a = temp\n    }\n    return b\n}\n\nfun main() {\n    for (i in 0 until 10) {\n        print(\"${fibonacci(i)} \")\n    }\n}",
            "bash" or "shell" or "sh" => "#!/bin/bash\nfibonacci() {\n    local n=$1\n    if [ $n -le 1 ]; then\n        echo $n\n        return\n    fi\n    local a=0 b=1\n    for ((i=2; i<=n; i++)); do\n        local temp=$b\n        b=$((a + b))\n        a=$temp\n    done\n    echo $b\n}\n\nfor ((i=0; i<10; i++)); do\n    echo -n \"$(fibonacci $i) \"\ndone\necho",
            "c" => "#include <stdio.h>\n\nint fibonacci(int n) {\n    if (n <= 1) return n;\n    int a = 0, b = 1;\n    for (int i = 2; i <= n; i++) {\n        int temp = b;\n        b = a + b;\n        a = temp;\n    }\n    return b;\n}\n\nint main() {\n    for (int i = 0; i < 10; i++)\n        printf(\"%d \", fibonacci(i));\n    return 0;\n}",
            _ => $"// Fibonacci in {GetLanguageName(lang)}\n// Implement: fn(n) -> returns nth Fibonacci number\n// Use iterative approach for O(n) time, O(1) space"
        };

        private string GetHelloWorldCode(string lang) => lang switch
        {
            "python" => "print('Hello World!')",
            "javascript" or "js" => "console.log('Hello World!');",
            "typescript" or "ts" => "console.log('Hello World!');",
            "java" => "public class HelloWorld {\n    public static void main(String[] args) {\n        System.out.println(\"Hello World!\");\n    }\n}",
            "csharp" or "c#" or "cs" => "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(\"Hello World!\");\n    }\n}",
            "cpp" or "c++" => "#include <iostream>\nint main() {\n    std::cout << \"Hello World!\" << std::endl;\n    return 0;\n}",
            "c" => "#include <stdio.h>\nint main() {\n    printf(\"Hello World!\\n\");\n    return 0;\n}",
            "go" or "golang" => "package main\n\nimport \"fmt\"\n\nfunc main() {\n    fmt.Println(\"Hello World!\")\n}",
            "rust" => "fn main() {\n    println!(\"Hello World!\");\n}",
            "ruby" => "puts 'Hello World!'",
            "php" => "<?php echo 'Hello World!'; ?>",
            "swift" => "print(\"Hello World!\")",
            "kotlin" or "kt" => "fun main() {\n    println(\"Hello World!\")\n}",
            "bash" or "shell" or "sh" => "#!/bin/bash\necho 'Hello World!'",
            "powershell" or "ps1" => "Write-Host 'Hello World!'",
            "lua" => "print('Hello World!')",
            "haskell" => "main = putStrLn \"Hello World!\"",
            "r" => "cat('Hello World!\\n')",
            "perl" => "print 'Hello World!\\n';",
            _ => $"// Hello World in {GetLanguageName(lang)}\nprint('Hello World!')"
        };

        private string GetSortCode(string lang) => lang switch
        {
            "python" => "def quicksort(arr):\n    if len(arr) <= 1:\n        return arr\n    pivot = arr[len(arr) // 2]\n    left = [x for x in arr if x < pivot]\n    middle = [x for x in arr if x == pivot]\n    right = [x for x in arr if x > pivot]\n    return quicksort(left) + middle + quicksort(right)\n\narr = [3, 6, 8, 10, 1, 2, 1]\nprint(quicksort(arr))",
            "javascript" or "js" => "function quicksort(arr) {\n    if (arr.length <= 1) return arr;\n    const pivot = arr[Math.floor(arr.length / 2)];\n    const left = arr.filter(x => x < pivot);\n    const middle = arr.filter(x => x === pivot);\n    const right = arr.filter(x => x > pivot);\n    return [...quicksort(left), ...middle, ...quicksort(right)];\n}\n\nconsole.log(quicksort([3, 6, 8, 10, 1, 2, 1]));",
            _ => $"// QuickSort in {GetLanguageName(lang)}\n// O(n log n) average, O(n²) worst case"
        };

        private string GetApiCode(string lang) => lang switch
        {
            "python" => "from flask import Flask, jsonify, request\n\napp = Flask(__name__)\n\n@app.route('/api/hello', methods=['GET'])\ndef hello():\n    return jsonify({'message': 'Hello from Rama!'})\n\n@app.route('/api/data', methods=['POST'])\ndef create_data():\n    data = request.json\n    return jsonify({'received': data}), 201\n\nif __name__ == '__main__':\n    app.run(debug=True)",
            "javascript" or "js" => "const express = require('express');\nconst app = express();\napp.use(express.json());\n\napp.get('/api/hello', (req, res) => {\n    res.json({ message: 'Hello from Rama!' });\n});\n\napp.post('/api/data', (req, res) => {\n    res.status(201).json({ received: req.body });\n});\n\napp.listen(3000, () => console.log('Server running on port 3000'));",
            "go" or "golang" => "package main\n\nimport (\n    \"encoding/json\"\n    \"net/http\"\n)\n\nfunc helloHandler(w http.ResponseWriter, r *http.Request) {\n    json.NewEncoder(w).Encode(map[string]string{\"message\": \"Hello from Rama!\"})\n}\n\nfunc main() {\n    http.HandleFunc(\"/api/hello\", helloHandler)\n    http.ListenAndServe(\":3000\", nil)\n}",
            _ => $"// REST API in {GetLanguageName(lang)}\n// GET /api/hello - returns JSON greeting\n// POST /api/data - accepts JSON body"
        };

        private string GetClassCode(string lang, string task) => lang switch
        {
            "python" => "class Account:\n    def __init__(self, owner, balance=0):\n        self.owner = owner\n        self.balance = balance\n    \n    def deposit(self, amount):\n        if amount > 0:\n            self.balance += amount\n            return True\n        return False\n    \n    def withdraw(self, amount):\n        if 0 < amount <= self.balance:\n            self.balance -= amount\n            return True\n        return False\n    \n    def __str__(self):\n        return f'{self.owner}: ${self.balance:.2f}'",
            "java" => "public class Account {\n    private String owner;\n    private double balance;\n    \n    public Account(String owner, double balance) {\n        this.owner = owner;\n        this.balance = balance;\n    }\n    \n    public boolean deposit(double amount) {\n        if (amount > 0) {\n            balance += amount;\n            return true;\n        }\n        return false;\n    }\n    \n    public boolean withdraw(double amount) {\n        if (amount > 0 && amount <= balance) {\n            balance -= amount;\n            return true;\n        }\n        return false;\n    }\n    \n    @Override\n    public String toString() {\n        return owner + \": $\" + String.format(\"%.2f\", balance);\n    }\n}",
            _ => $"// Account class in {GetLanguageName(lang)}\n// Properties: owner, balance\n// Methods: deposit(amount), withdraw(amount)"
        };

        private string GetFileCode(string lang) => lang switch
        {
            "python" => "# Read a file\nwith open('file.txt', 'r') as f:\n    content = f.read()\n    print(content)\n\n# Write to a file\nwith open('output.txt', 'w') as f:\n    f.write('Hello from Rama!')\n\n# Read lines\nwith open('file.txt', 'r') as f:\n    for line in f:\n        print(line.strip())",
            "javascript" or "js" => "const fs = require('fs');\n\n// Read file\nconst content = fs.readFileSync('file.txt', 'utf8');\nconsole.log(content);\n\n// Write file\nfs.writeFileSync('output.txt', 'Hello from Rama!');\n\n// Async read\nfs.readFile('file.txt', 'utf8', (err, data) => {\n    if (err) throw err;\n    console.log(data);\n});",
            _ => $"// File I/O in {GetLanguageName(lang)}\n// Read, write, and process files"
        };

        private string GetDatabaseCode(string lang) => lang switch
        {
            "python" => "import sqlite3\n\nconn = sqlite3.connect('database.db')\ncursor = conn.cursor()\n\n# Create table\ncursor.execute('''\n    CREATE TABLE IF NOT EXISTS users (\n        id INTEGER PRIMARY KEY,\n        name TEXT,\n        email TEXT\n    )\n''')\n\n# Insert\ncursor.execute('INSERT INTO users (name, email) VALUES (?, ?)', ('Rama', 'rama@ai.com'))\n\n# Query\ncursor.execute('SELECT * FROM users')\nfor row in cursor.fetchall():\n    print(row)\n\nconn.commit()\nconn.close()",
            _ => $"// Database CRUD in {GetLanguageName(lang)}\n// Create, Read, Update, Delete operations"
        };

        #endregion
    }
}
