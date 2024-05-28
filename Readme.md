Сейчас может быть немного витиевато:
для того, чтобы запустить проект, надо:
1. Сначала создать образ докера, заядя в проекте в терминал и написав команду "docker build -t musbooking-api ."
2. Затем подняться на папку выше "cd .." и запустить Docker-compose командой "docker compose -f musbooking.yml up -d"
3. Затем в проекте, в файле appsettings.json поменять строку подключения к базе данных с "Host=postgreshost;Username=postgres;Password=example;Database=MUSbooking" на "Host=postgreshost;Username=localhost;Password=example;Database=MUSbooking" - поменять адрес хоста, для того, чтобы создать базу данных.
4. Затем в терминале прописать dotnet ef database update - создасться база данных.
Теперь можно запускать проект, который запущен в докере и пользоваться.