using ReactComponentFlattener.Core.Services;

// Test utils.ts - has ClassValue type in spread
var testFile = @"E:\repos\flagstone-ui\tools\imported-tools\tools\1-normalization-tool\Test Samples\ai-learning-platform\lib\utils.ts";
var code = File.ReadAllText(testFile);

Console.WriteLine("=== ORIGINAL ===");
Console.WriteLine(code);
Console.WriteLine("\n\n=== STRIPPED ===");

var assembly = typeof(ComponentFlattener).Assembly;
var helperType = assembly.GetType("ReactComponentFlattener.Core.Services.TypeScriptHelper");
var method = helperType!.GetMethod("StripTypeScriptTypes", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
var stripped = (string)method!.Invoke(null, [code])!;

Console.WriteLine(stripped);
