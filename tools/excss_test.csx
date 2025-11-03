#!/usr/bin/env dotnet-script
#r "nuget: ExCSS, 4.2.3"

using ExCSS;

var css = @":root {
  --bs-primary: #0d6efd;
  --bs-secondary: #6c757d;
}";

var parser = new StylesheetParser();
var stylesheet = parser.Parse(css);

Console.WriteLine($"Rules count: {stylesheet.StyleRules.Count()}");

foreach (var rule in stylesheet.StyleRules)
{
    Console.WriteLine($"Selector: {rule.SelectorText}");
    Console.WriteLine($"Declarations: {rule.Style.Count()}");
    
    foreach (var declaration in rule.Style)
    {
        Console.WriteLine($"  {declaration.Name} = {declaration.Value}");
    }
}
