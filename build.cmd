call bundle
cd tools
cmd /c start consul agent -dev -bind 127.0.0.1 -config-file=server.json
cmd /c start vault server -config=vault.conf
cd ..
call bundle exec rake
set testerrorlevel=%errorlevel%
taskkill /IM consul.exe
taskkill /IM vault.exe
rem echo %testerrorlevel%
if %testerrorlevel% neq 0 exit /b %testerrorlevel%
