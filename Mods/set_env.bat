@echo off
chcp 65001 >nul
echo ========================================
echo   ValleyBot 环境变量设置工具
echo ========================================
echo.

:input
set /p VALLEYBOT_PATH="请输入 ValleyBot DLL 所在目录的完整路径: "

REM 检查用户是否输入了内容
if "%VALLEYBOT_PATH%"=="" (
    echo [错误] 路径不能为空！
    echo.
    goto input
)

REM 检查路径是否存在
if not exist "%VALLEYBOT_PATH%" (
    echo [警告] 路径不存在: %VALLEYBOT_PATH%
    set /p continue="是否仍要设置此路径？(Y/N): "
    if /i not "%continue%"=="Y" goto input
)

REM 检查 DLL 文件是否存在
if exist "%VALLEYBOT_PATH%\ValleyBot.dll" (
    echo [成功] 找到 ValleyBot.dll
) else (
    echo [警告] 在指定路径中未找到 ValleyBot.dll
)

echo.
echo 当前会话的环境变量已设置为: %VALLEYBOT_PATH%
echo.

REM 询问是否永久设置
set /p permanent="是否要永久设置此环境变量？(Y/N): "
if /i "%permanent%"=="Y" (
    setx VALLEYBOT_PATH "%VALLEYBOT_PATH%"
    echo [完成] 环境变量已永久保存（需要重启 IDE/终端生效）
) else (
    echo [提示] 环境变量仅在当前会话中有效
)

echo.
pause
