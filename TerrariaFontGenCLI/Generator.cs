using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using ReLogic.Content.Pipeline;
using System.Linq;
using System.Text;

namespace TerrariaFontGenCLI
{
    public sealed class Generator : Game
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly GraphicsDeviceManager _graphics;
         
        private static void Main()
        {
            Console.Clear();
            Logo();
            MainMenu();

            
        }
        static void Logo()
        {
            //================================= LOGO =================================
            string resourceName = "TerrariaFontGenCLI.Image.ascii_art.logo.txt";
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.OutputEncoding = Encoding.UTF8; // Ensure console can display UTF-8 characters
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();
                Console.WriteLine(content);
            }
        }
        static void MainMenu()
        {
            const int menuTop = 14;

            while (true)
            {
                // In menu
                Console.SetCursorPosition(0, menuTop);
                Console.ForegroundColor = ConsoleColor.White;
                
                Console.WriteLine("★ SELECT OPTION (PRESS KEY): ★\n");

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[1] Select Language. \n[2] Compile Fonts. \n[3] View Font Load. \n[4] Help. \n");
                Console.ResetColor();
                
                Console.WriteLine("➢ Press Key:");

                // Đặt con trỏ tại dòng nhập
                Console.SetCursorPosition(14, Console.CursorTop - 1);
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                // Xóa vùng menu bằng cách ghi đè trắng lên các dòng
                for (int i = 0; i < 7; i++)
                {
                    Console.SetCursorPosition(0, menuTop + i);
                    Console.Write(new string(' ', 200));
                }

                Console.SetCursorPosition(0, menuTop);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine("You selected [1]: Select Language.");

                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("\n[1] English \n[2] Vietnamese \n[3] Back \n");
                        Console.ResetColor();

                        Console.WriteLine("Press key:");
                        Console.SetCursorPosition(14, Console.CursorTop - 1);

                        ConsoleKeyInfo subKey = Console.ReadKey();

                        if (subKey.Key == ConsoleKey.D3) {
                            int linesToClear1 = 10;
                            for (int i = 0; i < linesToClear1; i++)
                            {
                                Console.SetCursorPosition(0, menuTop + i);
                                Console.Write(new string(' ', Console.BufferWidth));
                            }
                            continue;
                        };
                        break;

                    case ConsoleKey.D2:
                        using (var game = new Generator())
                        {
                            game.Run();
                        }
                        Console.WriteLine("Press Enter to return to main menu...");

                        ConsoleKeyInfo subKey2 = Console.ReadKey();
                        if (subKey2.Key == ConsoleKey.Enter)
                        {
                            int linesToClear1 = 40;
                            for (int i = 0; i < linesToClear1; i++)
                            {
                                int targetLine = menuTop + i;
                                if (targetLine < Console.BufferHeight)
                                {
                                    Console.SetCursorPosition(0, targetLine);
                                    Console.Write(new string(' ', Console.BufferWidth));
                                }
                            }
                            continue;
                        };
                        break;

                    case ConsoleKey.D3:
                        Console.WriteLine("YOU SELECTED [3]: VIEW FONT LOAD.\n");
                        
                        var totalXMLFile = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.xml").Count();
                        var totalXNBFile = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.xnb").Count();
                        
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Total XML files: {totalXMLFile}");
                        Console.WriteLine($"Total XNB files: {totalXNBFile}");
                        Console.ResetColor();

                        break;
                    case ConsoleKey.D4:
                        Console.WriteLine("You selected option 4: 4.");
                        break;
                    default:
                        continue;
                }

                Console.WriteLine("\nPress any key to return to main menu...");
                Console.ReadKey();

                // Xóa nội dung hiện tại trước khi quay lại menu
                int linesToClear = 14;
                for (int i = 0; i < linesToClear; i++)
                {
                    Console.SetCursorPosition(0, menuTop + i);
                    Console.Write(new string(' ', Console.BufferWidth));
                }
            }
        }

        public Generator()
        {
            ReLogicPipeLineAssembly = typeof(DynamicFontDescription).Assembly;
            XnaPipeLineAssembly = typeof(ContentCompiler).Assembly;

            var type = XnaPipeLineAssembly.GetType("Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentCompiler");

            var constructor = type
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .First();
            _compiler = (ContentCompiler)constructor.Invoke(null);
            _compileMethod = type.GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Instance);
            _graphics = new GraphicsDeviceManager(this);
            _context = new DfgContext(this);
            _importContext = new DfgImporterContext();
            _importer = (ContentImporter<DynamicFontDescription>)Activator.CreateInstance(ReLogicPipeLineAssembly.GetType("ReLogic.Content.Pipeline.DynamicFontImporter"));
            _processor = new DynamicFontProcessor();

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            CompileFonts();

            Exit();
            //Environment.Exit(0);
        }

        private void CompileFonts()
        {
            var descFiles = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.xml").ToList();

            Console.WriteLine("Total font files detected: {0}", descFiles.Count);

            foreach (var descFilePath in descFiles)
            {
                var descFileName = Path.GetFileName(descFilePath);

                Console.WriteLine("* {0}", descFileName);
            }

            Console.WriteLine();

            foreach (var descFilePath in descFiles)
            {
                var descFileName = Path.GetFileName(descFilePath);

                Console.Write("Start loading sample font file file: {0}", descFileName);

                var description = _importer.Import(descFilePath, _importContext);
                Console.WriteLine(" ..Done!");

                var fileName = Path.GetFileNameWithoutExtension(descFileName) + ".xnb";

                Console.Write("Start compiling font.");
                var content = _processor.Process(description, _context);
                Console.WriteLine(".Done!");

                Console.Write("Start compiling font content file: {0}", fileName);

                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    _compileMethod.Invoke(_compiler,
                        new object[]
                        {
                            fs, content, TargetPlatform.Windows, GraphicsProfile.Reach, true, Environment.CurrentDirectory,
                            Environment.CurrentDirectory
                        });
                }

                Console.WriteLine(" ..Done!");
                Console.WriteLine();
            }
        }

        private readonly ContentCompiler _compiler;

        private readonly MethodInfo _compileMethod;

        private readonly DfgContext _context;

        private readonly DfgImporterContext _importContext;

        private readonly ContentImporter<DynamicFontDescription> _importer;

        private readonly DynamicFontProcessor _processor;

        public readonly Assembly ReLogicPipeLineAssembly;

        public readonly Assembly XnaPipeLineAssembly;
    }
}
