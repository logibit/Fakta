call bundle
cd tools
cmd /c start consul agent -dev -bind 127.0.0.1 -config-file=server.json
cd ..
call bundle exec rake
