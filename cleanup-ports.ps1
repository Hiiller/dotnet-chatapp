# 清理占用端口的 dotnet 进程脚本
# 使用方法: .\cleanup-ports.ps1

Write-Host "正在查找占用端口的 dotnet 进程..." -ForegroundColor Yellow

# 检查常见端口
$ports = @(5000, 5001, 5005, 5006, 5007, 5008, 5009, 5010)
$processesToKill = @()

foreach ($port in $ports) {
    $result = netstat -ano | findstr ":$port" | findstr "LISTENING"
    if ($result) {
        $pid = ($result -split '\s+')[-1]
        if ($pid -and $pid -match '^\d+$') {
            $processesToKill += $pid
            Write-Host "发现端口 $port 被进程 $pid 占用" -ForegroundColor Red
        }
    }
}

if ($processesToKill.Count -eq 0) {
    Write-Host "没有发现占用端口的进程" -ForegroundColor Green
    exit
}

# 去重
$processesToKill = $processesToKill | Select-Object -Unique

Write-Host "`n准备终止以下进程:" -ForegroundColor Yellow
foreach ($pid in $processesToKill) {
    $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "  PID: $pid - $($proc.ProcessName) - $($proc.Path)" -ForegroundColor Cyan
    }
}

$confirm = Read-Host "`n是否终止这些进程? (Y/N)"
if ($confirm -eq 'Y' -or $confirm -eq 'y') {
    foreach ($pid in $processesToKill) {
        try {
            Stop-Process -Id $pid -Force -ErrorAction Stop
            Write-Host "已终止进程 $pid" -ForegroundColor Green
        }
        catch {
            Write-Host "无法终止进程 $pid : $_" -ForegroundColor Red
        }
    }
    Write-Host "`n清理完成!" -ForegroundColor Green
}
else {
    Write-Host "已取消操作" -ForegroundColor Yellow
}

