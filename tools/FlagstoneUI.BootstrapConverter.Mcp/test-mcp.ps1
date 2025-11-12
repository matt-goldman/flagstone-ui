#!/usr/bin/env pwsh
# Simple test script for the MCP server

$server = Start-Process -FilePath "dotnet" -ArgumentList @(
    "run",
    "--project", "tools/FlagstoneUI.BootstrapConverter.Mcp/FlagstoneUI.BootstrapConverter.Mcp.csproj"
) -NoNewWindow -PassThru -RedirectStandardInput "mcp-test-input.txt" -RedirectStandardOutput "mcp-test-output.txt"

Start-Sleep -Seconds 2

# Send initialize request
$initRequest = @{
    jsonrpc = "2.0"
    id = 1
    method = "initialize"
    params = @{}
} | ConvertTo-Json -Compress

Set-Content -Path "mcp-test-input.txt" -Value $initRequest

Start-Sleep -Seconds 1

# Send tools/list request
$listRequest = @{
    jsonrpc = "2.0"
    id = 2
    method = "tools/list"
    params = @{}
} | ConvertTo-Json -Compress

Add-Content -Path "mcp-test-input.txt" -Value $listRequest

Start-Sleep -Seconds 2

# Stop the server
Stop-Process -Id $server.Id -Force

# Show output
Write-Host "=== MCP Server Output ===" -ForegroundColor Cyan
Get-Content "mcp-test-output.txt"

# Cleanup
Remove-Item "mcp-test-input.txt" -ErrorAction SilentlyContinue
Remove-Item "mcp-test-output.txt" -ErrorAction SilentlyContinue
