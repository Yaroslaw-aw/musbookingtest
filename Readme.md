Сейчас может быть немного витиевато:
для того, чтобы запустить проект, надо сначала создать образо докера, заятя в терминал и написав команду docker build -t musbooking-api .
Затем, после запуска запустить проект из папки, зайти в файл appsettings.json и поменять строку подключения к базе данных с "Host=postgreshost;Username=postgres;Password=example;Database=MUSbooking" на "Host=postgreshost;Username=localhost;Password=example;Database=MUSbooking" - поменять адрес хоста, для того, чтобы создать базу данных.
Затем в терминале прописать dotnet ef database update - создасться база данных.
Теперь можно запускать проект, который запущен в докере и спокойно пользоваться.